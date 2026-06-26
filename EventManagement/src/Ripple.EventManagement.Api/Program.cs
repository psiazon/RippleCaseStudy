using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Ripple.EventManagement.Application.Common;
using Ripple.EventManagement.Application.Events;
using Ripple.EventManagement.Infrastructure;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console().WriteTo.File("logs/event-api-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddEventInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateEventCommand).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateEventValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("fixed", limiter => { limiter.PermitLimit = 100; limiter.Window = TimeSpan.FromMinutes(1); limiter.QueueLimit = 0; }));

//builder.Services.AddCors(options => options.AddPolicy("TrustedClients", policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>()).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:7001", "https://localhost:7001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 5_242_880);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 5_242_880);

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageEvents", policy => policy.RequireRole("Admin", "EventManager"));
    options.AddPolicy("CanReadEvents", policy => policy.RequireRole("Admin", "EventManager", "TicketAgent", "Customer"));
});

builder.Services.AddHttpClient("TicketInventory", client =>
{
    client.BaseAddress = new Uri("https://localhost:6001/");
    // configure timeouts/headers as needed
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSerilogRequestLogging();
app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Event API v1");
});

app.MapControllers();
app.UseHttpsRedirection();
//app.UseCors("TrustedClients");
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("fixed");
app.MapHealthChecks("/health");
app.Run();

public partial class Program { }
