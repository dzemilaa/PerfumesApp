using Microsoft.EntityFrameworkCore;
using reviewservice.Data;
using reviewservice.GraphQL;
using reviewservice.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    var orderServiceUrl = builder.Configuration["Services:OrderGraphQLUrl"]
        ?? (builder.Environment.IsDevelopment()
            ? "http://localhost:5003/graphql"
            : "https://orderservice-production-4f8d.up.railway.app/graphql/");
    client.BaseAddress = new Uri(orderServiceUrl);
});

builder.Services.AddDbContext<ReviewsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174",
                "https://bellaperfumes-s-iproject.vercel.app",
                "https://bellaperfumes-s-iproject-dd56.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .ModifyCostOptions(opt => opt.EnforceCostLimits = false);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ReviewsDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.MapGraphQL();
app.Run();