using Balance.API.Data;
using Balance.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddTransient<IEmailService, EmailService>(); // Comentado temporalmente

// Register the DbContext with the PostgreSQL provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// ============================================================
// CONFIGURACIÓN DE UPLOADS 
// ============================================================
string uploadPath;

// Detectar si estamos en Railway (por la variable de entorno o por la existencia de la carpeta)
var isRailway = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_GIT_REPO_NAME"))
                || Directory.Exists("/app/data/uploads");

if (isRailway)
{
    // En Railway, usar el volumen montado
    uploadPath = "/app/data/uploads";
}
else
{
    // En desarrollo local
    uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
}

// Crear la carpeta si no existe
if (!Directory.Exists(uploadPath))
    Directory.CreateDirectory(uploadPath);

// Configurar archivos estáticos
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

// ============================================================
// CONFIGURACIÓN DEL PIPELINE
// ============================================================

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Solo usar HTTPS redirection en desarrollo (Railway maneja SSL externamente)
if (!isRailway)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();