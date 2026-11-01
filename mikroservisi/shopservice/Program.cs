using Microsoft.EntityFrameworkCore;
using ShopService.Data;
using ShopService.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    var productServiceUrl = builder.Configuration["Services:ProductGraphQLUrl"]
        ?? (builder.Environment.IsDevelopment()
            ? "http://localhost:5001/graphql"
            : "https://productservice-production-c2fd.up.railway.app/graphql");
    client.BaseAddress = new Uri(productServiceUrl);
});

builder.Services.AddDbContextFactory<ShopDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
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
    .ModifyRequestOptions(opt =>
    {
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    });//detalji o gresci

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    await db.Database.EnsureCreatedAsync();
}
else
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("FrontendPolicy");
app.UseAuthorization();
app.MapGraphQL("/graphql");
app.Run();