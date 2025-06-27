using Microsoft.EntityFrameworkCore;
using EveAuthApi;
using IndyBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs explicitly - HTTP on 5678, HTTPS on 5679
builder.WebHost.UseUrls("http://localhost:5678", "https://localhost:5679");

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=EveAuth.db"));

// Add JWT Service
builder.Services.AddScoped<JwtService>();

// Add HTTP Service
builder.Services.AddScoped<IHttpService, HttpService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.UseRouting();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Removed to avoid warning. TODO: configure properly
//app.UseHttpsRedirection();

app.Run();
