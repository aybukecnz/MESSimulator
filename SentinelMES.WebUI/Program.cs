var builder = WebApplication.CreateBuilder(args);

// 1. ÝÞTE HAYAT KURTARAN SATIR (ZAP'ýn çökerttiði View motorunu ayaða kaldýrýr)
builder.Services.AddControllersWithViews();

// 2. HomeController'da _httpClient kullanabilmen için gerekli servis
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets(); // .NET 9.0 statik dosya yöneticisi
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}")
    .WithStaticAssets();

app.Run();