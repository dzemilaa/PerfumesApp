using loginservice.Data;
using loginservice.Graphql.NewFolder;
using loginservice.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
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

// DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// SMTP
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings")
);

// GraphQL
builder.Services
    .AddGraphQLServer()
     .AddQueryType<AuthQuery>()
    .AddMutationType<loginservice.Graphql.Mutations.AuthMutations>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await db.Database.EnsureCreatedAsync();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(MyAllowSpecificOrigins);

// ROOT → GraphQL
app.MapGet("/", () => Results.Redirect("/graphql"));

// GraphQL endpoint
app.MapGraphQL("/graphql");

app.Run();
