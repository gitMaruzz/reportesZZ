using APIProjectDocs.Models;
using System.ComponentModel.DataAnnotations;

namespace APIProjectDocs.DTOs
{
    public class DTO
    {
        // DTOs de Autenticación
        public class LoginRequestDto
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponseDto
        {
            public string Token { get; set; } = string.Empty;
            public DateTime Expiration { get; set; }
            public UsuarioDto Usuario { get; set; } = null!;
        }
        // DTOs de Usuario
        public class UsuarioDto
        {
            public int IdUsuario { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public RolUsuario Rol { get; set; }
            public bool Activo { get; set; }
            public DateTime FechaCreacion { get; set; }
        }

        public class CreateUsuarioDto
        {
            [Required]
            [StringLength(100)]
            public string Nombre { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [StringLength(100)]
            public string Email { get; set; } = string.Empty;

            [Required]
            [MinLength(6)]
            public string Password { get; set; } = string.Empty;

            [Required]
            public RolUsuario Rol { get; set; }
        }

        public class UpdateUsuarioDto
        {
            [StringLength(100)]
            public string? Nombre { get; set; }

            [EmailAddress]
            [StringLength(100)]
            public string? Email { get; set; }

            public bool? Activo { get; set; }
        }

        // DTOs de Plataforma
        public class PlataformaDto
        {
            public int IdPlataforma { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public bool Activa { get; set; }
            public DateTime FechaCreacion { get; set; }
            public List<ProyectoBasicoDto> Proyectos { get; set; } = new();
        }

        public class CreatePlataformaDto
        {
            [Required]
            [StringLength(200)]
            public string Nombre { get; set; } = string.Empty;

            [StringLength(500)]
            public string? Descripcion { get; set; }
        }

        public class UpdatePlataformaDto
        {
            [StringLength(200)]
            public string? Nombre { get; set; }

            [StringLength(500)]
            public string? Descripcion { get; set; }

            public bool? Activa { get; set; }
        }

        // DTOs de Proyecto
        public class ProyectoDto
        {
            public int IdProyecto { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public DateTime FechaInicio { get; set; }
            public DateTime? FechaFin { get; set; }
            public bool Activo { get; set; }
            public DateTime FechaCreacion { get; set; }
            public PlataformaBasicaDto Plataforma { get; set; } = null!;
            public List<EntregableBasicoDto> Entregables { get; set; } = new();
            public List<UsuarioDto> Lideres { get; set; } = new();
        }

        public class ProyectoBasicoDto
        {
            public int IdProyecto { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public DateTime FechaInicio { get; set; }
            public DateTime? FechaFin { get; set; }
            public bool Activo { get; set; }
        }

        public class CreateProyectoDto
        {
            [Required]
            [StringLength(200)]
            public string Nombre { get; set; } = string.Empty;

            [StringLength(1000)]
            public string? Descripcion { get; set; }

            [Required]
            public DateTime FechaInicio { get; set; }

            public DateTime? FechaFin { get; set; }

            [Required]
            public int IdPlataforma { get; set; }
        }

        public class UpdateProyectoDto
        {
            [StringLength(200)]
            public string? Nombre { get; set; }

            [StringLength(1000)]
            public string? Descripcion { get; set; }

            public DateTime? FechaInicio { get; set; }
            public DateTime? FechaFin { get; set; }
            public bool? Activo { get; set; }
        }

        // DTOs de Entregable
        public class EntregableDto
        {
            public int IdEntregable { get; set; }
            public int IdProyecto { get; set; }
            public string Nombre { get; set; }
            public string ProyectoNombre { get; set; } = string.Empty;
            public string PlataformaNombre { get; set; } = string.Empty;
            public string Titulo { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public DateTime FechaDisponibilidad { get; set; }
            public OrigenDatos OrigenDatos { get; set; }
            public string OrigenDatosDescripcion => OrigenDatos switch
            {
                OrigenDatos.SqlServer => "SQL Server",
                OrigenDatos.ApiExterna => "API Externa",
                _ => "Desconocido"
            };
            public string? ConfiguracionOrigen { get; set; }
            public bool Activo { get; set; }
            public DateTime FechaCreacion { get; set; }
            public DateTime? FechaModificacion { get; set; }
            public int CantidadComprobantes { get; set; }
            public bool EstaDisponible { get; set; }
            public string EstadoDescripcion => Activo
            ? (EstaDisponible ? "Disponible" : "Pendiente")
            : "Inactivo";

            public int? DiasFaltantes
            {
                get
                {
                    if (!Activo || EstaDisponible) return null;
                    var dias = (FechaDisponibilidad - DateTime.UtcNow).Days;
                    return dias > 0 ? dias : 0;
                }
            }
        }

