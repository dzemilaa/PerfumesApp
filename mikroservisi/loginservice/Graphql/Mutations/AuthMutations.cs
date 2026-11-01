using HotChocolate;
using HotChocolate.Execution;
using loginservice.Data;
using loginservice.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace loginservice.Graphql.Mutations
{
    public class AuthMutations
    {
        public async Task<string> Register(
            RegisterInput input,
            [Service] AuthDbContext context,
            [Service] IConfiguration config,
            [Service] IHostEnvironment environment)
        {
            var userExists = await context.Users
                .AnyAsync(u => u.Email == input.Email || u.Username == input.Username);

            if (userExists)
                throw new GraphQLException(new Error("User already exists", "USER_EXISTS"));

            var verificationToken = Guid.NewGuid().ToString();

            var user = new Users
            {
                Id = Guid.NewGuid(),
                Username = input.Username,
                Email = input.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
                IsActive = true,
                IsVerified = false,
                VerificationToken = verificationToken
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            try
            {
                await SendVerificationEmail(user.Email, verificationToken, config, environment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EMAIL ERROR: {ex.Message} | {ex.InnerException?.Message}");
            }

            return "User registered successfully! Please check your email to verify your account.";
        }

        public async Task<LoginPayload> Login(
            LoginInput input,
            [Service] AuthDbContext context)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == input.Email);

            if (user == null)
                throw new GraphQLException(new Error("Invalid credentials", "INVALID_CREDENTIALS"));

            bool validPassword = BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash);
            if (!validPassword)
                throw new GraphQLException(new Error("Invalid credentials", "INVALID_CREDENTIALS"));

            if (!user.IsVerified)
                throw new GraphQLException(new Error("Please verify your email", "NOT_VERIFIED"));

            return new LoginPayload
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<string> VerifyEmail(
            string token,
            [Service] AuthDbContext context)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user == null)
                throw new GraphQLException(new Error("Invalid or expired token", "INVALID_TOKEN"));

            user.IsVerified = true;
            await context.SaveChangesAsync();

            return "Email verified successfully!";
        }

        public async Task<UpdateProfilePayload> UpdateProfile(
            UpdateProfileInput input,
            [Service] AuthDbContext context)
        {
            var user = await context.Users.FindAsync(input.UserId);
            if (user == null)
                throw new GraphQLException(new Error("User not found", "USER_NOT_FOUND"));

            bool updated = false;

            if (!string.IsNullOrWhiteSpace(input.Username) && input.Username != user.Username)
            {
                var usernameTaken = await context.Users.AnyAsync(u => u.Username == input.Username && u.Id != input.UserId);
                if (usernameTaken)
                    throw new GraphQLException(new Error("Username already taken", "USERNAME_TAKEN"));
                user.Username = input.Username;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(input.Email) && input.Email != user.Email)
            {
                var emailTaken = await context.Users.AnyAsync(u => u.Email == input.Email && u.Id != input.UserId);
                if (emailTaken)
                    throw new GraphQLException(new Error("Email already taken", "EMAIL_TAKEN"));
                user.Email = input.Email;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password);
                updated = true;
            }

            if (!updated)
                throw new GraphQLException(new Error("Nothing to update", "NO_CHANGES"));

            await context.SaveChangesAsync();

            return new UpdateProfilePayload
            {
                Success = true,
                Message = "Profile updated successfully",
                User = new UserPayload
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsVerified = user.IsVerified
                }
            };
        }

        public async Task<string> DeleteUser(
            Guid userId,
            [Service] AuthDbContext context)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
                throw new GraphQLException(new Error("User not found", "USER_NOT_FOUND"));

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return "User deleted successfully.";
        }

        private async Task SendVerificationEmail(
            string email,
            string token,
            IConfiguration config,
            IHostEnvironment environment)
        {
            var apiKey = config["SendGrid:ApiKey"];
            var fromEmail = config["SendGrid:FromEmail"];
            var fromName = config["SendGrid:FromName"];
            var baseUrl = config["AppSettings:BaseUrl"];

            var verifyUrl = $"{baseUrl}/verify?token={Uri.EscapeDataString(token)}";
            if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine($"Local email verification URL: {verifyUrl}");
                return;
            }

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(fromName))
            {
                throw new InvalidOperationException("SendGrid settings are required to send verification emails.");
            }

            var clientOptions = new SendGridClientOptions
            {
                ApiKey = apiKey,
                HttpErrorAsException = true
            };
            var client = new SendGridClient(clientOptions);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(email);

            var subject = "Verify your email";
            var htmlContent = $@"
                <h2>Email verification</h2>
                <p>Click the button below to verify your email:</p>
                <a href='{verifyUrl}' 
                   style='padding:10px 20px; background-color:#d63384; color:white; text-decoration:none; border-radius:5px;'>
                   Verify Email
                </a>
                <p>If you did not create this account, ignore this email.</p>
            ";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception($"Email sending failed. Status: {response.StatusCode}. Body: {body}");
            }

            Console.WriteLine($"Verification email sent to {email}.");
        }
    }

    public class RegisterInput
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginInput
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginPayload
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class UpdateProfileInput
    {
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class UpdateProfilePayload
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserPayload User { get; set; }
    }

    public class UserPayload
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
    }
}