using SentinelMES.Application;
using SentinelMES.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

//  MİMARİ BAĞLANTILAR: Diğer katmanlardaki IoC kayıtlarını çağırıyoruz
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

app.UseAuthentication(); // Önce kimlik sor!
app.UseAuthorization();  // Sonra yetkisine bak!

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Geliştirme ortamında sertifika hatalarını önlemek için UseHttpsRedirection devre dışı bırakıldı.

//  KESİN KURAL: CORS politikası Authorization ve MapControllers'tan ÖNCE tetiklenmelidir!
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();