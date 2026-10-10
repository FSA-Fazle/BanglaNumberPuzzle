var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UsePathBase("/number");
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapGet("/healthz", () => "ok");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Game}/{action=Index}/{id?}");

app.Run();
