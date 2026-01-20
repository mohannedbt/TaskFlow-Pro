using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Repositories;
using TaskFlow_Pro.Repositories.Implementations;
using TaskFlow_Pro.Repositories.Interfaces;
using TaskFlow_Pro.Services;
using TaskFlow_Pro.Services.Implementations;
using TaskFlow_Pro.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ===== DbContext =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// ===== Identity =====
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository,TaskRepository>();
builder.Services.AddScoped<ITeamRepository,TeamRepository>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<StartUpService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LoginPath = "/Account/Login";
});


// ===== MVC + Razor Pages =====
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();
await StartUpService.SeedRolesAsync(app.Services);

// ===== Middleware pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ ORDER MATTERS
app.UseAuthentication();
app.UseAuthorization();

// ===== Endpoints =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Needed for Identity UI

app.Run();