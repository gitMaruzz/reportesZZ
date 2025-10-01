using APIProjectDocs.Models;
using Microsoft.EntityFrameworkCore;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Models.Data;
using static APIProjectDocs.Services.Service;

namespace APIProjectDocs.Services
{
    public class ProyectoService : IProyectoService
    {
        private readonly ApplicationDbContext _context;

        public ProyectoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<PagedResultDto<ProyectoDto>>> GetAllAsync(int page = 1, int pageSize = 10, bool? activo = null)
        {
            try
            {
                var query = _context.Proyectos
                    .Include(p => p.Plataforma)
                    .Include(p => p.Entregables.Where(e => e.Activo))
                    .Include(p => p.UsuarioProyectos)
                        .ThenInclude(up => up.Usuario)
                    .AsQueryable();

                // Filtrar por estado activo si se especifica
                if (activo.HasValue)
                {
                    query = query.Where(p => p.Activo == activo.Value);
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var proyectos = await query
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var proyectosDto = proyectos.Select(p => MapToProyectoDto(p)).ToList();

                var result = new PagedResultDto<ProyectoDto>
                {
                    Items = proyectosDto,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = true,
                    Message = "Proyectos obtenidos exitosamente",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = false,
                    Message = "Error obteniendo proyectos",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<PagedResultDto<ProyectoDto>>> GetByPlataformaAsync(int idPlataforma, int page = 1, int pageSize = 10, bool? activo = null)
        {
            try
            {
                var query = _context.Proyectos
                    .Include(p => p.Plataforma)
                    .Include(p => p.Entregables.Where(e => e.Activo))
                    .Include(p => p.UsuarioProyectos)
                        .ThenInclude(up => up.Usuario)
                    .Where(p => p.IdPlataforma == idPlataforma);

                // Filtrar por estado activo si se especifica
                if (activo.HasValue)
                {
                    query = query.Where(p => p.Activo == activo.Value);
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var proyectos = await query
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var proyectosDto = proyectos.Select(p => MapToProyectoDto(p)).ToList();

                var result = new PagedResultDto<ProyectoDto>
                {
                    Items = proyectosDto,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = true,
                    Message = $"Proyectos de la plataforma obtenidos exitosamente",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = false,
                    Message = "Error obteniendo proyectos por plataforma",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<PagedResultDto<ProyectoDto>>> GetByLiderAsync(int idUsuario, int page = 1, int pageSize = 10, bool? activo = null)
        {
            try
            {
                var query = _context.Proyectos
                    .Include(p => p.Plataforma)
                    .Include(p => p.Entregables.Where(e => e.Activo))
                    .Include(p => p.UsuarioProyectos)
                        .ThenInclude(up => up.Usuario)
                    .Where(p => p.UsuarioProyectos.Any(up => up.IdUsuario == idUsuario));

                // Filtrar por estado activo si se especifica
                if (activo.HasValue)
                {
                    query = query.Where(p => p.Activo == activo.Value);
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var proyectos = await query
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var proyectosDto = proyectos.Select(p => MapToProyectoDto(p)).ToList();

                var result = new PagedResultDto<ProyectoDto>
                {
                    Items = proyectosDto,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = true,
                    Message = $"Proyectos del líder obtenidos exitosamente",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PagedResultDto<ProyectoDto>>
                {
                    Success = false,
                    Message = "Error obteniendo proyectos por líder",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<ProyectoDto>> GetByIdAsync(int id)
        {
            try
            {
                var proyecto = await _context.Proyectos
                    .Include(p => p.Plataforma)
                    .Include(p => p.Entregables.Where(e => e.Activo))
                    .Include(p => p.UsuarioProyectos)
                        .ThenInclude(up => up.Usuario)
                    .FirstOrDefaultAsync(p => p.IdProyecto == id);

                if (proyecto == null)
                {
                    return new ApiResponseDto<ProyectoDto>
                    {
                        Success = false,
                        Message = "Proyecto no encontrado",
                        Errors = new List<string> { $"No existe un proyecto con ID {id}" }
                    };
                }

                var proyectoDto = MapToProyectoDto(proyecto);

                return new ApiResponseDto<ProyectoDto>
                {
                    Success = true,
                    Message = "Proyecto obtenido exitosamente",
                    Data = proyectoDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<ProyectoDto>
                {
                    Success = false,
                    Message = "Error obteniendo proyecto",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<ProyectoDto>> CreateAsync(CreateProyectoDto dto)
        {
            try
            {
                // Verificar que la plataforma existe y está activa
                var plataforma = await _context.Plataformas
                    .FirstOrDefaultAsync(p => p.IdPlataforma == dto.IdPlataforma && p.Activa);

                if (plataforma == null)
                {
                    return new ApiResponseDto<ProyectoDto>
                    {
                        Success = false,
                        Message = "Plataforma no encontrada o inactiva",
                        Errors = new List<string> { "La plataforma debe existir y estar activa" }
                    };
                }

                // Validar que el nombre no exista en la misma plataforma
                var existeNombre = await _context.Proyectos
                    .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() &&
                                   p.IdPlataforma == dto.IdPlataforma);

                if (existeNombre)
                {
                    return new ApiResponseDto<ProyectoDto>
                    {
                        Success = false,
                        Message = "Ya existe un proyecto con ese nombre en la plataforma",
                        Errors = new List<string> { "El nombre del proyecto debe ser único dentro de la plataforma" }
                    };
                }

                // Validar fechas
                if (dto.FechaFin.HasValue && dto.FechaFin.Value <= dto.FechaInicio)
                {
                    return new ApiResponseDto<ProyectoDto>
                    {
                        Success = false,
                        Message = "La fecha de fin debe ser posterior a la fecha de inicio",
                        Errors = new List<string> { "Fechas inválidas" }
                    };
                }

                var proyecto = new Proyecto
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin,
                    Activo = true,
                    IdPlataforma = dto.IdPlataforma,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Proyectos.Add(proyecto);
                await _context.SaveChangesAsync();

                // Recargar con relaciones
                await _context.Entry(proyecto)
                    .Reference(p => p.Plataforma)
                    .LoadAsync();

                var proyectoDto = MapToProyectoDto(proyecto);

                return new ApiResponseDto<ProyectoDto>
                {
                    Success = true,
                    Message = "Proyecto creado exitosamente",
                    Data = proyectoDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<ProyectoDto>
                {
                    Success = false,
                    Message = "Error creando proyecto",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<ProyectoDto>> UpdateAsync(int id, UpdateProyectoDto dto)
        {
            try
            {
                var proyecto = await _context.Proyectos
                    .Include(p => p.Plataforma)
                    .FirstOrDefaultAsync(p => p.IdProyecto == id);

                if (proyecto == null)
                {
                    return new ApiResponseDto<ProyectoDto>
                    {
                        Success = false,
                        Message = "Proyecto no encontrado",
                        Errors = new List<string> { $"No existe un proyecto con ID {id}" }
                    };
                }

                // Validar nombre único si se está cambiando
                if (!string.IsNullOrEmpty(dto.Nombre) && dto.Nombre.ToLower() != proyecto.Nombre.ToLower())
                {
                    var existeNombre = await _context.Proyectos
                        .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() &&
                                       p.IdPlataforma == proyecto.IdPlataforma &&
                                       p.IdProyecto != id);

                    if (existeNombre)
                    {
                        return new ApiResponseDto<ProyectoDto>
                        {
                            Success = false,
                            Message = "Ya existe otro proyecto con ese nombre en la plataforma",
                            Errors = new List<string> { "El nombre del proyecto debe ser único dentro de la plataforma" }
                        };
                    }
                }

                // Validar fechas si se proporcionan
                var fechaInicio = dto.FechaInicio ?? proyecto.FechaInicio;
                var fechaFin = dto.FechaFin ?? proyecto.FechaFin;

                if (fechaFin.HasValue && fechaFin.Value <= fechaInicio)
                {
                    return new ApiResponseDto<ProyectoDto>
                    {
                        Success = false,
                        Message = "La fecha de fin debe ser posterior a la fecha de inicio",
                        Errors = new List<string> { "Fechas inválidas" }
                    };
                }

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrEmpty(dto.Nombre))
                    proyecto.Nombre = dto.Nombre;

                if (dto.Descripcion != null)
                    proyecto.Descripcion = dto.Descripcion;

                if (dto.FechaInicio.HasValue)
                    proyecto.FechaInicio = dto.FechaInicio.Value;

                if (dto.FechaFin.HasValue)
                    proyecto.FechaFin = dto.FechaFin.Value;

                if (dto.Activo.HasValue)
                    proyecto.Activo = dto.Activo.Value;

                await _context.SaveChangesAsync();

                // Recargar con todas las relaciones
                await _context.Entry(proyecto)
                    .Collection(p => p.Entregables)
                    .Query()
                    .Where(e => e.Activo)
                    .LoadAsync();

                await _context.Entry(proyecto)
                    .Collection(p => p.UsuarioProyectos)
                    .Query()
                    .Include(up => up.Usuario)
                    .LoadAsync();

                var proyectoDto = MapToProyectoDto(proyecto);

                return new ApiResponseDto<ProyectoDto>
                {
                    Success = true,
                    Message = "Proyecto actualizado exitosamente",
                    Data = proyectoDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<ProyectoDto>
                {
                    Success = false,
                    Message = "Error actualizando proyecto",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            try
            {
                var proyecto = await _context.Proyectos
                    .Include(p => p.Entregables)
                    .FirstOrDefaultAsync(p => p.IdProyecto == id);

                if (proyecto == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Proyecto no encontrado",
                        Errors = new List<string> { $"No existe un proyecto con ID {id}" }
                    };
                }

                // Verificar si tiene entregables activos
                var tieneEntregablesActivos = proyecto.Entregables.Any(e => e.Activo);
                if (tieneEntregablesActivos)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No se puede desactivar el proyecto",
                        Errors = new List<string> { "El proyecto tiene entregables activos asociados" }
                    };
                }

                // Soft delete - solo marcar como inactivo
                proyecto.Activo = false;
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Proyecto desactivado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error desactivando proyecto",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> AsignarLiderAsync(int idProyecto, int idUsuario)
        {
            try
            {
                // Verificar que el proyecto existe y está activo
                var proyecto = await _context.Proyectos
                    .FirstOrDefaultAsync(p => p.IdProyecto == idProyecto && p.Activo);

                if (proyecto == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Proyecto no encontrado o inactivo",
                        Errors = new List<string> { "El proyecto debe existir y estar activo" }
                    };
                }

                // Verificar que el usuario existe, está activo y es líder de proyecto
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Activo && u.Rol == RolUsuario.LiderProyecto);

                if (usuario == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no encontrado o no es líder de proyecto",
                        Errors = new List<string> { "El usuario debe existir, estar activo y tener rol de Líder de Proyecto" }
                    };
                }

                // Verificar si ya está asignado
                var yaAsignado = await _context.UsuarioProyectos
                    .AnyAsync(up => up.IdUsuario == idUsuario && up.IdProyecto == idProyecto);

                if (yaAsignado)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "El líder ya está asignado a este proyecto",
                        Errors = new List<string> { "La asignación ya existe" }
                    };
                }

                // Crear la asignación
                var asignacion = new UsuarioProyecto
                {
                    IdUsuario = idUsuario,
                    IdProyecto = idProyecto,
                    FechaAsignacion = DateTime.UtcNow
                };

                _context.UsuarioProyectos.Add(asignacion);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Líder asignado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error asignando líder",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DesasignarLiderAsync(int idProyecto, int idUsuario)
        {
            try
            {
                var asignacion = await _context.UsuarioProyectos
                    .FirstOrDefaultAsync(up => up.IdUsuario == idUsuario && up.IdProyecto == idProyecto);

                if (asignacion == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Asignación no encontrada",
                        Errors = new List<string> { "El líder no está asignado a este proyecto" }
                    };
                }

                _context.UsuarioProyectos.Remove(asignacion);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Líder desasignado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error desasignando líder",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<List<UsuarioDto>>> GetLideresAsync(int idProyecto)
        {
            try
            {
                var lideres = await _context.UsuarioProyectos
                    .Where(up => up.IdProyecto == idProyecto)
                    .Include(up => up.Usuario)
                    .Select(up => new UsuarioDto
                    {
                        IdUsuario = up.Usuario.IdUsuario,
                        Nombre = up.Usuario.Nombre,
                        Email = up.Usuario.Email,
                        Rol = up.Usuario.Rol,
                        Activo = up.Usuario.Activo,
                        FechaCreacion = up.Usuario.FechaCreacion
                    })
                    .ToListAsync();

                return new ApiResponseDto<List<UsuarioDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {lideres.Count} líderes",
                    Data = lideres
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<UsuarioDto>>
                {
                    Success = false,
                    Message = "Error obteniendo líderes",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private ProyectoDto MapToProyectoDto(Proyecto proyecto)
        {
            return new ProyectoDto
            {
                IdProyecto = proyecto.IdProyecto,
                Nombre = proyecto.Nombre,
                Descripcion = proyecto.Descripcion,
                FechaInicio = proyecto.FechaInicio,
                FechaFin = proyecto.FechaFin,
                Activo = proyecto.Activo,
                FechaCreacion = proyecto.FechaCreacion,
                Plataforma = new PlataformaBasicaDto
                {
                    IdPlataforma = proyecto.Plataforma.IdPlataforma,
                    Nombre = proyecto.Plataforma.Nombre,
                    Activa = proyecto.Plataforma.Activa
                },
                Entregables = proyecto.Entregables?.Select(e => new EntregableBasicoDto
                {
                    IdEntregable = e.IdEntregable,
                    Nombre = e.Nombre,
                    Titulo = e.Titulo,
                    FechaDisponibilidad = e.FechaDisponibilidad,
                    OrigenDatos = e.OrigenDatos,
                    Activo = e.Activo
                }).ToList() ?? new List<EntregableBasicoDto>(),
                Lideres = proyecto.UsuarioProyectos?.Select(up => new UsuarioDto
                {
                    IdUsuario = up.Usuario.IdUsuario,
                    Nombre = up.Usuario.Nombre,
                    Email = up.Usuario.Email,
                    Rol = up.Usuario.Rol,
                    Activo = up.Usuario.Activo,
                    FechaCreacion = up.Usuario.FechaCreacion
                }).ToList() ?? new List<UsuarioDto>()
            };
        }
    }
}
