using Resume.Application;
using Resume.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

NormalizeKnowledgeBasePath(builder);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

static void NormalizeKnowledgeBasePath(WebApplicationBuilder builder)
{
    var path = builder.Configuration["KnowledgeBase:Path"];

    if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
    {
        return;
    }

    builder.Configuration["KnowledgeBase:Path"] =
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, path));
}
