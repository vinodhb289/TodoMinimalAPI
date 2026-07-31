using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(options =>
    options.UseInMemoryDatabase("TodoList"));
builder.Services.AddOpenApi();

builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

var app = builder.Build();
app.UseRequestLogging();
app.UseIpRateLimiting();
app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapPost("/todoitems", async (TodoDb db, Todo todo)
=>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
});

app.MapGet("/", () => "This is a Rate Limited API!");
app.MapGet("/hello", () => "Hello World");
app.Run();


