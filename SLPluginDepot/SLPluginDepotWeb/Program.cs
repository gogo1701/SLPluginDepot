using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SLPluginDepotModels.Models;
using SLPluginDepotDB;
using SLPluginDepotServices.Services;
using SLPluginDepotServices.Interfaces;

namespace SLPluginDepotWeb;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddDefaultTokenProviders().AddDefaultUI();

        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped<IPluginService, PluginService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}
