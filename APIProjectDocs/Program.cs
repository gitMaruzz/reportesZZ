using APIProjectDocs.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static APIProjectDocs.Models.Data;
using static APIProjectDocs.Services.Service;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuraci�n de JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurado"));

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = builder.Environment.IsProduction();
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// HttpClient para APIs externas
builder.Services.AddHttpClient();

// Servicios de la aplicaci�n
builder.Services.AddScoped<IDataSourceService, DataSourceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPlataformaService, PlataformaService>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IEntregableService, EntregableService>();
//builder.Services.AddScoped<IComprobantePagoService, ComprobantePagoService>();

// ================================================================
// CONFIGURACI�N CORS ESPEC�FICA PARA TU CASO
// ================================================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                // Agregar TODOS los or�genes que necesites
                "http://localhost:5173",      // Vite dev server
                "http://localhost:3000",      // React dev server
                "http://localhost:4200",      // Angular dev server
                "http://localhost:8080",      // Vue dev server
                "https://localhost:5173",     // Vite con HTTPS
                "https://reports.payfri-bi.com",      // Dominio de producci�n
                "https://www.payfri-bi.com",  // Con www
                "https://app.payfri-bi.com"   // Subdominio app
               )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // Cache preflight por 10 min
    });
});

// Configuraci�n de controladores
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Entregables por Proyectos",
        Version = "v1",
        Description = "API para la gesti�n de entregables asociados a proyectos por plataformas"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ================================================================
// MIDDLEWARE CORS PERSONALIZADO (ANTES DE TODO)
// ================================================================
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();

    // Lista de or�genes permitidos
    var allowedOrigins = new[]
    {
        "http://localhost:5173",
        "https://localhost:5173",
        "http://localhost:3000",
        "https://payfri-bi.com",
        "https://www.payfri-bi.com",
        "https://app.payfri-bi.com"
    };

    if (allowedOrigins.Contains(origin) || string.IsNullOrEmpty(origin))
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", origin ?? "*");
    }
    else if (app.Environment.IsDevelopment())
    {
        // En desarrollo, ser m�s permisivo
        context.Response.Headers.Add("Access-Control-Allow-Origin", origin ?? "*");
    }

    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH");
    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With, Accept, Origin");
    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
    context.Response.Headers.Add("Access-Control-Max-Age", "86400");

    // Manejar preflight requests (OPTIONS)
    if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = 204; // No Content
        return;
    }

    await next();
});

// ================================================================
// PIPELINE NORMAL
// ================================================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Entregables v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

// CORS despu�s del middleware personalizado
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint de verificaci�n CORS
app.MapGet("/cors-test", () => new {
    message = "CORS funcionando correctamente",
    timestamp = DateTime.UtcNow,
    allowedOrigins = new[] {
        "http://localhost:5173",
        "https://localhost:5173",
        "https://payfri-bi.com"
    }
});

app.MapGet("/health", () => new {
    status = "Healthy",
    timestamp = DateTime.UtcNow
});

// Informaci�n al iniciar
Console.WriteLine("==========================================");
Console.WriteLine("API ENTREGABLES - CORS CONFIGURADO");
Console.WriteLine("==========================================");
Console.WriteLine($" API URL: https://api.reports.payfri-bi.com/");
Console.WriteLine($" CORS permitido para:");
Console.WriteLine($" http://localhost:5173 (Vite dev)");
Console.WriteLine($" https://payfri-bi.com");
Console.WriteLine($" https://app.payfri-bi.com");
Console.WriteLine("==========================================");

app.Run();