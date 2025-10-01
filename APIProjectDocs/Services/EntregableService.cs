using APIProjectDocs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Models.Data;
using static APIProjectDocs.Services.Service;

namespace APIProjectDocs.Services
{
    public class EntregableService : IEntregableService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataSourceService _dataSourceService;
        private readonly ILogger<EntregableService> _logger;

        public EntregableService(
            ApplicationDbContext context,
            IDataSourceService dataSourceService,
            ILogger<EntregableService> logger)
        {
            _context = context;
            _dataSourceService = dataSourceService;
            _logger = logger;
        }

        public async Task<ApiResponseDto<List<EntregableDto>>> GetAllAsync()
        {
            try
            {
                var entregables = await _context.Entregables
                    .Include(e => e.Proyecto)
                        .ThenInclude(p => p.Plataforma)
                    .Include(e => e.ComprobantesPago)
                    .Where(e => e.Activo)
                    .OrderBy(e => e.FechaDisponibilidad)
                    .ToListAsync();

                var entregablesDto = entregables.Select(MapToDto).ToList();

                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = true,
                    Message = "Entregables obtenidos exitosamente",
                    Data = entregablesDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los entregables");
                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<EntregableDto>> GetByIdAsync(int id)
        {
            try
            {
                var entregable = await _context.Entregables
                    .Include(e => e.Proyecto)
                        .ThenInclude(p => p.Plataforma)
                    .Include(e => e.ComprobantesPago)
                    .FirstOrDefaultAsync(e => e.IdEntregable == id);

                if (entregable == null)
                {
                    return new ApiResponseDto<EntregableDto>
                    {
                        Success = false,
                        Message = "Entregable no encontrado"
                    };
                }

                var entregableDto = MapToDto(entregable);

                return new ApiResponseDto<EntregableDto>
                {
                    Success = true,
                    Message = "Entregable obtenido exitosamente",
                    Data = entregableDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el entregable {Id}", id);
                return new ApiResponseDto<EntregableDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<List<EntregableDto>>> GetByProyectoAsync(int idProyecto)
        {
            try
            {
                var proyecto = await _context.Proyectos.FindAsync(idProyecto);
                if (proyecto == null)
                {
                    return new ApiResponseDto<List<EntregableDto>>
                    {
                        Success = false,
                        Message = "Proyecto no encontrado"
                    };
                }

                var entregables = await _context.Entregables
                    .Include(e => e.Proyecto)
                        .ThenInclude(p => p.Plataforma)
                    .Include(e => e.ComprobantesPago)
                    .Where(e => e.IdProyecto == idProyecto && e.Activo)
                    .OrderBy(e => e.FechaDisponibilidad)
                    .ToListAsync();

                var entregablesDto = entregables.Select(MapToDto).ToList();

                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = true,
                    Message = $"Entregables del proyecto '{proyecto.Nombre}' obtenidos exitosamente",
                    Data = entregablesDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entregables del proyecto {IdProyecto}", idProyecto);
                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<List<EntregableDto>>> GetByUserAsync(int idUsuario)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.ProyectosAsignados)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null)
                {
                    return new ApiResponseDto<List<EntregableDto>>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                var proyectosIds = usuario.ProyectosAsignados.Select(p => p.IdProyecto).ToList();

                var entregables = await _context.Entregables
                    .Include(e => e.Proyecto)
                        .ThenInclude(p => p.Plataforma)
                    .Include(e => e.ComprobantesPago)
                    .Where(e => proyectosIds.Contains(e.IdProyecto) && e.Activo)
                    .OrderBy(e => e.FechaDisponibilidad)
                    .ToListAsync();

                var entregablesDto = entregables.Select(MapToDto).ToList();

                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = true,
                    Message = "Entregables del usuario obtenidos exitosamente",
                    Data = entregablesDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entregables del usuario {IdUsuario}", idUsuario);
                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<EntregableDto>> CreateAsync(CreateEntregableDto createDto)
        {
            try
            {
                // Validar que el proyecto existe
                var proyecto = await _context.Proyectos.FindAsync(createDto.IdProyecto);
                if (proyecto == null)
                {
                    return new ApiResponseDto<EntregableDto>
                    {
                        Success = false,
                        Message = "El proyecto especificado no existe"
                    };
                }

                // Validar que el proyecto esté activo
                if (!proyecto.Activo)
                {
                    return new ApiResponseDto<EntregableDto>
                    {
                        Success = false,
                        Message = "No se pueden crear entregables en proyectos inactivos"
                    };
                }

                // Validar fecha de disponibilidad
                if (createDto.FechaDisponibilidad < DateTime.Today)
                {
                    return new ApiResponseDto<EntregableDto>
                    {
                        Success = false,
                        Message = "La fecha de disponibilidad no puede ser anterior a hoy"
                    };
                }

                var entregable = new Entregable
                {
                    IdProyecto = createDto.IdProyecto,
                    Titulo = createDto.Titulo,
                    Descripcion = createDto.Descripcion,
                    FechaDisponibilidad = createDto.FechaDisponibilidad,
                    OrigenDatos = createDto.OrigenDatos,
                    ConfiguracionOrigen = createDto.ConfiguracionOrigen,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Entregables.Add(entregable);
                await _context.SaveChangesAsync();

                // Recargar con relaciones
                entregable = await _context.Entregables
                    .Include(e => e.Proyecto)
                        .ThenInclude(p => p.Plataforma)
                    .Include(e => e.ComprobantesPago)
                    .FirstAsync(e => e.IdEntregable == entregable.IdEntregable);

                var entregableDto = MapToDto(entregable);

                return new ApiResponseDto<EntregableDto>
                {
                    Success = true,
                    Message = "Entregable creado exitosamente",
                    Data = entregableDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el entregable");
                return new ApiResponseDto<EntregableDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<EntregableDto>> UpdateAsync(int id, UpdateEntregableDto updateDto)
        {
            try
            {
                var entregable = await _context.Entregables
                    .Include(e => e.Proyecto)
                        .ThenInclude(p => p.Plataforma)
                    .FirstOrDefaultAsync(e => e.IdEntregable == id);

                if (entregable == null)
                {
                    return new ApiResponseDto<EntregableDto>
                    {
                        Success = false,
                        Message = "Entregable no encontrado"
                    };
                }

                // Validar fecha de disponibilidad si se proporciona
                if (updateDto.FechaDisponibilidad.HasValue &&
                    updateDto.FechaDisponibilidad.Value < DateTime.Today)
                {
                    return new ApiResponseDto<EntregableDto>
                    {
                        Success = false,
                        Message = "La fecha de disponibilidad no puede ser anterior a hoy"
                    };
                }

                // Actualizar campos
                if (!string.IsNullOrEmpty(updateDto.Titulo))
                    entregable.Titulo = updateDto.Titulo;

                if (!string.IsNullOrEmpty(updateDto.Descripcion))
                    entregable.Descripcion = updateDto.Descripcion;

                if (updateDto.FechaDisponibilidad.HasValue)
                    entregable.FechaDisponibilidad = updateDto.FechaDisponibilidad.Value;

                if (!string.IsNullOrEmpty(updateDto.ConfiguracionOrigen))
                    entregable.ConfiguracionOrigen = updateDto.ConfiguracionOrigen;

                if (updateDto.OrigenDatos.HasValue)
                    entregable.OrigenDatos = updateDto.OrigenDatos.Value;

                entregable.FechaModificacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Recargar con relaciones actualizadas
                await _context.Entry(entregable)
                    .Reference(e => e.Proyecto)
                    .LoadAsync();
                await _context.Entry(entregable.Proyecto)
                    .Reference(p => p.Plataforma)
                    .LoadAsync();
                await _context.Entry(entregable)
                    .Collection(e => e.ComprobantesPago)
                    .LoadAsync();

                var entregableDto = MapToDto(entregable);

                return new ApiResponseDto<EntregableDto>
                {
                    Success = true,
                    Message = "Entregable actualizado exitosamente",
                    Data = entregableDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el entregable {Id}", id);
                return new ApiResponseDto<EntregableDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            try
            {
                var entregable = await _context.Entregables.FindAsync(id);
                if (entregable == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Entregable no encontrado"
                    };
                }

                // Verificar si tiene comprobantes de pago
                var tieneComprobantes = await _context.ComprobantesPago
                    .AnyAsync(c => c.IdEntregable == id);

                if (tieneComprobantes)
                {
                    // Soft delete - solo desactivar
                    entregable.Activo = false;
                    entregable.FechaModificacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return new ApiResponseDto<bool>
                    {
                        Success = true,
                        Message = "Entregable desactivado (tiene comprobantes asociados)",
                        Data = true
                    };
                }
                else
                {
                    // Hard delete - eliminar completamente
                    _context.Entregables.Remove(entregable);
                    await _context.SaveChangesAsync();

                    return new ApiResponseDto<bool>
                    {
                        Success = true,
                        Message = "Entregable eliminado exitosamente",
                        Data = true
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el entregable {Id}", id);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<EntregableDisponibilidadDto>> CheckDisponibilidadAsync(int id)
        {
            try
            {
                var entregable = await _context.Entregables
                    .Include(e => e.Proyecto)
                    .FirstOrDefaultAsync(e => e.IdEntregable == id);

                if (entregable == null)
                {
                    return new ApiResponseDto<EntregableDisponibilidadDto>
                    {
                        Success = false,
                        Message = "Entregable no encontrado"
                    };
                }

                var ahora = DateTime.UtcNow;
                var disponibilidad = new EntregableDisponibilidadDto
                {
                    IdEntregable = entregable.IdEntregable,
                    Titulo = entregable.Titulo,
                    FechaDisponibilidad = entregable.FechaDisponibilidad,
                    EstaDisponible = entregable.Activo && entregable.FechaDisponibilidad <= ahora,
                    DiasFaltantes = entregable.FechaDisponibilidad > ahora
                        ? (int)(entregable.FechaDisponibilidad - ahora).TotalDays
                        : 0,
                    Estado = entregable.Activo
                        ? (entregable.FechaDisponibilidad <= ahora ? "Disponible" : "Pendiente")
                        : "Inactivo",
                    FechaConsulta = ahora
                };

                return new ApiResponseDto<EntregableDisponibilidadDto>
                {
                    Success = true,
                    Message = "Disponibilidad verificada exitosamente",
                    Data = disponibilidad
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad del entregable {Id}", id);
                return new ApiResponseDto<EntregableDisponibilidadDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<object>> GetEntregableDataAsync(int id)
        {
            try
            {
                var entregable = await _context.Entregables
                    .Include(e => e.Proyecto)
                    .FirstOrDefaultAsync(e => e.IdEntregable == id);

                if (entregable == null)
                {
                    return new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Entregable no encontrado"
                    };
                }

                // Verificar disponibilidad
                if (!entregable.Activo || entregable.FechaDisponibilidad > DateTime.UtcNow)
                {
                    return new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "El entregable no está disponible aún"
                    };
                }

                // Obtener datos según el origen configurado
                object data;
                switch (entregable.OrigenDatos)
                {
                    case OrigenDatos.SqlServer:
                        data = await _dataSourceService.GetFromSqlServerAsync(entregable.ConfiguracionOrigen);
                        break;
                    case OrigenDatos.ApiExterna:
                        data = await _dataSourceService.GetFromApiExternaAsync(entregable.ConfiguracionOrigen);
                        break;
                    default:
                        return new ApiResponseDto<object>
                        {
                            Success = false,
                            Message = "Origen de datos no válido"
                        };
                }

                return new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Datos del entregable obtenidos exitosamente",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos del entregable {Id}", id);
                return new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<List<EntregableDto>>> GetEntregablesDisponiblesAsync()
        {
            try
            {
                var ahora = DateTime.UtcNow;
                var entregables = await _context.Entregables
                    .Include(e => e.Proyecto)
                        .ThenInclude(p => p.Plataforma)
                    .Include(e => e.ComprobantesPago)
                    .Where(e => e.Activo && e.FechaDisponibilidad <= ahora)
                    .OrderBy(e => e.FechaDisponibilidad)
                    .ToListAsync();

                var entregablesDto = entregables.Select(MapToDto).ToList();

                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = true,
                    Message = "Entregables disponibles obtenidos exitosamente",
                    Data = entregablesDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entregables disponibles");
                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<List<EntregableDto>>> GetEntregablesPendientesAsync()
        {
            try
            {
                var ahora = DateTime.UtcNow;
                var entregables = await _context.Entregables
                    .Include(e => e.Proyecto)
                        .ThenInclude(p => p.Plataforma)
                    .Include(e => e.ComprobantesPago)
                    .Where(e => e.Activo && e.FechaDisponibilidad > ahora)
                    .OrderBy(e => e.FechaDisponibilidad)
                    .ToListAsync();

                var entregablesDto = entregables.Select(MapToDto).ToList();

                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = true,
                    Message = "Entregables pendientes obtenidos exitosamente",
                    Data = entregablesDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entregables pendientes");
                return new ApiResponseDto<List<EntregableDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> ActivarAsync(int id)
        {
            try
            {
                var entregable = await _context.Entregables.FindAsync(id);
                if (entregable == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Entregable no encontrado"
                    };
                }

                entregable.Activo = true;
                entregable.FechaModificacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Entregable activado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar el entregable {Id}", id);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DesactivarAsync(int id)
        {
            try
            {
                var entregable = await _context.Entregables.FindAsync(id);
                if (entregable == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Entregable no encontrado"
                    };
                }

                entregable.Activo = false;
                entregable.FechaModificacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Entregable desactivado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar el entregable {Id}", id);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private static EntregableDto MapToDto(Entregable entregable)
        {
            return new EntregableDto
            {
                IdEntregable = entregable.IdEntregable,
                IdProyecto = entregable.IdProyecto,
                ProyectoNombre = entregable.Proyecto?.Nombre ?? "",
                PlataformaNombre = entregable.Proyecto?.Plataforma?.Nombre ?? "",
                Titulo = entregable.Titulo,
                Descripcion = entregable.Descripcion,
                FechaDisponibilidad = entregable.FechaDisponibilidad,
                OrigenDatos = entregable.OrigenDatos,
                ConfiguracionOrigen = entregable.ConfiguracionOrigen,
                Activo = entregable.Activo,
                FechaCreacion = entregable.FechaCreacion,
                FechaModificacion = entregable.FechaModificacion,
                CantidadComprobantes = entregable.ComprobantesPago?.Count ?? 0,
                EstaDisponible = entregable.Activo && entregable.FechaDisponibilidad <= DateTime.UtcNow
            };
        }
    }

}
