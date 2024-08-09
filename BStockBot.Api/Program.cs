using BStockBot.Api.Data;
using BStockBot.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddDbContext<BStockContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("Main")));
builder.Services.AddScoped<ReportGenerationService>();
builder.Services.AddControllers();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();
