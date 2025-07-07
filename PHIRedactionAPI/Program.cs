using PHIRedactionAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with proper settings
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PHI Redaction API",
        Version = "v1",
        Description = "API for processing and redacting PHI from lab order documents",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PHI Redaction System"
        }
    });
});

// Add custom services
builder.Services.AddScoped<IPhiRedactionService, PhiRedactionService>();

// Add CORS with more permissive settings for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://frontend:80", "http://localhost:80", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true); // Allow any origin in development
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for this demo
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PHI Redaction API v1");
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    c.DocumentTitle = "PHI Redaction API Documentation";
});

// Configure HTTPS redirection only in production
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngular");
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { status = "Healthy", timestamp = DateTime.UtcNow });

// API test endpoint
app.MapGet("/api/test", () => new { message = "API is working", timestamp = DateTime.UtcNow });

// Root endpoint that redirects to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
