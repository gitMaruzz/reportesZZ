using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIProjectDocs.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public RolUsuario Rol { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaModificacion { get; set; }

        // Relaciones
        public virtual ICollection<UsuarioPlataforma> UsuarioPlataformas { get; set; } = new List<UsuarioPlataforma>();
        public virtual ICollection<UsuarioProyecto> UsuarioProyectos { get; set; } = new List<UsuarioProyecto>();

        // Relaciones
        //public virtual ICollection<Plataforma> PlataformasAsignadas { get; set; } = new List<Plataforma>();

        //public virtual ICollection<Proyecto> ProyectosAsignados { get; set; } = new List<Proyecto>();
    }

    public enum RolUsuario
    {
        Direccion = 1,
        CoordinadorPlataforma = 2,
        LiderProyecto = 3,
        UsuarioAdministracion = 4
    }

    public class Plataforma
    {
        [Key]
        public int IdPlataforma { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public bool Activa { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        public virtual ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
        public virtual ICollection<UsuarioPlataforma> UsuarioPlataformas { get; set; } = new List<UsuarioPlataforma>();
    }

    public class UsuarioPlataforma
    {
        [Key]
        public int Id { get; set; }

        public int IdUsuario { get; set; }
        public int IdPlataforma { get; set; }

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey("IdPlataforma")]
        public virtual Plataforma Plataforma { get; set; } = null!;
    }

    public class Proyecto
    {
        [Key]
        public int IdProyecto { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public bool Activo { get; set; } = true;

        [Required]
        public int IdPlataforma { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey("IdPlataforma")]
        public virtual Plataforma Plataforma { get; set; } = null!;

        public virtual ICollection<Entregable> Entregables { get; set; } = new List<Entregable>();
        public virtual ICollection<UsuarioProyecto> UsuarioProyectos { get; set; } = new List<UsuarioProyecto>();
    }

    public class UsuarioProyecto
    {
        [Key]
        public int Id { get; set; }

        public int IdUsuario { get; set; }
        public int IdProyecto { get; set; }

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey("IdProyecto")]
        public virtual Proyecto Proyecto { get; set; } = null!;
    }

    public class Entregable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEntregable { get; set; }

        [Required]
        public int IdProyecto { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Titulo { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Required]
        public DateTime FechaDisponibilidad { get; set; }

        [Required]
        public OrigenDatos OrigenDatos { get; set; }

        [StringLength(500)]
        public string? ConfiguracionOrigen { get; set; } // Vista SQL o URL API

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaModificacion { get; set; }


        // Relaciones
        [ForeignKey("IdProyecto")]
        public virtual Proyecto Proyecto { get; set; } = null!;

        public virtual ICollection<ComprobantePago> ComprobantesPago { get; set; } = new List<ComprobantePago>();
    }

    public enum OrigenDatos
    {
        SqlServer = 1,
        ApiExterna = 2
    }


    public class ComprobantePago
    {
        [Key]
        public int IdComprobante { get; set; }

        [Required]
        public int IdEntregable { get; set; }

        [Required]
        public DateTime FechaDocumento { get; set; }

        [Required]
        [StringLength(500)]
        public string RutaArchivoPDF { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NombreArchivoOriginal { get; set; }

        public long TamanoArchivo { get; set; }

        public int IdUsuarioSubida { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey("IdEntregable")]
        public virtual Entregable Entregable { get; set; } = null!;

        [ForeignKey("IdUsuarioSubida")]
        public virtual Usuario UsuarioSubida { get; set; } = null!;
    }
}
