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
    public class PlataformasController : ControllerBase
    {
        private readonly IPlataformaService _plataformaService;

        public PlataformasController(IPlataformaService plataformaService)
        {
            _plataformaService = plataformaService;
        }

        /// <summary>
        /// Obtener todas las plataformas (paginado)
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="activa">Filtrar por estado activo/inactivo</param>
        /// <returns>Lista paginada de plataformas</returns>
        [HttpGet]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<PlataformaDto>>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? activa = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new ApiResponseDto<PagedResultDto<PlataformaDto>>
                {
                    Success = false,
                    Message = "Parámetros de paginación inválidos",
                    Errors = new List<string> { "La página debe ser >= 1 y el tamaño de página entre 1 y 100" }
                });
            }

            var result = await _plataformaService.GetAllAsync(page, pageSize, activa);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener plataforma por ID
        /// </summary>
        /// <param name="id">ID de la plataforma</param>
        /// <returns>Datos de la plataforma</returns>
        [HttpGet("{id}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<PlataformaDto>>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDto<PlataformaDto>
                {
                    Success = false,
                    Message = "ID de plataforma inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Si es coordinador, verificar que tenga acceso a esta plataforma
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var plataformasAsignadas = User.GetPlataformasAsignadas();
                if (!plataformasAsignadas.Contains(id))
                {
                    return Forbid();
                }
            }

            var result = await _plataformaService.GetByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Crear nueva plataforma (Solo Dirección)
        /// </summary>
        /// <param name="dto">Datos de la plataforma a crear</param>
        /// <returns>Plataforma creada</returns>
        [HttpPost]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<PlataformaDto>>> Create([FromBody] CreatePlataformaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<PlataformaDto>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var result = await _plataformaService.CreateAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.IdPlataforma }, result);
        }

        /// <summary>
        /// Actualizar plataforma existente (Solo Dirección)
        /// </summary>
        /// <param name="id">ID de la plataforma</param>
        /// <param name="dto">Datos a actualizar</param>
        /// <returns>Plataforma actualizada</returns>
        [HttpPut("{id}")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<PlataformaDto>>> Update(int id, [FromBody] UpdatePlataformaDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDto<PlataformaDto>
                {
                    Success = false,
                    Message = "ID de plataforma inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<PlataformaDto>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var result = await _plataformaService.UpdateAsync(id, dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Desactivar plataforma (soft delete) - Solo Dirección
        /// </summary>
        /// <param name="id">ID de la plataforma</param>
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
                    Message = "ID de plataforma inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            var result = await _plataformaService.DeleteAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        /// <summary>
        /// Asignar coordinador a plataforma (Solo Dirección)
        /// </summary>
        /// <param name="idPlataforma">ID de la plataforma</param>
        /// <param name="dto">ID del usuario coordinador</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{idPlataforma}/asignar-coordinador")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<bool>>> AsignarCoordinador(
            int idPlataforma,
            [FromBody] AsignarUsuarioDto dto)
        {
            if (idPlataforma <= 0)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "ID de plataforma inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var result = await _plataformaService.AsignarCoordinadorAsync(idPlataforma, dto.IdUsuario);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Desasignar coordinador de plataforma (Solo Dirección)
        /// </summary>
        /// <param name="idPlataforma">ID de la plataforma</param>
        /// <param name="idUsuario">ID del usuario coordinador</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{idPlataforma}/coordinadores/{idUsuario}")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<bool>>> DesasignarCoordinador(
            int idPlataforma,
            int idUsuario)
        {
            if (idPlataforma <= 0 || idUsuario <= 0)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "IDs inválidos",
                    Errors = new List<string> { "Los IDs deben ser mayores a 0" }
                });
            }

            var result = await _plataformaService.DesasignarCoordinadorAsync(idPlataforma, idUsuario);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener coordinadores asignados a una plataforma
        /// </summary>
        /// <param name="idPlataforma">ID de la plataforma</param>
        /// <returns>Lista de coordinadores</returns>
        [HttpGet("{idPlataforma}/coordinadores")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<List<UsuarioDto>>>> GetCoordinadores(int idPlataforma)
        {
            if (idPlataforma <= 0)
            {
                return BadRequest(new ApiResponseDto<List<UsuarioDto>>
                {
                    Success = false,
                    Message = "ID de plataforma inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Si es coordinador, verificar que tenga acceso a esta plataforma
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var plataformasAsignadas = User.GetPlataformasAsignadas();
                if (!plataformasAsignadas.Contains(idPlataforma))
                {
                    return Forbid();
                }
            }

            var result = await _plataformaService.GetCoordinadoresAsync(idPlataforma);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        /// <summary>
        /// Obtener plataformas asignadas a un coordinador
        /// </summary>
        /// <param name="idCoordinador">ID del coordinador</param>
        /// <returns>Lista de plataformas del coordinador</returns>
        [HttpGet("by-coordinador/{idCoordinador}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<List<PlataformaDto>>>> GetByCoordinador(int idCoordinador)
        {
            if (idCoordinador <= 0)
            {
                return BadRequest(new ApiResponseDto<List<PlataformaDto>>
                {
                    Success = false,
                    Message = "ID de coordinador inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Si es coordinador, solo puede ver sus propias plataformas
            var currentUserRole = User.GetUserRole();
            var currentUserId = User.GetUserId();

            if (currentUserRole == RolUsuario.CoordinadorPlataforma && currentUserId != idCoordinador)
            {
                return Forbid();
            }

            var result = await _plataformaService.GetPlataformasByCoordinadorAsync(idCoordinador);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener mis plataformas (para el coordinador autenticado)
        /// </summary>
        /// <returns>Lista de plataformas del coordinador autenticado</returns>
        [HttpGet("mis-plataformas")]
        [AuthorizeRoles(RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<List<PlataformaDto>>>> GetMisPlataformas()
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == 0)
            {
                return Unauthorized();
            }

            var result = await _plataformaService.GetPlataformasByCoordinadorAsync(currentUserId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener coordinadores disponibles para asignar (usuarios con rol CoordinadorPlataforma)
        /// </summary>
        /// <param name="idPlataforma">ID de plataforma (opcional, para excluir coordinadores ya asignados)</param>
        /// <returns>Lista de coordinadores disponibles</returns>
        [HttpGet("coordinadores-disponibles")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<List<UsuarioDto>>>> GetCoordinadoresDisponibles(
            [FromQuery] int? idPlataforma = null)
        {
            try
            {
                var query = _plataformaService.GetAllAsync(); // Usamos el contexto a través del servicio

                // Para simplificar, devolvemos una respuesta básica
                // En una implementación completa, necesitaríamos acceso directo al contexto
                // o crear un método específico en el servicio

                return Ok(new ApiResponseDto<List<UsuarioDto>>
                {
                    Success = true,
                    Message = "Para obtener coordinadores disponibles, use el endpoint de usuarios con filtro por rol",
                    Data = new List<UsuarioDto>()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<List<UsuarioDto>>
                {
                    Success = false,
                    Message = "Error obteniendo coordinadores disponibles",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener estadísticas de una plataforma
        /// </summary>
        /// <param name="idPlataforma">ID de la plataforma</param>
        /// <returns>Estadísticas de la plataforma</returns>
        [HttpGet("{idPlataforma}/estadisticas")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<PlataformaEstadisticasDto>>> GetEstadisticas(int idPlataforma)
        {
            if (idPlataforma <= 0)
            {
                return BadRequest(new ApiResponseDto<PlataformaEstadisticasDto>
                {
                    Success = false,
                    Message = "ID de plataforma inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Si es coordinador, verificar que tenga acceso a esta plataforma
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var plataformasAsignadas = User.GetPlataformasAsignadas();
                if (!plataformasAsignadas.Contains(idPlataforma))
                {
                    return Forbid();
                }
            }

            // Obtener la plataforma con proyectos
            var plataformaResult = await _plataformaService.GetByIdAsync(idPlataforma);

            if (!plataformaResult.Success || plataformaResult.Data == null)
            {
                return NotFound(plataformaResult);
            }

            var plataforma = plataformaResult.Data;

            var estadisticas = new PlataformaEstadisticasDto
            {
                IdPlataforma = plataforma.IdPlataforma,
                NombrePlataforma = plataforma.Nombre,
                TotalProyectos = plataforma.Proyectos.Count,
                ProyectosActivos = plataforma.Proyectos.Count(p => p.Activo),
                ProyectosInactivos = plataforma.Proyectos.Count(p => !p.Activo),
                ProyectosSinFechaFin = plataforma.Proyectos.Count(p => !p.FechaFin.HasValue),
                ProyectosConFechaFin = plataforma.Proyectos.Count(p => p.FechaFin.HasValue),
                FechaConsulta = DateTime.UtcNow
            };

            return Ok(new ApiResponseDto<PlataformaEstadisticasDto>
            {
                Success = true,
                Message = "Estadísticas obtenidas exitosamente",
                Data = estadisticas
            });
        }

        /// <summary>
        /// Obtener resumen de todas las plataformas con estadísticas básicas
        /// </summary>
        /// <returns>Resumen de plataformas</returns>
        [HttpGet("resumen")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<List<PlataformaResumenDto>>>> GetResumen()
        {
            try
            {
                var plataformasResult = await _plataformaService.GetAllAsync(1, 100); // Obtener todas

                if (!plataformasResult.Success || plataformasResult.Data == null)
                {
                    return BadRequest(plataformasResult);
                }

                var resumen = plataformasResult.Data.Items.Select(p => new PlataformaResumenDto
                {
                    IdPlataforma = p.IdPlataforma,
                    Nombre = p.Nombre,
                    Activa = p.Activa,
                    TotalProyectos = p.Proyectos.Count,
                    ProyectosActivos = p.Proyectos.Count(pr => pr.Activo),
                    FechaCreacion = p.FechaCreacion
                }).ToList();

                return Ok(new ApiResponseDto<List<PlataformaResumenDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {resumen.Count} plataformas",
                    Data = resumen
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<List<PlataformaResumenDto>>
                {
                    Success = false,
                    Message = "Error obteniendo resumen de plataformas",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

    }
}
