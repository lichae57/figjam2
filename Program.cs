var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpClient for API calls
builder.Services.AddHttpClient<figjam2.Services.ApiClient>();
builder.Services.AddScoped<figjam2.Services.ApiClient>();

// Add Cookie Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only use HTTPS redirection if HTTPS is configured
var httpsPort = builder.Configuration["HTTPS_PORT"] ?? 
                Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
if (!string.IsNullOrEmpty(httpsPort) || app.Configuration.GetValue<bool>("HTTPS:Enabled", false))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

// Enable request buffering for reading request body multiple times
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
