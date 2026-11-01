using HotChocolate.Types.Pagination;
using Microsoft.EntityFrameworkCore;
using productservice.Data;
using productservice.GraphQL;
using HotChocolate;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyCostOptions(opt => {
        opt.EnforceCostLimits = false;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
    await db.Database.EnsureCreatedAsync();
    app.MapOpenApi();
}

app.UseCors("AllowAll");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var uploadRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "perfumes-product-uploads");
Directory.CreateDirectory(uploadRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads"
});

app.MapGraphQL("/graphql");

app.MapPost("/api/upload", async (HttpRequest request, IConfiguration config) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("Invalid content type.");

    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");

    if (file == null || file.Length == 0)
        return Results.BadRequest("No file uploaded.");

    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
    if (!allowedTypes.Contains(file.ContentType))
        return Results.BadRequest("Invalid file type.");

    var cloudName = config["Cloudinary:CloudName"];
    var apiKey = config["Cloudinary:ApiKey"];
    var apiSecret = config["Cloudinary:ApiSecret"];
    if (app.Environment.IsDevelopment() &&
        (string.IsNullOrWhiteSpace(cloudName) ||
         string.IsNullOrWhiteSpace(apiKey) ||
         string.IsNullOrWhiteSpace(apiSecret)))
    {
        var extension = file.ContentType == "image/png" ? ".png" : ".jpg";
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var output = File.Create(System.IO.Path.Combine(uploadRoot, fileName));
        await file.CopyToAsync(output);

        var publicBaseUrl = config["AppSettings:PublicBaseUrl"] ?? "http://localhost:5001";
        return Results.Ok(new { imageUrl = $"{publicBaseUrl.TrimEnd('/')}/uploads/{fileName}" });
    }

    if (string.IsNullOrWhiteSpace(cloudName) ||
        string.IsNullOrWhiteSpace(apiKey) ||
        string.IsNullOrWhiteSpace(apiSecret))
    {
        return Results.Problem(
            "Cloud image storage is not configured.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var account = new Account(
        cloudName,
        apiKey,
        apiSecret
    );
    var cloudinary = new Cloudinary(account);

    using var stream = file.OpenReadStream();
    var uploadParams = new ImageUploadParams
    {
        File = new FileDescription(file.FileName, stream),
        Folder = "bella-perfumes"
    };

    var uploadResult = await cloudinary.UploadAsync(uploadParams);

    if (uploadResult.Error != null)
        return Results.BadRequest(uploadResult.Error.Message);

    return Results.Ok(new { imageUrl = uploadResult.SecureUrl.ToString() });
});

app.Run();