        public class EntregableBasicoDto
        {
            public int IdEntregable { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Titulo { get; set; }
            public DateTime FechaDisponibilidad { get; set; }
            public OrigenDatos OrigenDatos { get; set; }
            public bool Activo { get; set; }
        }

        public class CreateEntregableDto
        {
            [Required(ErrorMessage = "El ID del proyecto es obligatorio")]
            public int IdProyecto { get; set; }

            [Required(ErrorMessage = "El nombre es obligatorio")]
            [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El título es obligatorio")]
            [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
            public string Titulo { get; set; } = string.Empty;

            [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
            public string? Descripcion { get; set; }

            [Required(ErrorMessage = "La fecha de disponibilidad es obligatoria")]
            public DateTime FechaDisponibilidad { get; set; }

            [Required(ErrorMessage = "El origen de datos es obligatorio")]
            public OrigenDatos OrigenDatos { get; set; }

            [Required(ErrorMessage = "La configuración del origen es obligatoria")]
            [StringLength(2000, ErrorMessage = "La configuración no puede exceder 2000 caracteres")]
            public string ConfiguracionOrigen { get; set; } = string.Empty;
        }

        public class UpdateEntregableDto
        {
            [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
            public string? Titulo { get; set; }

            [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
            public string? Nombre { get; set; }

            [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
            public string? Descripcion { get; set; }

            public DateTime? FechaDisponibilidad { get; set; }

            public OrigenDatos? OrigenDatos { get; set; }

            [StringLength(2000, ErrorMessage = "La configuración no puede exceder 2000 caracteres")]
            public string? ConfiguracionOrigen { get; set; }
        }

        /// <summary>
        /// DTO para verificar la disponibilidad de un entregable
        /// </summary>
        public class EntregableDisponibilidadDto
        {
            public int IdEntregable { get; set; }
            public string Titulo { get; set; } = string.Empty;
            public DateTime FechaDisponibilidad { get; set; }
            public bool EstaDisponible { get; set; }
            public int DiasFaltantes { get; set; }
            public string Estado { get; set; } = string.Empty; // "Disponible", "Pendiente", "Inactivo"
            public DateTime FechaConsulta { get; set; }
        }

        /// <summary>
        /// DTO para listado simplificado de entregables
        /// </summary>
        public class EntregableListDto
        {
            public int IdEntregable { get; set; }
            public string Titulo { get; set; } = string.Empty;
            public string ProyectoNombre { get; set; } = string.Empty;
            public DateTime FechaDisponibilidad { get; set; }
            public bool EstaDisponible { get; set; }
            public string Estado { get; set; } = string.Empty;
            public int CantidadComprobantes { get; set; }
        }

        /// <summary>
        /// DTO para configurar origen de datos SQL Server
        /// </summary>
        public class SqlServerConfigDto
        {
            [Required]
            public string ConnectionString { get; set; } = string.Empty;

            [Required]
            public string ViewName { get; set; } = string.Empty;

            public Dictionary<string, object>? Parameters { get; set; }
        }

        /// DTO para configurar origen de datos API Externa
        /// </summary>
        public class ApiExternaConfigDto
        {
            [Required]
            public string Url { get; set; } = string.Empty;

            public string? Method { get; set; } = "GET";

            public Dictionary<string, string>? Headers { get; set; }

            public object? Body { get; set; }

            public int TimeoutSeconds { get; set; } = 30;
        }

        // DTOs de Comprobante de Pago
        public class ComprobantePagoDto
        {
            public int IdComprobante { get; set; }
            public DateTime FechaDocumento { get; set; }
            public string RutaArchivoPDF { get; set; } = string.Empty;
            public string? NombreArchivoOriginal { get; set; }
            public long TamanoArchivo { get; set; }
            public DateTime FechaSubida { get; set; }
            public UsuarioDto UsuarioSubida { get; set; } = null!;
            public EntregableBasicoDto Entregable { get; set; } = null!;
        }

        public class CreateComprobantePagoDto
        {
            [Required]
            public int IdEntregable { get; set; }

            [Required]
            public DateTime FechaDocumento { get; set; }

            [Required]
            public IFormFile ArchivoPDF { get; set; } = null!;
        }

        // DTOs auxiliares
        public class PlataformaBasicaDto
        {
            public int IdPlataforma { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public bool Activa { get; set; }
        }

        public class AsignarUsuarioDto
        {
            [Required]
            public int IdUsuario { get; set; }
        }

        // DTOs para respuestas de API
        public class ApiResponseDto<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
            public List<string> Errors { get; set; } = new();
        }

        public class PagedResultDto<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalItems { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public bool HasNextPage { get; set; }
            public bool HasPreviousPage { get; set; }
        }

        // DTOs para datos de entregables
        public class EntregableDataDto
        {
            public int IdEntregable { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public DateTime FechaDisponibilidad { get; set; }
            public OrigenDatos OrigenDatos { get; set; }
            public object? Datos { get; set; }
            public DateTime FechaConsulta { get; set; }
            public bool ExitoConsulta { get; set; }
            public string? MensajeError { get; set; }
        }

        /// <summary>
        /// DTO para el cambio de contraseña
        /// </summary>
        public class ChangePasswordRequestDto
        {
            [Required]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required]
            [MinLength(6)]
            public string NewPassword { get; set; } = string.Empty;

            [Required]
            [Compare("NewPassword")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        /// <summary>
        /// DTO para estadísticas de plataforma
        /// </summary>
        public class PlataformaEstadisticasDto
        {
            public int IdPlataforma { get; set; }
            public string NombrePlataforma { get; set; } = string.Empty;
            public int TotalProyectos { get; set; }
            public int ProyectosActivos { get; set; }
            public int ProyectosInactivos { get; set; }
            public int ProyectosSinFechaFin { get; set; }
            public int ProyectosConFechaFin { get; set; }
            public DateTime FechaConsulta { get; set; }
        }

        /// <summary>
        /// DTO para resumen de plataformas
        /// </summary>
        public class PlataformaResumenDto
        {
            public int IdPlataforma { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public bool Activa { get; set; }
            public int TotalProyectos { get; set; }
            public int ProyectosActivos { get; set; }
            public DateTime FechaCreacion { get; set; }
        }

        /// <summary>
        /// DTO para estadísticas de proyecto
        /// </summary>
        public class ProyectoEstadisticasDto
        {
            public int IdProyecto { get; set; }
            public string NombreProyecto { get; set; } = string.Empty;
            public string NombrePlataforma { get; set; } = string.Empty;
            public int TotalEntregables { get; set; }
            public int EntregablesActivos { get; set; }
            public int EntregablesInactivos { get; set; }
            public int TotalLideres { get; set; }
            public int LideresActivos { get; set; }
            public DateTime FechaInicio { get; set; }
            public DateTime? FechaFin { get; set; }
            public int DiasTranscurridos { get; set; }
            public int? DiasRestantes { get; set; }
            public DateTime FechaConsulta { get; set; }
        }

        /// <summary>
        /// DTO para resumen de proyectos agrupados por plataforma
        /// Usado en reportes y dashboards ejecutivos
        /// </summary>
        public class ProyectoPorPlataformaDto
        {
            /// <summary>
            /// ID único de la plataforma
            /// </summary>
            public int IdPlataforma { get; set; }

            /// <summary>
            /// Nombre de la plataforma
            /// </summary>
            public string NombrePlataforma { get; set; } = string.Empty;

            /// <summary>
            /// Total de proyectos (activos + inactivos) en esta plataforma
            /// </summary>
            public int TotalProyectos { get; set; }

            /// <summary>
            /// Número de proyectos activos en esta plataforma
            /// </summary>
            public int ProyectosActivos { get; set; }

            /// <summary>
            /// Número de proyectos inactivos en esta plataforma
            /// </summary>
            public int ProyectosInactivos { get; set; }

            /// <summary>
            /// Total de entregables de todos los proyectos de esta plataforma
            /// </summary>
            public int TotalEntregables { get; set; }

            /// <summary>
            /// Nombre del proyecto que tiene más entregables en esta plataforma
            /// </summary>
            public string ProyectoConMasEntregables { get; set; } = string.Empty;

            /// <summary>
            /// Porcentaje de proyectos activos (calculado automáticamente)
            /// </summary>
            public decimal PorcentajeProyectosActivos => TotalProyectos > 0
                ? Math.Round((decimal)ProyectosActivos / TotalProyectos * 100, 2)
                : 0;

            /// <summary>
            /// Promedio de entregables por proyecto (calculado automáticamente)
            /// </summary>
            public decimal PromedioEntregablesPorProyecto => TotalProyectos > 0
                ? Math.Round((decimal)TotalEntregables / TotalProyectos, 2)
                : 0;

            /// <summary>
            /// Estado general de la plataforma basado en porcentaje de proyectos activos
            /// </summary>
            public string EstadoGeneral => PorcentajeProyectosActivos switch
            {
                >= 80 => "Excelente",
                >= 60 => "Bueno",
                >= 40 => "Regular",
                >= 20 => "Bajo",
                _ => "Crítico"
            };

            /// <summary>
            /// Indicador de productividad basado en entregables por proyecto
            /// </summary>
            public string IndicadorProductividad => PromedioEntregablesPorProyecto switch
            {
                >= 5 => "Alto",
                >= 3 => "Medio",
                >= 1 => "Bajo",
                _ => "Sin Entregables"
            };
        }

        /// <summary>
        /// DTO extendido para reportes más detallados de plataforma
        /// </summary>
        public class ProyectoPorPlataformaDetalladoDto : ProyectoPorPlataformaDto
        {
            /// <summary>
            /// Lista de nombres de proyectos activos
            /// </summary>
            public List<string> NombresProyectosActivos { get; set; } = new List<string>();

            /// <summary>
            /// Lista de nombres de proyectos inactivos
            /// </summary>
            public List<string> NombresProyectosInactivos { get; set; } = new List<string>();

            /// <summary>
            /// Número de líderes únicos trabajando en esta plataforma
            /// </summary>
            public int LideresUnicos { get; set; }

            /// <summary>
            /// Fecha del proyecto más antiguo
            /// </summary>
            public DateTime? FechaProyectoMasAntiguo { get; set; }

            /// <summary>
            /// Fecha del proyecto más reciente
            /// </summary>
            public DateTime? FechaProyectoMasReciente { get; set; }

            /// <summary>
            /// Proyectos que están próximos a vencer (fecha fin en los próximos 30 días)
            /// </summary>
            public int ProyectosProximosAVencer { get; set; }

            /// <summary>
            /// Proyectos sin fecha de fin definida
            /// </summary>
            public int ProyectosSinFechaFin { get; set; }
        }

        /// <summary>
        /// DTO para métricas comparativas entre plataformas
        /// </summary>
        public class ComparativaPlataformasDto
        {
            /// <summary>
            /// Lista de plataformas con sus métricas
            /// </summary>
            public List<ProyectoPorPlataformaDto> Plataformas { get; set; } = new List<ProyectoPorPlataformaDto>();

            /// <summary>
            /// Plataforma con mayor número de proyectos
            /// </summary>
            public string PlataformaConMasProyectos { get; set; } = string.Empty;

            /// <summary>
            /// Plataforma con mayor porcentaje de proyectos activos
            /// </summary>
            public string PlataformaMasActiva { get; set; } = string.Empty;

            /// <summary>
            /// Plataforma con mayor productividad (más entregables por proyecto)
            /// </summary>
            public string PlataformaMasProductiva { get; set; } = string.Empty;

            /// <summary>
            /// Total de proyectos en todas las plataformas
            /// </summary>
            public int TotalProyectosGlobal => Plataformas.Sum(p => p.TotalProyectos);

            /// <summary>
            /// Total de entregables en todas las plataformas
            /// </summary>
            public int TotalEntregablesGlobal => Plataformas.Sum(p => p.TotalEntregables);

            /// <summary>
            /// Promedio global de proyectos por plataforma
            /// </summary>
            public decimal PromedioProyectosPorPlataforma => Plataformas.Count > 0
                ? Math.Round((decimal)TotalProyectosGlobal / Plataformas.Count, 2)
                : 0;

            /// <summary>
            /// Fecha de generación del reporte
            /// </summary>
            public DateTime FechaReporte { get; set; } = DateTime.UtcNow;
        }

  
    }
}
