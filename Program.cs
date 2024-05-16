using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PdfSharp.Fonts;
using simposio.Services.BDConecction;
using simposio.Services.DAO;
using simposio.Services.Email;
using simposio.Services.PDF;
using System.Text;

var options = new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot"
};


var builder = WebApplication.CreateBuilder(options);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder =>
            policyBuilder.WithOrigins("http://localhost:4200") // Aquí pones los orígenes que necesites
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Añade AllowCredentials si necesitas soportar cookies, etc.
});

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
