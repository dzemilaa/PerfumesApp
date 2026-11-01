using Microsoft.EntityFrameworkCore;
using orderservice;
using orderservice.Data;
using orderservice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        private readonly IShopServiceClient _shopClient;
        private readonly IConfiguration _config;
        private readonly OrdersDbContext _dbContext;

        public Mutation(IShopServiceClient shopClient, IConfiguration config, OrdersDbContext dbContext)
        {
            _shopClient = shopClient;
            _config = config;
            _dbContext = dbContext;
        }

        public async Task<OrderResult> CreateOrder(
            [GraphQLNonNullType] string userId,
            string firstName,
            string lastName,
            string email,
            string phone,
            string address,
            string? promoCode)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return new OrderResult(false, "Invalid userId.", null);

            var cartItems = await _shopClient.GetCartItems(userGuid);
            if (!cartItems.Any())
                return new OrderResult(false, "Cart is empty.", null);

            // Validacija promo koda
            string? validatedPromoCode = null;
            if (promoCode == "DISCOUNT10")
                validatedPromoCode = "DISCOUNT10";

            var order = new Orders
            {
                Id = Guid.NewGuid(),
                UserId = userGuid,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = 0,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Address = address,
                PromoCode = validatedPromoCode
            };

            foreach (var item in cartItems)
            {
                order.OrderItems.Add(new OrderItems
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Image = item.Image,
                    OrderId = order.Id
                });

                order.TotalAmount += item.Price * item.Quantity;
            }

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            await _shopClient.ClearCart(userGuid);

            var previousOrders = await _dbContext.Orders
                .Where(o => o.UserId == userGuid)
                .CountAsync();

            if (previousOrders == 1)
            {
                try
                {
                    await SendDiscountEmail(order.Email, order.FirstName);
                    Console.WriteLine($"Discount email sent to {order.Email}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Send discount email failed: " + ex.Message);
                }
            }

            var orderView = MapToOrderView(order);
            return new OrderResult(true, "Order created.", orderView);
        }

        private async Task SendDiscountEmail(string email, string? firstName)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var fromEmail = _config["SendGrid:FromEmail"];
            var fromName = _config["SendGrid:FromName"];

            if (string.Equals(
                    _config["ASPNETCORE_ENVIRONMENT"],
                    "Development",
                    StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("Local development: skipped optional first-order email.");
                return;
            }

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(fromName))
            {
                throw new InvalidOperationException("SendGrid settings are required to send first-order emails.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(email);

            var subject = "Your First Order Reward! Use DISCOUNT10";
            var htmlContent = $@"
        <h2>Hello {firstName ?? "Customer"}!</h2>
        <p>Thank you for placing your first order with us. As a welcome gift, here is a promo code for 10% off your next order:</p>
        <h3 style='color:#d63384;'>DISCOUNT10</h3>
        <p>Use it at checkout on your next order to receive 10% off your total!</p>
        <p>Hurry, the code is valid for 30 days.</p>
        <p>Happy shopping!<br/>Bella Perfumes Team</p>
    ";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Discount email sending failed.");
        }

        private OrderView MapToOrderView(Orders order)
        {
            var itemsView = order.OrderItems.Select(oi => new OrderItemView(
                oi.ProductId,
                oi.Price,
                oi.Quantity,
                oi.Image
            )).ToList();

            return new OrderView(
                order.Id,
                order.UserId,
                order.OrderDate,
                order.Status,
                order.TotalAmount,
                order.FirstName,
                order.LastName,
                order.Email,
                order.Phone,
                order.Address,
                order.Image,
                order.PromoCode,
                itemsView
            );
        }

        public async Task<OrderResult> UpdateOrderStatus(
            [GraphQLNonNullType] string orderId,
            [GraphQLNonNullType] string newStatus)
        {
            if (!Guid.TryParse(orderId, out var orderGuid))
                return new OrderResult(false, "Invalid orderId.", null);

            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderGuid);

            if (order == null)
                return new OrderResult(false, "Order not found.", null);

            order.Status = newStatus;
            await _dbContext.SaveChangesAsync();

            return new OrderResult(true, "Status updated.", MapToOrderView(order));
        }
    }

    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public OrderView? Order { get; set; }

        public OrderResult() { }
        public OrderResult(bool success, string message, OrderView? order)
        {
            Success = success;
            Message = message;
            Order = order;
        }
    }

    public class OrderView

    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Image { get; set; }
        public string? PromoCode { get; set; }

        public List<OrderItemView> OrderItems { get; set; } = new();

        public OrderView(Guid id, Guid userId, DateTime orderDate, string status, decimal totalAmount,
                         string? firstName, string? lastName, string? email, string? phone, string? address,
                         string? image, string? promoCode, List<OrderItemView> orderItems)
        {
            Id = id;
            UserId = userId;
            OrderDate = orderDate;
            Status = status;
            TotalAmount = totalAmount;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            Address = address;
            Image = image;
            PromoCode = promoCode;
            OrderItems = orderItems;
        }
    }

    public class OrderItemView
    {
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }

        public OrderItemView(Guid productId, decimal price, int quantity, string? image)
        {
            ProductId = productId;
            Price = price;
            Quantity = quantity;
            Image = image;
        }
    }
}