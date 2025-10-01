using APIProjectDocs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Models.Data;
using static APIProjectDocs.Services.Service;

namespace APIProjectDocs.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.Activo);

                if (usuario == null)
                {
                    return new ApiResponseDto<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Credenciales inválidas",
                        Errors = new List<string> { "Usuario no encontrado o inactivo" }
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
                {
                    return new ApiResponseDto<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Credenciales inválidas",
                        Errors = new List<string> { "Contraseña incorrecta" }
                    };
                }

                var tokenResult = await GenerateTokenAsync(usuario);
                if (!tokenResult.Success)
                {
                    return new ApiResponseDto<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Error generando token",
                        Errors = tokenResult.Errors
                    };
                }

                var usuarioDto = new UsuarioDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol,
                    Activo = usuario.Activo,
                    FechaCreacion = usuario.FechaCreacion
                };

                var response = new LoginResponseDto
                {
                    Token = tokenResult.Data!,
                    Expiration = DateTime.UtcNow.AddHours(GetTokenExpiryHours()),
                    Usuario = usuarioDto
                };

                return new ApiResponseDto<LoginResponseDto>
                {
                    Success = true,
                    Message = "Login exitoso",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<string>> GenerateTokenAsync(Usuario usuario)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                    new Claim("UserId", usuario.IdUsuario.ToString()),
                    new Claim("UserRole", ((int)usuario.Rol).ToString())
                };

                // Agregar claims específicos según el rol
                await AddRoleSpecificClaimsAsync(claims, usuario);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(GetTokenExpiryHours()),
                    Issuer = jwtSettings["Issuer"],
                    Audience = jwtSettings["Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return new ApiResponseDto<string>
                {
                    Success = true,
                    Message = "Token generado exitosamente",
                    Data = tokenString
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<string>
                {
                    Success = false,
                    Message = "Error generando token",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<Usuario>> ValidateTokenAsync(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
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

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return new ApiResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Token inválido",
                        Errors = new List<string> { "No se pudo extraer el ID del usuario del token" }
                    };
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == userId && u.Activo);

                if (usuario == null)
                {
                    return new ApiResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Usuario no encontrado o inactivo",
                        Errors = new List<string> { "El usuario asociado al token no existe o está inactivo" }
                    };
                }

                return new ApiResponseDto<Usuario>
                {
                    Success = true,
                    Message = "Token válido",
                    Data = usuario
                };
            }
            catch (SecurityTokenException ex)
            {
                return new ApiResponseDto<Usuario>
                {
                    Success = false,
                    Message = "Token inválido",
                    Errors = new List<string> { ex.Message }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Usuario>
                {
                    Success = false,
                    Message = "Error validando token",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task AddRoleSpecificClaimsAsync(List<Claim> claims, Usuario usuario)
        {
            switch (usuario.Rol)
            {
                case RolUsuario.CoordinadorPlataforma:
                    var plataformas = await _context.UsuarioPlataformas
                        .Where(up => up.IdUsuario == usuario.IdUsuario)
                        .Select(up => up.IdPlataforma.ToString())
                        .ToListAsync();

                    if (plataformas.Any())
                    {
                        claims.Add(new Claim("PlataformasAsignadas", string.Join(",", plataformas)));
                    }
                    break;

                case RolUsuario.LiderProyecto:
                    var proyectos = await _context.UsuarioProyectos
                        .Where(up => up.IdUsuario == usuario.IdUsuario)
                        .Select(up => up.IdProyecto.ToString())
                        .ToListAsync();

                    if (proyectos.Any())
                    {
                        claims.Add(new Claim("ProyectosAsignados", string.Join(",", proyectos)));
                    }
                    break;
            }
        }

        private int GetTokenExpiryHours()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            if (int.TryParse(jwtSettings["ExpiryInHours"], out int hours))
            {
                return hours;
            }
            return 24; // Default 24 horas
        }

    }
}
