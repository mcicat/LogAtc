using LogAtc.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
await CreateNewDefaultUserIfNeededAsync(app);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

async Task CreateNewDefaultUserIfNeededAsync(WebApplication app)
{
    using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (userManager.Users.Any(x => x.Email == "admin@atclog.com"))
    {
        return;
    }
    var user = new ApplicationUser
    {
        UserName = "admin@atclog.com",
        Email = "admin@atclog.com",
        EmailConfirmed = true,
        NormalizedEmail = "admin@atclog.com",
        NormalizedUserName = "admin@atclog.com",
        TwoFactorEnabled = false
    };

    var result = await userManager.CreateAsync(user, "Atc123...");
    if (!result.Succeeded)
    {
        throw new InvalidOperationException("Default user cannot be created");
    }
}
