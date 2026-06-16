using SentinelMES.Application;
using SentinelMES.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🧠 MİMARİ BAĞLANTILAR: Diğer katmanlardaki IoC kayıtlarını çağırıyoruz
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 📡 CORS POLİTİKASI: Dashboard (WebUI) uygulamasının API'ye pürüzsüz bağlanabilmesi için köprü
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Geliştirme ortamında sertifika hatalarını önlemek için UseHttpsRedirection devre dışı bırakıldı.

// 🚨 KESİN KURAL: CORS politikası Authorization ve MapControllers'tan ÖNCE tetiklenmelidir!
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();