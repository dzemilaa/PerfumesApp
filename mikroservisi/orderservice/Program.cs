using Microsoft.EntityFrameworkCore;
using orderservice;
using orderservice.Data;
using OrderService.GraphQL;
using HotChocolate;
using HotChocolate.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
{
    var shopServiceUrl = builder.Configuration["Services:ShopGraphQLUrl"]
        ?? (builder.Environment.IsDevelopment()
            ? "http://localhost:5002/graphql"
            : "https://shopservice-production.up.railway.app/graphql/");
    client.BaseAddress = new Uri(shopServiceUrl);
});

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<Mutation>(); 
builder.Services.AddScoped<Query>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174",
                "https://bellaperfumes-s-iproject.vercel.app",
                "https://bellaperfumes-s-iproject-dd56.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
               .AllowCredentials();

    });
});

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors("AllowLocalhost");
app.MapGraphQL();


app.Run();