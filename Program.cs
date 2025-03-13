using Supabase;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona o appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Obtém os dados de conexão do Supabase
string? supabaseUrl = builder.Configuration["Supabase:Url"];
string? supabaseKey = builder.Configuration["Supabase:Key"];

Client? supabaseClient = null;

if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
{
    var supabaseOptions = new SupabaseOptions();
    supabaseClient = new Client(supabaseUrl, supabaseKey, supabaseOptions);

    // Registra o Supabase como um serviço no DI (caso precise injetá-lo em Controllers)
    builder.Services.AddSingleton(supabaseClient);
}

var app = builder.Build();

// Configuração do pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
