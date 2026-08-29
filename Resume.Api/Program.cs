using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Resume.Application;
using Resume.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

NormalizeKnowledgeBasePath(builder);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(exceptionHandlerApp =>
    exceptionHandlerApp.Run(async context =>
    {
        IExceptionHandlerFeature? exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

        if (exceptionFeature?.Error is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            string error = JsonSerializer.Serialize(new { error = validationException.Message });
            await context.Response.WriteAsync(error);
        }
    }));

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

static void NormalizeKnowledgeBasePath(WebApplicationBuilder builder)
{
    string? path = builder.Configuration["KnowledgeBase:Path"];

    if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
    {
        return;
    }

    builder.Configuration["KnowledgeBase:Path"] =
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, path));
}
