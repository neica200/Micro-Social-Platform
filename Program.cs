using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Micro_social_app.Models; 


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

//Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//NPGSQL    
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
      
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

//Identity si roluri
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() 
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRoles.InitializeAsync(services);
}


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapRazorPages();

app.Run();
