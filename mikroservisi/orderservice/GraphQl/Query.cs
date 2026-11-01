﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using orderservice.Data;
using orderservice.Models;
using HotChocolate;

namespace OrderService.GraphQL
{
    public class Query
    {
        private readonly OrdersDbContext _dbContext;

        public Query(OrdersDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [GraphQLName("getOrders")]
        public async Task<IReadOnlyList<OrderView>> GetOrders(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                throw new ArgumentException("Invalid userId format.");

            var orders = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userGuid)
                .ToListAsync();

            if (!orders.Any())
                return Array.Empty<OrderView>();

            return orders.Select(o => new OrderView(
                o.Id,
                o.UserId,
                o.OrderDate,
                o.Status,
                o.TotalAmount,
                o.FirstName,
                o.LastName,
                o.Email,
                o.Phone,
                o.Address,
                o.Image,
                 o.PromoCode,
                o.OrderItems.Select(oi => new OrderItemView(
                    oi.ProductId,
                    oi.Price,
                    oi.Quantity,
                    oi.Image
                )).ToList()
            )).ToList();
        }

        [GraphQLName("getOrderById")]
        public async Task<OrderView?> GetOrderById(Guid orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            return new OrderView(
                order.Id, order.UserId, order.OrderDate, order.Status, order.TotalAmount,
                order.FirstName, order.LastName, order.Email, order.Phone, order.Address,
                order.Image,order.PromoCode,
                order.OrderItems.Select(oi => new OrderItemView(
                    oi.ProductId, oi.Price, oi.Quantity, oi.Image
                )).ToList()
            );
        }

        [GraphQLName("getAllOrders")]
        public async Task<IReadOnlyList<OrderView>> GetAllOrders()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();

            if (!orders.Any())
                return Array.Empty<OrderView>();

            return orders.Select(o => new OrderView(
                o.Id, o.UserId, o.OrderDate, o.Status, o.TotalAmount,
                o.FirstName, o.LastName, o.Email, o.Phone, o.Address,
                o.Image, o.PromoCode,
                o.OrderItems.Select(oi => new OrderItemView(
                    oi.ProductId, oi.Price, oi.Quantity, oi.Image
                )).ToList()
            )).ToList();
        }
    }

}