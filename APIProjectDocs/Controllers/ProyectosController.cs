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
    public class ProyectosController : ControllerBase
    {
        private readonly IProyectoService _proyectoService;

        public ProyectosController(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }

        /// <summary>
        /// Obtener todos los proyectos (paginado)
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="activo">Filtrar por estado activo/inactivo</param>
        /// <returns>Lista paginada de proyectos</returns>
        [HttpGet]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<ProyectoDto>>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? activo = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = false,
                    Message = "Parámetros de paginación inválidos",
                    Errors = new List<string> { "La página debe ser >= 1 y el tamaño de página entre 1 y 100" }
                });
            }

            var result = await _proyectoService.GetAllAsync(page, pageSize, activo);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener proyectos por plataforma (paginado)
        /// </summary>
        /// <param name="idPlataforma">ID de la plataforma</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="activo">Filtrar por estado activo/inactivo</param>
        /// <returns>Lista paginada de proyectos de la plataforma</returns>
        [HttpGet("by-plataforma/{idPlataforma}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<ProyectoDto>>>> GetByPlataforma(
            int idPlataforma,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? activo = null)
        {
            if (idPlataforma <= 0)
            {
                return BadRequest(new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = false,
                    Message = "ID de plataforma inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = false,
                    Message = "Parámetros de paginación inválidos",
                    Errors = new List<string> { "La página debe ser >= 1 y el tamaño de página entre 1 y 100" }
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

            var result = await _proyectoService.GetByPlataformaAsync(idPlataforma, page, pageSize, activo);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        /// <summary>
        /// Obtener proyecto por ID
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Datos del proyecto</returns>
        [HttpGet("{id}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma, RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<ProyectoDto>>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDto<ProyectoDto>
                {
                    Success = false,
                    Message = "ID de proyecto inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            var result = await _proyectoService.GetByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            // Verificar permisos según el rol
            var currentUserRole = User.GetUserRole();
            var currentUserId = User.GetUserId();

            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var plataformasAsignadas = User.GetPlataformasAsignadas();
                if (!plataformasAsignadas.Contains(result.Data!.Plataforma.IdPlataforma))
                {
                    return Forbid();
                }
            }
            else if (currentUserRole == RolUsuario.LiderProyecto)
            {
                var proyectosAsignados = User.GetProyectosAsignados();
                if (!proyectosAsignados.Contains(id))
                {
                    return Forbid();
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Crear nuevo proyecto
        /// </summary>
        /// <param name="dto">Datos del proyecto a crear</param>
        /// <returns>Proyecto creado</returns>
        [HttpPost]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<ProyectoDto>>> Create([FromBody] CreateProyectoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<ProyectoDto>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            // Si es coordinador, verificar que tenga acceso a la plataforma
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var plataformasAsignadas = User.GetPlataformasAsignadas();
                if (!plataformasAsignadas.Contains(dto.IdPlataforma))
                {
                    return Forbid();
                }
            }

            var result = await _proyectoService.CreateAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.IdProyecto }, result);
        }

        /// <summary>
        /// Actualizar proyecto existente
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <param name="dto">Datos a actualizar</param>
        /// <returns>Proyecto actualizado</returns>
        [HttpPut("{id}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<ProyectoDto>>> Update(int id, [FromBody] UpdateProyectoDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDto<ProyectoDto>
                {
                    Success = false,
                    Message = "ID de proyecto inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto<ProyectoDto>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            // Si es coordinador, verificar que tenga acceso al proyecto
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var proyectoResult = await _proyectoService.GetByIdAsync(id);
                if (proyectoResult.Success)
                {
                    var plataformasAsignadas = User.GetPlataformasAsignadas();
                    if (!plataformasAsignadas.Contains(proyectoResult.Data!.Plataforma.IdPlataforma))
                    {
                        return Forbid();
                    }
                }
            }

            var result = await _proyectoService.UpdateAsync(id, dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        /// <summary>
        /// Desactivar proyecto (soft delete)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<bool>>> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "ID de proyecto inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Si es coordinador, verificar que tenga acceso al proyecto
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var proyectoResult = await _proyectoService.GetByIdAsync(id);
                if (proyectoResult.Success)
                {
                    var plataformasAsignadas = User.GetPlataformasAsignadas();
                    if (!plataformasAsignadas.Contains(proyectoResult.Data!.Plataforma.IdPlataforma))
                    {
                        return Forbid();
                    }
                }
            }

            var result = await _proyectoService.DeleteAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Asignar líder a proyecto
        /// </summary>
        /// <param name="idProyecto">ID del proyecto</param>
        /// <param name="dto">ID del usuario líder</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{idProyecto}/asignar-lider")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<bool>>> AsignarLider(
            int idProyecto,
            [FromBody] AsignarUsuarioDto dto)
        {
            if (idProyecto <= 0)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "ID de proyecto inválido",
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

            // Si es coordinador, verificar que tenga acceso al proyecto
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var proyectoResult = await _proyectoService.GetByIdAsync(idProyecto);
                if (proyectoResult.Success)
                {
                    var plataformasAsignadas = User.GetPlataformasAsignadas();
                    if (!plataformasAsignadas.Contains(proyectoResult.Data!.Plataforma.IdPlataforma))
                    {
                        return Forbid();
                    }
                }
            }

            var result = await _proyectoService.AsignarLiderAsync(idProyecto, dto.IdUsuario);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Desasignar líder de proyecto
        /// </summary>
        /// <param name="idProyecto">ID del proyecto</param>
        /// <param name="idUsuario">ID del usuario líder</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{idProyecto}/lideres/{idUsuario}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma)]
        public async Task<ActionResult<ApiResponseDto<bool>>> DesasignarLider(
            int idProyecto,
            int idUsuario)
        {
            if (idProyecto <= 0 || idUsuario <= 0)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "IDs inválidos",
                    Errors = new List<string> { "Los IDs deben ser mayores a 0" }
                });
            }

            // Si es coordinador, verificar que tenga acceso al proyecto
            var currentUserRole = User.GetUserRole();
            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var proyectoResult = await _proyectoService.GetByIdAsync(idProyecto);
                if (proyectoResult.Success)
                {
                    var plataformasAsignadas = User.GetPlataformasAsignadas();
                    if (!plataformasAsignadas.Contains(proyectoResult.Data!.Plataforma.IdPlataforma))
                    {
                        return Forbid();
                    }
                }
            }

            var result = await _proyectoService.DesasignarLiderAsync(idProyecto, idUsuario);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener líderes asignados a un proyecto
        /// </summary>
        /// <param name="idProyecto">ID del proyecto</param>
        /// <returns>Lista de líderes</returns>
        [HttpGet("{idProyecto}/lideres")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma, RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<List<UsuarioDto>>>> GetLideres(int idProyecto)
        {
            if (idProyecto <= 0)
            {
                return BadRequest(new ApiResponseDto<List<UsuarioDto>>
                {
                    Success = false,
                    Message = "ID de proyecto inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Verificar permisos según el rol
            var currentUserRole = User.GetUserRole();
            var currentUserId = User.GetUserId();

            if (currentUserRole == RolUsuario.CoordinadorPlataforma)
            {
                var proyectoResult = await _proyectoService.GetByIdAsync(idProyecto);
                if (proyectoResult.Success)
                {
                    var plataformasAsignadas = User.GetPlataformasAsignadas();
                    if (!plataformasAsignadas.Contains(proyectoResult.Data!.Plataforma.IdPlataforma))
                    {
                        return Forbid();
                    }
                }
            }
            else if (currentUserRole == RolUsuario.LiderProyecto)
            {
                var proyectosAsignados = User.GetProyectosAsignados();
                if (!proyectosAsignados.Contains(idProyecto))
                {
                    return Forbid();
                }
            }

            var result = await _proyectoService.GetLideresAsync(idProyecto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener mis proyectos (para el líder autenticado)
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="activo">Filtrar por estado activo/inactivo</param>
        /// <returns>Lista de proyectos del usuario autenticado</returns>
        [HttpGet("mis-proyectos")]
        [AuthorizeRoles(RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<ProyectoDto>>>> GetMisProyectos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? activo = null)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId == 0)
            {
                return Unauthorized();
            }

            var result = await _proyectoService.GetByLiderAsync(currentUserId, page, pageSize, activo);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener estadísticas de un proyecto
        /// </summary>
        /// <param name="idProyecto">ID del proyecto</param>
        /// <returns>Estadísticas del proyecto</returns>
        [HttpGet("{idProyecto}/estadisticas")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma, RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<ProyectoEstadisticasDto>>> GetEstadisticas(int idProyecto)
        {
            if (idProyecto <= 0)
            {
                return BadRequest(new ApiResponseDto<ProyectoEstadisticasDto>
                {
                    Success = false,
                    Message = "ID de proyecto inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Verificar permisos
            var currentUserRole = User.GetUserRole();
            var currentUserId = User.GetUserId();

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
                var proyectoResult = await _proyectoService.GetByIdAsync(idProyecto);
                if (proyectoResult.Success)
                {
                    var plataformasAsignadas = User.GetPlataformasAsignadas();
                    if (!plataformasAsignadas.Contains(proyectoResult.Data!.Plataforma.IdPlataforma))
                    {
                        return Forbid();
                    }
                }
            }

            // Obtener el proyecto con entregables
            var resultado = await _proyectoService.GetByIdAsync(idProyecto);

            if (!resultado.Success || resultado.Data == null)
            {
                return NotFound(resultado);
            }

            var proyecto = resultado.Data;

            var estadisticas = new ProyectoEstadisticasDto
            {
                IdProyecto = proyecto.IdProyecto,
                NombreProyecto = proyecto.Nombre,
                NombrePlataforma = proyecto.Plataforma.Nombre,
                TotalEntregables = proyecto.Entregables.Count,
                EntregablesActivos = proyecto.Entregables.Count(e => e.Activo),
                EntregablesInactivos = proyecto.Entregables.Count(e => !e.Activo),
                TotalLideres = proyecto.Lideres.Count,
                LideresActivos = proyecto.Lideres.Count(l => l.Activo),
                FechaInicio = proyecto.FechaInicio,
                FechaFin = proyecto.FechaFin,
                DiasTranscurridos = (int)(DateTime.UtcNow - proyecto.FechaInicio).TotalDays,
                DiasRestantes = proyecto.FechaFin.HasValue ?
                    (int)(proyecto.FechaFin.Value - DateTime.UtcNow).TotalDays : null,
                FechaConsulta = DateTime.UtcNow
            };

            return Ok(new ApiResponseDto<ProyectoEstadisticasDto>
            {
                Success = true,
                Message = "Estadísticas obtenidas exitosamente",
                Data = estadisticas
            });
        }


        /// <summary>
        /// Obtener resumen de proyectos por plataforma
        /// </summary>
        /// <returns>Resumen de proyectos</returns>
        [HttpGet("resumen-por-plataforma")]
        [AuthorizeRoles(RolUsuario.Direccion)]
        public async Task<ActionResult<ApiResponseDto<List<ProyectoPorPlataformaDto>>>> GetResumenPorPlataforma()
        {
            try
            {
                // Obtener todos los proyectos para hacer el resumen
                var proyectosResult = await _proyectoService.GetAllAsync(1, 1000); // Obtener muchos proyectos

                if (!proyectosResult.Success || proyectosResult.Data == null)
                {
                    return BadRequest(proyectosResult);
                }

                var resumen = proyectosResult.Data.Items
                    .GroupBy(p => new { p.Plataforma.IdPlataforma, p.Plataforma.Nombre })
                    .Select(g => new ProyectoPorPlataformaDto
                    {
                        IdPlataforma = g.Key.IdPlataforma,
                        NombrePlataforma = g.Key.Nombre,
                        TotalProyectos = g.Count(),
                        ProyectosActivos = g.Count(p => p.Activo),
                        ProyectosInactivos = g.Count(p => !p.Activo),
                        TotalEntregables = g.Sum(p => p.Entregables.Count),
                        ProyectoConMasEntregables = g.OrderByDescending(p => p.Entregables.Count)
                                                   .FirstOrDefault()?.Nombre ?? "N/A"
                    })
                    .OrderByDescending(r => r.TotalProyectos)
                    .ToList();

                return Ok(new ApiResponseDto<List<ProyectoPorPlataformaDto>>
                {
                    Success = true,
                    Message = $"Resumen de {resumen.Count} plataformas obtenido exitosamente",
                    Data = resumen
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<List<ProyectoPorPlataformaDto>>
                {
                    Success = false,
                    Message = "Error obteniendo resumen por plataforma",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener proyectos por líder (paginado)
        /// </summary>
        /// <param name="idLider">ID del líder de proyecto</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="activo">Filtrar por estado activo/inactivo</param>
        /// <returns>Lista paginada de proyectos del líder</returns>
        [HttpGet("by-lider/{idLider}")]
        [AuthorizeRoles(RolUsuario.Direccion, RolUsuario.CoordinadorPlataforma, RolUsuario.LiderProyecto)]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<ProyectoDto>>>> GetByLider(
            int idLider,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? activo = null)
        {
            // Validación del ID del líder
            if (idLider <= 0)
            {
                return BadRequest(new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = false,
                    Message = "ID de líder inválido",
                    Errors = new List<string> { "El ID debe ser mayor a 0" }
                });
            }

            // Validación de parámetros de paginación
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = false,
                    Message = "Parámetros de paginación inválidos",
                    Errors = new List<string> { "La página debe ser >= 1 y el tamaño de página entre 1 y 100" }
                });
            }

            // Verificar permisos: un líder solo puede ver sus propios proyectos
            var currentUserRole = User.GetUserRole();
            var currentUserId = User.GetUserId();

            if (currentUserRole == RolUsuario.LiderProyecto && currentUserId != idLider)
            {
                return Forbid();
            }

            // Llamar al servicio para obtener los proyectos
            var result = await _proyectoService.GetByLiderAsync(idLider, page, pageSize, activo);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

    }
}
