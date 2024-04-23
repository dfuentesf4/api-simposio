using PdfSharp.Fonts;
using simposio.Services.BDConecction;
using simposio.Services.DAO;
using simposio.Services.Email;
using simposio.Services.PDF;

var options = new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot" // Define expl�citamente la ruta a wwwroot si es necesario
};


var builder = WebApplication.CreateBuilder(options);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder =>
            policyBuilder.WithOrigins("http://localhost:4200") // Aqu� pones los or�genes que necesites
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // A�ade AllowCredentials si necesitas soportar cookies, etc.
});

GlobalFontSettings.FontResolver = new FontResolver(builder.Environment.ContentRootPath);

// Add services to the container.

builder.Services.AddControllers();

//Settings to use PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("AzurePostgreSQL");
builder.Services.AddSingleton(new PostgreSQLConfiguration(connectionString));

//Settings to use Email
SMPTConfig smtpConfig = builder.Configuration.GetSection("SMTP").Get<SMPTConfig>();
builder.Services.AddSingleton(smtpConfig);
builder.Services.AddSingleton<EmailService>();

builder.Services.AddScoped<DatabaseConnectionService>(); 
builder.Services.AddScoped<ParticipanteDAO>();
builder.Services.AddScoped<MerchandisingDAO>();
builder.Services.AddScoped<ExpositorDAO>();
builder.Services.AddScoped<ColaboradorDAO>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
