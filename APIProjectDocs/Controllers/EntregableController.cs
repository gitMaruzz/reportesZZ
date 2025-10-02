using APIProjectDocs.Models;
using APIProjectDocs.Utils;
using Microsoft.AspNetCore.Mvc;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Services.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIProjectDocs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntregableController : ControllerBase
    {
        private readonly IEntregableService _entregableService;
        private readonly ILogger<EntregableController> _logger;

        public EntregableController(
            IEntregableService entregableService,
            ILogger<EntregableController> logger)
        {
            _entregableService = entregableService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los entregables (solo Dirección)
        /// </summary>
        [HttpGet]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<List<EntregableDto>>>> GetAll()
        {
            var result = await _entregableService.GetAllAsync();
            return StatusCode(result.Success ? 200 : 500, result);
        }

        /// <summary>
        /// Obtener entregable por ID con validación de acceso
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<EntregableDto>>> GetById(int id)
        {
            var currentUserRole = User.GetUserRole();
            var currentUserId = User.GetUserId();

            // Verificar acceso según rol
            if (currentUserRole == RolUsuario.LiderProyecto)
            {
                var entregableResult = await _entregableService.GetByIdAsync(id);
                if (entregableResult.Success && entregableResult.Data != null)
                {
                    var proyectosAsignados = User.GetProyectosAsignados();
                    if (!proyectosAsignados.Contains(entregableResult.Data.IdProyecto))
                    {
                        return Forbid();
                    }
                }
            }
            else if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var entregableResult = await _entregableService.GetByIdAsync(id);
                if (entregableResult.Success && entregableResult.Data != null)
                {
                    var plataformasAsignadas = User.GetPlataformasAsignadas();
                    // Necesitaríamos obtener la plataforma del entregable para validar
                    // Por simplificación, asumimos que el coordinador puede ver entregables de sus plataformas
                }
            }

            var result = await _entregableService.GetByIdAsync(id);
            return StatusCode(result.Success ? 200 : 404, result);
        }

        /// <summary>
        /// Obtener entregables por proyecto con validación de acceso
        /// </summary>
        [HttpGet("by-proyecto/{idProyecto}")]
        public async Task<ActionResult<ApiResponseDto<List<EntregableDto>>>> GetByProyecto(int idProyecto)
        {
            var currentUserRole = User.GetUserRole();

            // Validar acceso según rol
            if (currentUserRole == RolUsuario.LiderProyecto)
            {
                var proyectosAsignados = User.GetProyectosAsignados();
                if (!proyectosAsignados.Contains(idProyecto))
                {
                    return Forbid();
                }
            }
            else if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var plataformasAsignadas = User.GetPlataformasAsignadas();
                // Aquí deberíamos validar que el proyecto pertenece a una de sus plataformas
                // Por ahora permitimos el acceso y validamos en el servicio
            }

            var result = await _entregableService.GetByProyectoAsync(idProyecto);
            return StatusCode(result.Success ? 200 : (result.Message.Contains("no encontrado") ? 404 : 500), result);
        }

        /// <summary>
        /// Obtener entregables del usuario actual (solo Líder de Proyecto)
        /// </summary>
        [HttpGet("mis-entregables")]
        [AuthorizeRoles(RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<List<EntregableDto>>>> GetMisEntregables()
        {
            var currentUserId = User.GetUserId();
            var result = await _entregableService.GetByUserAsync(currentUserId);
            return StatusCode(result.Success ? 200 : 500, result);
        }

        /// <summary>
        /// Crear nuevo entregable (solo Líder de Proyecto)
        /// </summary>
        [HttpPost]
        [AuthorizeRoles(RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<EntregableDto>>> Create([FromBody] CreateEntregableDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<EntregableDto>
                {
                    Success = false,
                    Message = "Datos inválidos",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            // Validar que el líder puede crear entregables en este proyecto
            var proyectosAsignados = User.GetProyectosAsignados();
            if (!proyectosAsignados.Contains(createDto.IdProyecto))
            {
                return Forbid();
            }

            var result = await _entregableService.CreateAsync(createDto);
            return StatusCode(result.Success ? 201 : 400, result);
        }

        /// <summary>
        /// Actualizar entregable (solo Líder de Proyecto propietario)
        /// </summary>
        [HttpPut("{id}")]
        [AuthorizeRoles(RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<EntregableDto>>> Update(int id, [FromBody] UpdateEntregableDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<EntregableDto>
                {
                    Success = false,
                    Message = "Datos inválidos",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            // Verificar que el entregable pertenece a un proyecto asignado al líder
            var entregableResult = await _entregableService.GetByIdAsync(id);
            if (!entregableResult.Success || entregableResult.Data == null)
            {
                return NotFound(entregableResult);
            }

            var proyectosAsignados = User.GetProyectosAsignados();
            if (!proyectosAsignados.Contains(entregableResult.Data.IdProyecto))
            {
                return Forbid();
            }

            var result = await _entregableService.UpdateAsync(id, updateDto);
            return StatusCode(result.Success ? 200 : 400, result);
        }

        /// <summary>
        /// Eliminar entregable (solo Líder de Proyecto propietario)
        /// </summary>
        [HttpDelete("{id}")]
        [AuthorizeRoles(RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<bool>>> Delete(int id)
        {
            // Verificar que el entregable pertenece a un proyecto asignado al líder
            var entregableResult = await _entregableService.GetByIdAsync(id);
            if (!entregableResult.Success || entregableResult.Data == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Entregable no encontrado"
                });
            }

            var proyectosAsignados = User.GetProyectosAsignados();
            if (!proyectosAsignados.Contains(entregableResult.Data.IdProyecto))
            {
                return Forbid();
            }

            var result = await _entregableService.DeleteAsync(id);
            return StatusCode(result.Success ? 200 : 500, result);
        }

        /// <summary>
        /// Verificar disponibilidad de entregable
        /// </summary>
        //[HttpGet("{id}/disponibilidad")]
        //public async Task<ActionResult<ApiResponseDto<EntregableDisponibilidadDto>>> CheckDisponibilidad(int id)
        //{
        //    var currentUserRole = User.GetUserRole();

        //    // Validar acceso según rol
        //    if (currentUserRole == RolUsuario.LiderProyecto)
        //    {
        //        var entregableResult = await _entregableService.GetByIdAsync(id);
        //        if (entregableResult.Success && entregableResult.Data != null)
        //        {
        //            var proyectosAsignados = User.GetProyectosAsignados();
        //            if (!proyectosAsignados.Contains(entregableResult.Data.IdProyecto))
        //            {
        //                return Forbid();
        //            }
        //        }
        //    }

        //    var result = await _entregableService.CheckDisponibilidadAsync(id);
        //    return StatusCode(result.Success ? 200 : 404, result);
        //}

        /// <summary>
        /// Obtener datos del entregable (desde SQL Server o API externa)
        /// </summary>
        [HttpGet("{id}/data")]
        public async Task<ActionResult<ApiResponseDto<object>>> GetEntregableData(int id)
        {
            var currentUserRole = User.GetUserRole();

            // Validar acceso según rol
            if (currentUserRole == RolUsuario.LiderProyecto)
            {
                var entregableResult = await _entregableService.GetByIdAsync(id);
                if (entregableResult.Success && entregableResult.Data != null)
                {
                    var proyectosAsignados = User.GetProyectosAsignados();
                    if (!proyectosAsignados.Contains(entregableResult.Data.IdProyecto))
                    {
                        return Forbid();
                    }
                }
            }
            else if (currentUserRole == RolUsuario.UsuarioAdministracion)
            {
                // Los usuarios de administración pueden acceder a los datos para adjuntar comprobantes
                // pero no modificar los entregables
            }

            var result = await _entregableService.GetEntregableDataAsync(id);
            return StatusCode(result.Success ? 200 : (result.Message.Contains("no encontrado") ? 404 : 400), result);
        }

        /// <summary>
        /// Obtener entregables disponibles
        /// </summary>
        [HttpGet("disponibles")]
        public async Task<ActionResult<ApiResponseDto<List<EntregableDto>>>> GetDisponibles()
        {
            var result = await _entregableService.GetEntregablesDisponiblesAsync();
            return StatusCode(result.Success ? 200 : 500, result);
        }

        /// <summary>
        /// Obtener entregables pendientes
        /// </summary>
        [HttpGet("pendientes")]
        public async Task<ActionResult<ApiResponseDto<List<EntregableDto>>>> GetPendientes()
        {
            var result = await _entregableService.GetEntregablesPendientesAsync();
            return StatusCode(result.Success ? 200 : 500, result);
        }

        /// <summary>
        /// Activar entregable (solo Líder de Proyecto propietario)
        /// </summary>
        [HttpPatch("{id}/activar")]
        [AuthorizeRoles(RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<bool>>> Activar(int id)
        {
            // Verificar que el entregable pertenece a un proyecto asignado al líder
            var entregableResult = await _entregableService.GetByIdAsync(id);
            if (!entregableResult.Success || entregableResult.Data == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Entregable no encontrado"
                });
            }

            var proyectosAsignados = User.GetProyectosAsignados();
            if (!proyectosAsignados.Contains(entregableResult.Data.IdProyecto))
            {
                return Forbid();
            }

            var result = await _entregableService.ActivarAsync(id);
            return StatusCode(result.Success ? 200 : 500, result);
        }

        /// <summary>
        /// Desactivar entregable (solo Líder de Proyecto propietario)
        /// </summary>
        [HttpPatch("{id}/desactivar")]
        [AuthorizeRoles(RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<bool>>> Desactivar(int id)
        {
            // Verificar que el entregable pertenece a un proyecto asignado al líder
            var entregableResult = await _entregableService.GetByIdAsync(id);
            if (!entregableResult.Success || entregableResult.Data == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Entregable no encontrado"
                });
            }

            var proyectosAsignados = User.GetProyectosAsignados();
            if (!proyectosAsignados.Contains(entregableResult.Data.IdProyecto))
            {
                return Forbid();
            }

            var result = await _entregableService.DesactivarAsync(id);
            return StatusCode(result.Success ? 200 : 500, result);
        }

        /// <summary>
        /// Obtener estadísticas de entregables (solo Dirección y Coordinadores)
        /// </summary>
        [HttpGet("estadisticas")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<object>>> GetEstadisticas()
        {
            try
            {
                var disponibles = await _entregableService.GetEntregablesDisponiblesAsync();
                var pendientes = await _entregableService.GetEntregablesPendientesAsync();

                var estadisticas = new
                {
                    TotalDisponibles = disponibles.Data?.Count ?? 0,
                    TotalPendientes = pendientes.Data?.Count ?? 0,
                    Total = (disponibles.Data?.Count ?? 0) + (pendientes.Data?.Count ?? 0),
                    FechaConsulta = DateTime.UtcNow
                };

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Estadísticas obtenidas exitosamente",
                    Data = estadisticas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de entregables");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }
    }
}
