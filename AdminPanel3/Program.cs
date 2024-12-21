using AdminPanel3.Models;  // Adjust this to match your project's namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (ConfigureServices equivalent).
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

// Register the ApplicationDbContext with the connection string
// object value = builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline (Configure equivalent).

if (!app.Environment.IsDevelopment())
{
    // Production environment-specific configurations
    app.UseExceptionHandler("/Home/Error");

    // Optionally configure HSTS (HTTP Strict Transport Security)
    app.UseHsts();

    // You can also add logging or other middleware here for production-only
}

// Middleware common for all environments
app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable session middleware (this must be before routing or any controller usage)
app.UseSession();

app.UseRouting();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Route configuration (Controller actions).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
