using APIProjectDocs.Models;
using Microsoft.EntityFrameworkCore;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Models.Data;
using static APIProjectDocs.Services.Service;

namespace APIProjectDocs.Services
{
    public class PlataformaService : IPlataformaService
    {
        private readonly ApplicationDbContext _context;

        public PlataformaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<PagedResultDto<PlataformaDto>>> GetAllAsync(int page = 1, int pageSize = 10, bool? activa = null)
        {
            try
            {
                var query = _context.Plataformas.AsQueryable();

                // Filtrar por estado activo si se especifica
                if (activa.HasValue)
                {
                    query = query.Where(p => p.Activa == activa.Value);
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var plataformas = await query
                    .Include(p => p.Proyectos.Where(pr => pr.Activo)) // Solo proyectos activos
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var plataformasDto = plataformas.Select(p => new PlataformaDto
                {
                    IdPlataforma = p.IdPlataforma,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Activa = p.Activa,
                    FechaCreacion = p.FechaCreacion,
                    Proyectos = p.Proyectos.Select(pr => new ProyectoBasicoDto
                    {
                        IdProyecto = pr.IdProyecto,
                        Nombre = pr.Nombre,
                        Descripcion = pr.Descripcion,
                        FechaInicio = pr.FechaInicio,
                        FechaFin = pr.FechaFin,
                        Activo = pr.Activo
                    }).ToList()
                }).ToList();

                var result = new PagedResultDto<PlataformaDto>
                {
                    Items = plataformasDto,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new ApiResponseDto<PagedResultDto<PlataformaDto>>
                {
                    Success = true,
                    Message = "Plataformas obtenidas exitosamente",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PagedResultDto<PlataformaDto>>
                {
                    Success = false,
                    Message = "Error obteniendo plataformas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<PlataformaDto>> GetByIdAsync(int id)
        {
            try
            {
                var plataforma = await _context.Plataformas
                    .Include(p => p.Proyectos.Where(pr => pr.Activo))
                    .FirstOrDefaultAsync(p => p.IdPlataforma == id);

                if (plataforma == null)
                {
                    return new ApiResponseDto<PlataformaDto>
                    {
                        Success = false,
                        Message = "Plataforma no encontrada",
                        Errors = new List<string> { $"No existe una plataforma con ID {id}" }
                    };
                }

                var plataformaDto = new PlataformaDto
                {
                    IdPlataforma = plataforma.IdPlataforma,
                    Nombre = plataforma.Nombre,
                    Descripcion = plataforma.Descripcion,
                    Activa = plataforma.Activa,
                    FechaCreacion = plataforma.FechaCreacion,
                    Proyectos = plataforma.Proyectos.Select(pr => new ProyectoBasicoDto
                    {
                        IdProyecto = pr.IdProyecto,
                        Nombre = pr.Nombre,
                        Descripcion = pr.Descripcion,
                        FechaInicio = pr.FechaInicio,
                        FechaFin = pr.FechaFin,
                        Activo = pr.Activo
                    }).ToList()
                };

                return new ApiResponseDto<PlataformaDto>
                {
                    Success = true,
                    Message = "Plataforma obtenida exitosamente",
                    Data = plataformaDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PlataformaDto>
                {
                    Success = false,
                    Message = "Error obteniendo plataforma",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<PlataformaDto>> CreateAsync(CreatePlataformaDto dto)
        {
            try
            {
                // Validar que el nombre no exista
                var existeNombre = await _context.Plataformas
                    .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());

                if (existeNombre)
                {
                    return new ApiResponseDto<PlataformaDto>
                    {
                        Success = false,
                        Message = "El nombre de la plataforma ya existe",
                        Errors = new List<string> { "Ya existe una plataforma con este nombre" }
                    };
                }

                var plataforma = new Plataforma
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Activa = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Plataformas.Add(plataforma);
                await _context.SaveChangesAsync();

                var plataformaDto = new PlataformaDto
                {
                    IdPlataforma = plataforma.IdPlataforma,
                    Nombre = plataforma.Nombre,
                    Descripcion = plataforma.Descripcion,
                    Activa = plataforma.Activa,
                    FechaCreacion = plataforma.FechaCreacion,
                    Proyectos = new List<ProyectoBasicoDto>()
                };

                return new ApiResponseDto<PlataformaDto>
                {
                    Success = true,
                    Message = "Plataforma creada exitosamente",
                    Data = plataformaDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PlataformaDto>
                {
                    Success = false,
                    Message = "Error creando plataforma",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<PlataformaDto>> UpdateAsync(int id, UpdatePlataformaDto dto)
        {
            try
            {
                var plataforma = await _context.Plataformas
                    .FirstOrDefaultAsync(p => p.IdPlataforma == id);

                if (plataforma == null)
                {
                    return new ApiResponseDto<PlataformaDto>
                    {
                        Success = false,
                        Message = "Plataforma no encontrada",
                        Errors = new List<string> { $"No existe una plataforma con ID {id}" }
                    };
                }

                // Validar nombre único si se está cambiando
                if (!string.IsNullOrEmpty(dto.Nombre) && dto.Nombre.ToLower() != plataforma.Nombre.ToLower())
                {
                    var existeNombre = await _context.Plataformas
                        .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower() && p.IdPlataforma != id);

                    if (existeNombre)
                    {
                        return new ApiResponseDto<PlataformaDto>
                        {
                            Success = false,
                            Message = "El nombre de la plataforma ya existe",
                            Errors = new List<string> { "Ya existe otra plataforma con este nombre" }
                        };
                    }
                }

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrEmpty(dto.Nombre))
                    plataforma.Nombre = dto.Nombre;

                if (dto.Descripcion != null) // Permitir descripción vacía
                    plataforma.Descripcion = dto.Descripcion;

                if (dto.Activa.HasValue)
                    plataforma.Activa = dto.Activa.Value;

                await _context.SaveChangesAsync();

                // Recargar con proyectos
                await _context.Entry(plataforma)
                    .Collection(p => p.Proyectos)
                    .Query()
                    .Where(pr => pr.Activo)
                    .LoadAsync();

                var plataformaDto = new PlataformaDto
                {
                    IdPlataforma = plataforma.IdPlataforma,
                    Nombre = plataforma.Nombre,
                    Descripcion = plataforma.Descripcion,
                    Activa = plataforma.Activa,
                    FechaCreacion = plataforma.FechaCreacion,
                    Proyectos = plataforma.Proyectos.Select(pr => new ProyectoBasicoDto
                    {
                        IdProyecto = pr.IdProyecto,
                        Nombre = pr.Nombre,
                        Descripcion = pr.Descripcion,
                        FechaInicio = pr.FechaInicio,
                        FechaFin = pr.FechaFin,
                        Activo = pr.Activo
                    }).ToList()
                };

                return new ApiResponseDto<PlataformaDto>
                {
                    Success = true,
                    Message = "Plataforma actualizada exitosamente",
                    Data = plataformaDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PlataformaDto>
                {
                    Success = false,
                    Message = "Error actualizando plataforma",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            try
            {
                var plataforma = await _context.Plataformas
                    .Include(p => p.Proyectos)
                    .FirstOrDefaultAsync(p => p.IdPlataforma == id);

                if (plataforma == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Plataforma no encontrada",
                        Errors = new List<string> { $"No existe una plataforma con ID {id}" }
                    };
                }

                // Verificar si tiene proyectos activos
                var tieneProyectosActivos = plataforma.Proyectos.Any(p => p.Activo);
                if (tieneProyectosActivos)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No se puede eliminar la plataforma",
                        Errors = new List<string> { "La plataforma tiene proyectos activos asociados" }
                    };
                }

                // Soft delete - solo marcar como inactiva
                plataforma.Activa = false;
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Plataforma desactivada exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error desactivando plataforma",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> AsignarCoordinadorAsync(int idPlataforma, int idUsuario)
        {
            try
            {
                // Verificar que la plataforma existe y está activa
                var plataforma = await _context.Plataformas
                    .FirstOrDefaultAsync(p => p.IdPlataforma == idPlataforma && p.Activa);

                if (plataforma == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Plataforma no encontrada o inactiva",
                        Errors = new List<string> { "La plataforma debe existir y estar activa" }
                    };
                }

                // Verificar que el usuario existe, está activo y es coordinador
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Activo && u.Rol == RolUsuario.CoordinadorPlataforma);

                if (usuario == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no encontrado o no es coordinador de plataforma",
                        Errors = new List<string> { "El usuario debe existir, estar activo y tener rol de Coordinador de Plataforma" }
                    };
                }

                // Verificar si ya está asignado
                var yaAsignado = await _context.UsuarioPlataformas
                    .AnyAsync(up => up.IdUsuario == idUsuario && up.IdPlataforma == idPlataforma);

                if (yaAsignado)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "El coordinador ya está asignado a esta plataforma",
                        Errors = new List<string> { "La asignación ya existe" }
                    };
                }

                // Crear la asignación
                var asignacion = new UsuarioPlataforma
                {
                    IdUsuario = idUsuario,
                    IdPlataforma = idPlataforma,
                    FechaAsignacion = DateTime.UtcNow
                };

                _context.UsuarioPlataformas.Add(asignacion);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Coordinador asignado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error asignando coordinador",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DesasignarCoordinadorAsync(int idPlataforma, int idUsuario)
        {
            try
            {
                var asignacion = await _context.UsuarioPlataformas
                    .FirstOrDefaultAsync(up => up.IdUsuario == idUsuario && up.IdPlataforma == idPlataforma);

                if (asignacion == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Asignación no encontrada",
                        Errors = new List<string> { "El coordinador no está asignado a esta plataforma" }
                    };
                }

                _context.UsuarioPlataformas.Remove(asignacion);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Coordinador desasignado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error desasignando coordinador",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<List<UsuarioDto>>> GetCoordinadoresAsync(int idPlataforma)
        {
            try
            {
                var coordinadores = await _context.UsuarioPlataformas
                    .Where(up => up.IdPlataforma == idPlataforma)
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
                    Message = $"Se encontraron {coordinadores.Count} coordinadores",
                    Data = coordinadores
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<UsuarioDto>>
                {
                    Success = false,
                    Message = "Error obteniendo coordinadores",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<List<PlataformaDto>>> GetPlataformasByCoordinadorAsync(int idUsuario)
        {
            try
            {
                var plataformas = await _context.UsuarioPlataformas
                    .Where(up => up.IdUsuario == idUsuario)
                    .Include(up => up.Plataforma)
                        .ThenInclude(p => p.Proyectos.Where(pr => pr.Activo))
                    .Select(up => new PlataformaDto
                    {
                        IdPlataforma = up.Plataforma.IdPlataforma,
                        Nombre = up.Plataforma.Nombre,
                        Descripcion = up.Plataforma.Descripcion,
                        Activa = up.Plataforma.Activa,
                        FechaCreacion = up.Plataforma.FechaCreacion,
                        Proyectos = up.Plataforma.Proyectos.Select(pr => new ProyectoBasicoDto
                        {
                            IdProyecto = pr.IdProyecto,
                            Nombre = pr.Nombre,
                            Descripcion = pr.Descripcion,
                            FechaInicio = pr.FechaInicio,
                            FechaFin = pr.FechaFin,
                            Activo = pr.Activo
                        }).ToList()
                    })
                    .ToListAsync();

                return new ApiResponseDto<List<PlataformaDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {plataformas.Count} plataformas",
                    Data = plataformas
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<PlataformaDto>>
                {
                    Success = false,
                    Message = "Error obteniendo plataformas del coordinador",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
