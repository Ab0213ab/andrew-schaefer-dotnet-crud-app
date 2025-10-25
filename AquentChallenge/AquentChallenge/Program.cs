using AquentChallenge.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/aquent.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(ctx.Configuration)
);


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
// Note: The comments here may seem excessive, but I need
// to keep them to explain the purpose of each middleware.
if (!app.Environment.IsDevelopment())
{
    // Intercepts exceptions anywhere in the pipeline and redirects to the Error action
    // Saves us from having to put try/catch blocks everywhere in the project
    // Works with serilog so that exceptions are auto-logged
    app.UseExceptionHandler("/Error/Error");

    // .NET convention for production security
    app.UseHsts();
}

// Catches non-exception status codes (i.e., 404, 403, 500) and re-executes a controller action to show a clean error page.
app.UseStatusCodePagesWithReExecute("/Error/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();