using Supabase;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Adicionar servi�os
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona o appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Obt�m os dados de conex�o do Supabase
string? supabaseUrl = builder.Configuration["Supabase:Url"];
string? supabaseKey = builder.Configuration["Supabase:Key"];

Client? supabaseClient = null;

if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
{
    var supabaseOptions = new SupabaseOptions();
    supabaseClient = new Client(supabaseUrl, supabaseKey, supabaseOptions);

    // Registra o Supabase como um servi�o no DI (caso precise injet�-lo em Controllers)
    builder.Services.AddSingleton(supabaseClient);
}

var app = builder.Build();

// Configura��o do pipeline de requisi��es
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
