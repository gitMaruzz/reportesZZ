using APIProjectDocs.Models;
using Microsoft.EntityFrameworkCore;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Models.Data;
using static APIProjectDocs.Services.Service;

namespace APIProjectDocs.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<PagedResultDto<UsuarioDto>>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Usuarios.AsQueryable();

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var usuarios = await query
                    .OrderBy(u => u.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UsuarioDto
                    {
                        IdUsuario = u.IdUsuario,
                        Nombre = u.Nombre,
                        Email = u.Email,
                        Rol = u.Rol,
                        Activo = u.Activo,
                        FechaCreacion = u.FechaCreacion
                    })
                    .ToListAsync();

                var result = new PagedResultDto<UsuarioDto>
                {
                    Items = usuarios,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new ApiResponseDto<PagedResultDto<UsuarioDto>>
                {
                    Success = true,
                    Message = "Usuarios obtenidos exitosamente",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<PagedResultDto<UsuarioDto>>
                {
                    Success = false,
                    Message = "Error obteniendo usuarios",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<UsuarioDto>> GetByIdAsync(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == id);

                if (usuario == null)
                {
                    return new ApiResponseDto<UsuarioDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Errors = new List<string> { $"No existe un usuario con ID {id}" }
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

                return new ApiResponseDto<UsuarioDto>
                {
                    Success = true,
                    Message = "Usuario obtenido exitosamente",
                    Data = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Error obteniendo usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<UsuarioDto>> GetByEmailAsync(string email)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (usuario == null)
                {
                    return new ApiResponseDto<UsuarioDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Errors = new List<string> { $"No existe un usuario con email {email}" }
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

                return new ApiResponseDto<UsuarioDto>
                {
                    Success = true,
                    Message = "Usuario obtenido exitosamente",
                    Data = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Error obteniendo usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<UsuarioDto>> CreateAsync(CreateUsuarioDto dto)
        {
            try
            {
                // Validar que el email no exista
                var existeEmail = await _context.Usuarios
                    .AnyAsync(u => u.Email == dto.Email);

                if (existeEmail)
                {
                    return new ApiResponseDto<UsuarioDto>
                    {
                        Success = false,
                        Message = "El email ya está en uso",
                        Errors = new List<string> { "Ya existe un usuario con este email" }
                    };
                }

                var usuario = new Usuario
                {
                    Nombre = dto.Nombre,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Rol = dto.Rol,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var usuarioDto = new UsuarioDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol,
                    Activo = usuario.Activo,
                    FechaCreacion = usuario.FechaCreacion
                };

                return new ApiResponseDto<UsuarioDto>
                {
                    Success = true,
                    Message = "Usuario creado exitosamente",
                    Data = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Error creando usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == id);

                if (usuario == null)
                {
                    return new ApiResponseDto<UsuarioDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Errors = new List<string> { $"No existe un usuario con ID {id}" }
                    };
                }

                // Validar email único si se está cambiando
                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != usuario.Email)
                {
                    var existeEmail = await _context.Usuarios
                        .AnyAsync(u => u.Email == dto.Email && u.IdUsuario != id);

                    if (existeEmail)
                    {
                        return new ApiResponseDto<UsuarioDto>
                        {
                            Success = false,
                            Message = "El email ya está en uso",
                            Errors = new List<string> { "Ya existe otro usuario con este email" }
                        };
                    }
                }

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrEmpty(dto.Nombre))
                    usuario.Nombre = dto.Nombre;

                if (!string.IsNullOrEmpty(dto.Email))
                    usuario.Email = dto.Email;

                if (dto.Activo.HasValue)
                    usuario.Activo = dto.Activo.Value;

                await _context.SaveChangesAsync();

                var usuarioDto = new UsuarioDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol,
                    Activo = usuario.Activo,
                    FechaCreacion = usuario.FechaCreacion
                };

                return new ApiResponseDto<UsuarioDto>
                {
                    Success = true,
                    Message = "Usuario actualizado exitosamente",
                    Data = usuarioDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UsuarioDto>
                {
                    Success = false,
                    Message = "Error actualizando usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == id);

                if (usuario == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Errors = new List<string> { $"No existe un usuario con ID {id}" }
                    };
                }

                // Soft delete - solo marcar como inactivo
                usuario.Activo = false;
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Usuario desactivado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error desactivando usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> ChangePasswordAsync(int id, string currentPassword, string newPassword)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == id && u.Activo);

                if (usuario == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Errors = new List<string> { $"No existe un usuario activo con ID {id}" }
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, usuario.PasswordHash))
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Contraseña actual incorrecta",
                        Errors = new List<string> { "La contraseña actual no es válida" }
                    };
                }

                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Contraseña cambiada exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error cambiando contraseña",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

    }
}
