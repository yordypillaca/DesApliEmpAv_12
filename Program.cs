using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using TallerApi.Models;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddOpenApi();

var connectionString = NormalizarConexion(
    builder.Configuration.GetConnectionString("Taller")
    ?? builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("Defina la cadena de conexión 'Taller'."));

builder.Services.AddDbContext<TallerContext>(options =>
    options.UseNpgsql(connectionString));

var origen = builder.Configuration["Cors:Origin"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
    options.AddPolicy("Front", policy => policy
        .WithOrigins(origen)
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "TallerApi");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));
if (string.IsNullOrWhiteSpace(port))
    app.UseHttpsRedirection();
app.UseCors("Front");
app.UseAuthorization();
app.MapControllers();

await PrepararBase(app);
app.Run();

static async Task PrepararBase(WebApplication app)
{
    var ruta = Path.Combine(app.Environment.ContentRootPath, "TallerDB.sql");
    if (!File.Exists(ruta))
        return;

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<TallerContext>();
    var conexion = db.Database.GetDbConnection();
    await conexion.OpenAsync();
    try
    {
        await using var consulta = conexion.CreateCommand();
        consulta.CommandText = """SELECT to_regclass('public."Clientes"')""";
        var tabla = await consulta.ExecuteScalarAsync();
        if (tabla is not null and not DBNull)
            return;

        await using var comando = conexion.CreateCommand();
        comando.CommandText = await File.ReadAllTextAsync(ruta);
        await comando.ExecuteNonQueryAsync();
    }
    finally
    {
        await conexion.CloseAsync();
    }
}

static string NormalizarConexion(string raw)
{
    if (!raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        && !raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        return raw;

    var uri = new Uri(raw);
    var info = uri.UserInfo.Split(':', 2);
    var usuario = Uri.UnescapeDataString(info[0]);
    var clave = info.Length > 1 ? Uri.UnescapeDataString(info[1]) : "";
    var baseDatos = uri.AbsolutePath.Trim('/');
    var puerto = uri.IsDefaultPort ? 5432 : uri.Port;
    var ssl = uri.Host.Contains('.') ? "Require" : "Prefer";

    return $"Host={uri.Host};Port={puerto};Database={baseDatos};Username={usuario};Password='{clave.Replace("'", "''")}';SSL Mode={ssl};Trust Server Certificate=true";
}
