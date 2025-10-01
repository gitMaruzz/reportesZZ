using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Services.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIProjectDocs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;

        public AuthController(IAuthService authService, IUsuarioService usuarioService)
        {
            _authService = authService;
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Autenticar usuario y obtener token JWT
        /// </summary>
        /// <param name="request">Credenciales de login</param>
        /// <returns>Token JWT y datos del usuario</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        // <summary>
        /// Validar token JWT actual
        /// </summary>
        /// <returns>Información del usuario autenticado</returns>
        [HttpGet("validate")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<UsuarioDto>>> ValidateToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Token inválido",
                    Errors = new List<string> { "No se pudo extraer el ID del usuario del token" }
                });
            }

            var result = await _usuarioService.GetByIdAsync(userId);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }


        /// <summary>
        /// Cambiar contraseña del usuario autenticado
        /// </summary>
        /// <param name="request">Contraseñas actual y nueva</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<bool>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Token inválido",
                    Errors = new List<string> { "No se pudo extraer el ID del usuario del token" }
                });
            }

            var result = await _usuarioService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener información del usuario autenticado
        /// </summary>
        /// <returns>Datos del usuario actual</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<UsuarioDto>>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Token inválido",
                    Errors = new List<string> { "No se pudo extraer el ID del usuario del token" }
                });
            }

            var result = await _usuarioService.GetByIdAsync(userId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
