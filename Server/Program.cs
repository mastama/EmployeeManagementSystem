using Microsoft.EntityFrameworkCore;
using ServiceLibrary.Data;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.Contracts;
using ServiceLibrary.Repositories.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Tambahkan layanan Swagger ke DI container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Konfigurasi connect db
builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
                             throw new InvalidOperationException("Could not find a connection string."));
    });

// Konfigurasi JWT
builder.Services.Configure<JwtSection>(builder.Configuration.GetSection("JwtSection"));
// Konfigurasi lainnya
builder.Services.AddScoped<IUserAccount, UserAccountRepository>();
// Allow CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm",
        builder => builder
            .WithOrigins("https://localhost:7130", "http://localhost:5298") //from properties client
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
// Layanan untuk Controller
builder.Services.AddControllers();

var app = builder.Build();

// Tambahkan middleware Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorWasm");
app.MapControllers();
app.Run();