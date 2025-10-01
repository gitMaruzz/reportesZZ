using APIProjectDocs.Models;
using APIProjectDocs.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Services.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIProjectDocs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Obtener todos los usuarios (paginado)
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de usuarios</returns>
        [HttpGet]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<UsuarioDto>>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new ApiResponseDto<PagedResultDto<UsuarioDto>>
                {
                    Success = false,
                    Message = "Parámetros de paginación inválidos",
                    Errors = new List<string> { "La página debe ser >= 1 y el tamaño de página entre 1 y 100" }
                });
            }

            var result = await _usuarioService.GetAllAsync(page, pageSize);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener usuario por ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Datos del usuario</returns>
        [HttpGet("{id}")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<UsuarioDto>>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "ID de usuario inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            var result = await _usuarioService.GetByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener usuario por email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Datos del usuario</returns>
        [HttpGet("by-email/{email}")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<UsuarioDto>>> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Email requerido",
                    Errors = new List<string> { "Debe proporcionar un email válido" }
                });
            }

            var result = await _usuarioService.GetByEmailAsync(email);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Crear nuevo usuario
        /// </summary>
        /// <param name="dto">Datos del usuario a crear</param>
        /// <returns>Usuario creado</returns>
        [HttpPost]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<UsuarioDto>>> Create([FromBody] CreateUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var result = await _usuarioService.CreateAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.IdUsuario }, result);
        }

        /// <summary>
        /// Actualizar usuario existente
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="dto">Datos a actualizar</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<UsuarioDto>>> Update(int id, [FromBody] UpdateUsuarioDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "ID de usuario inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var result = await _usuarioService.UpdateAsync(id, dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Desactivar usuario (soft delete)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<bool>>> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "ID de usuario inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Verificar que no sea el usuario actual
            var currentUserId = User.GetUserId();
            if (currentUserId == id)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "No puede desactivar su propio usuario",
                    Errors = new List<string> { "No se permite la auto-desactivación" }
                });
            }

            var result = await _usuarioService.DeleteAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener usuarios por rol
        /// </summary>
        /// <param name="rol">Rol a filtrar</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de usuarios por rol</returns>
        [HttpGet("by-role/{rol}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<UsuarioDto>>>> GetByRole(
            RolUsuario rol,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new ApiResponseDto<PagedResultDto<UsuarioDto>>
                {
                    Success = false,
                    Message = "Parámetros de paginación inválidos",
                    Errors = new List<string> { "La página debe ser >= 1 y el tamaño de página entre 1 y 100" }
                });
            }

            // Los coordinadores solo pueden ver líderes de proyecto
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma && rol != RolUsuario.LiderProyecto)
            {
                return Forbid();
            }

            var result = await _usuarioService.GetAllAsync(page, pageSize);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Filtrar por rol
            var filteredItems = result.Data!.Items.Where(u => u.Rol == rol).ToList();
            var filteredResult = new PagedResultDto<UsuarioDto>
            {
                Items = filteredItems,
                TotalItems = filteredItems.Count,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)filteredItems.Count / pageSize),
                HasNextPage = page < (int)Math.Ceiling((double)filteredItems.Count / pageSize),
                HasPreviousPage = page > 1
            };

            return Ok(new ApiResponseDto<PagedResultDto<UsuarioDto>>
            {
                Success = true,
                Message = "Usuarios obtenidos exitosamente",
                Data = filteredResult
            });
        }
    }
}
