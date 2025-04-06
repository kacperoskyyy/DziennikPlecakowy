using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Services;
using DziennikPlecakowy.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DziennikPlecakowyDbContext>();

builder.Services.Configure<CypherServiceOptions>(options =>
{
    options.Key = "1234567890123456";
    options.IV = "1234567890123456";
});


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<ICypherService, CypherService>();
builder.Services.AddScoped<IMountainService, MountainService>();
builder.Services.AddScoped<ITripService, TripService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
