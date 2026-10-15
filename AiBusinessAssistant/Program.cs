using AiBusinessAssistant.Data;
using AiBusinessAssistant.Extensions;
using AiBusinessAssistant.Middleware;
using AiBusinessAssistant.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<AiBusinessAssistant.Validation.Order.CreateOrderDtoValidator>();
builder.Services.AddScoped<FluentValidationActionFilter>();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
    options.Filters.AddService<FluentValidationActionFilter>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register your DbContext with SQLite
builder.Services.AddDbContext<BusinessAiDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApplicationServices(); // Register application services

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
