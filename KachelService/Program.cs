using EasyJwt;
using KachelService;
using KachelService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KachelService", Version = "v1" });

    // add JWT Authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] { }}
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

Configuration.Muenchen = builder.Configuration.GetValue<string>("Filter:Muenchen");
Configuration.Leipzig = builder.Configuration.GetValue<string>("Filter:Leipzig");
Configuration.Essen = builder.Configuration.GetValue<string>("Filter:Essen");

Configuration.CustomerService = builder.Configuration.GetValue<string>("Kachel:CustomerService");
Configuration.UrlStg = builder.Configuration.GetValue<string>("Kachel:UrlStg");
Configuration.UrlPv = builder.Configuration.GetValue<string>("Kachel:UrlPv");
Configuration.UrlPvLive = builder.Configuration.GetValue<string>("Kachel:UrlPvlive");
Configuration.UseAES = builder.Configuration.GetValue<bool>("Kachel:UseAES");

builder.Services.AddEasyJwt(builder.Configuration);

builder.Services.AddSingleton<FilterService>();
builder.Services.AddSingleton<LinkService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<TileService>();

bool useSwagger = builder.Configuration.GetValue<bool>("Kachel:Swagger");
string swaggerEndpoint = builder.Configuration.GetValue<string>("Kachel:SwaggerEndpoint");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    if (useSwagger)
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint(swaggerEndpoint, "KachelService v1"));
    }
}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
