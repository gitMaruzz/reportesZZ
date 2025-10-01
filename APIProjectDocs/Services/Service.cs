using APIProjectDocs.Models;
using static APIProjectDocs.DTOs.DTO;

namespace APIProjectDocs.Services
{
    public class Service
    {
        public interface IAuthService
        {
            Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request);
            Task<ApiResponseDto<string>> GenerateTokenAsync(Usuario usuario);
            Task<ApiResponseDto<Usuario>> ValidateTokenAsync(string token);
        }

        public interface IUsuarioService
        {
            Task<ApiResponseDto<PagedResultDto<UsuarioDto>>> GetAllAsync(int page = 1, int pageSize = 10);
            Task<ApiResponseDto<UsuarioDto>> GetByIdAsync(int id);
            Task<ApiResponseDto<UsuarioDto>> GetByEmailAsync(string email);
            Task<ApiResponseDto<UsuarioDto>> CreateAsync(CreateUsuarioDto dto);
            Task<ApiResponseDto<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto);
            Task<ApiResponseDto<bool>> DeleteAsync(int id);
            Task<ApiResponseDto<bool>> ChangePasswordAsync(int id, string currentPassword, string newPassword);
        }

        public interface IPlataformaService
        {
            Task<ApiResponseDto<PagedResultDto<PlataformaDto>>> GetAllAsync(int page = 1, int pageSize = 10, bool? activa = null);
            Task<ApiResponseDto<PlataformaDto>> GetByIdAsync(int id);
            Task<ApiResponseDto<PlataformaDto>> CreateAsync(CreatePlataformaDto dto);
            Task<ApiResponseDto<PlataformaDto>> UpdateAsync(int id, UpdatePlataformaDto dto);
            Task<ApiResponseDto<bool>> DeleteAsync(int id);
            Task<ApiResponseDto<bool>> AsignarCoordinadorAsync(int idPlataforma, int idUsuario);
            Task<ApiResponseDto<bool>> DesasignarCoordinadorAsync(int idPlataforma, int idUsuario);
            Task<ApiResponseDto<List<UsuarioDto>>> GetCoordinadoresAsync(int idPlataforma);
            Task<ApiResponseDto<List<PlataformaDto>>> GetPlataformasByCoordinadorAsync(int idUsuario);
        }

        public interface IProyectoService
        {
            Task<ApiResponseDto<PagedResultDto<ProyectoDto>>> GetAllAsync(int page = 1, int pageSize = 10, bool? activo = null);
            Task<ApiResponseDto<PagedResultDto<ProyectoDto>>> GetByPlataformaAsync(int idPlataforma, int page = 1, int pageSize = 10, bool? activo = null);
            Task<ApiResponseDto<PagedResultDto<ProyectoDto>>> GetByLiderAsync(int idUsuario, int page = 1, int pageSize = 10, bool? activo = null);
            Task<ApiResponseDto<ProyectoDto>> GetByIdAsync(int id);
            Task<ApiResponseDto<ProyectoDto>> CreateAsync(CreateProyectoDto dto);
            Task<ApiResponseDto<ProyectoDto>> UpdateAsync(int id, UpdateProyectoDto dto);
            Task<ApiResponseDto<bool>> DeleteAsync(int id);
            Task<ApiResponseDto<bool>> AsignarLiderAsync(int idProyecto, int idUsuario);
            Task<ApiResponseDto<bool>> DesasignarLiderAsync(int idProyecto, int idUsuario);
            Task<ApiResponseDto<List<UsuarioDto>>> GetLideresAsync(int idProyecto);
        }

        public interface IEntregableService
        {
            // CRUD básico
            Task<ApiResponseDto<List<EntregableDto>>> GetAllAsync();
            Task<ApiResponseDto<EntregableDto>> GetByIdAsync(int id);
            Task<ApiResponseDto<List<EntregableDto>>> GetByProyectoAsync(int idProyecto);
            Task<ApiResponseDto<List<EntregableDto>>> GetByUserAsync(int idUsuario);
            Task<ApiResponseDto<EntregableDto>> CreateAsync(CreateEntregableDto createDto);
            Task<ApiResponseDto<EntregableDto>> UpdateAsync(int id, UpdateEntregableDto updateDto);
            Task<ApiResponseDto<bool>> DeleteAsync(int id);

            // Funcionalidades específicas
            //Task<ApiResponseDto<EntregableDisponibilidadDto>> CheckDisponibilidadAsync(int id);
            Task<ApiResponseDto<object>> GetEntregableDataAsync(int id);
            Task<ApiResponseDto<List<EntregableDto>>> GetEntregablesDisponiblesAsync();
            Task<ApiResponseDto<List<EntregableDto>>> GetEntregablesPendientesAsync();
            Task<ApiResponseDto<bool>> ActivarAsync(int id);
            Task<ApiResponseDto<bool>> DesactivarAsync(int id);
        }

        public interface IComprobantePagoService
        {
            Task<ApiResponseDto<PagedResultDto<ComprobantePagoDto>>> GetAllAsync(int page = 1, int pageSize = 10);
            Task<ApiResponseDto<PagedResultDto<ComprobantePagoDto>>> GetByEntregableAsync(int idEntregable, int page = 1, int pageSize = 10);
            Task<ApiResponseDto<ComprobantePagoDto>> GetByIdAsync(int id);
            Task<ApiResponseDto<ComprobantePagoDto>> CreateAsync(CreateComprobantePagoDto dto, int idUsuario);
            Task<ApiResponseDto<bool>> DeleteAsync(int id);
            Task<ApiResponseDto<byte[]>> DownloadAsync(int id);
        }

        /// <summary>
        /// Servicio para obtener datos desde diferentes orígenes (SQL Server y APIs externas)
        /// </summary>
        public interface IDataSourceService
        {
            /// <summary>
            /// Obtener datos desde una vista de SQL Server
            /// </summary>
            /// <param name="configuracion">JSON con la configuración de conexión y vista</param>
            /// <returns>Objeto dinámico con los datos obtenidos</returns>
            Task<object> GetFromSqlServerAsync(string configuracion);

            /// <summary>
            /// Obtener datos desde una API externa
            /// </summary>
            /// <param name="configuracion">JSON con la configuración de la API (URL, método, headers, etc.)</param>
            /// <returns>Objeto dinámico con los datos obtenidos</returns>
            Task<object> GetFromApiExternaAsync(string configuracion);

            /// <summary>
            /// Validar configuración de SQL Server
            /// </summary>
            /// <param name="configuracion">JSON con la configuración</param>
            /// <returns>True si la configuración es válida</returns>
            Task<bool> ValidarConfiguracionSqlServerAsync(string configuracion);

            /// <summary>
            /// Validar configuración de API externa
            /// </summary>
            /// <param name="configuracion">JSON con la configuración</param>
            /// <returns>True si la configuración es válida</returns>
            Task<bool> ValidarConfiguracionApiExternaAsync(string configuracion);

        }

        public interface IFileService
        {
            Task<ApiResponseDto<string>> SaveFileAsync(IFormFile file, string subfolder);
            Task<ApiResponseDto<bool>> DeleteFileAsync(string filePath);
            Task<ApiResponseDto<byte[]>> GetFileAsync(string filePath);
            bool IsValidPdfFile(IFormFile file);
            string GetUniqueFileName(string originalFileName);
        }
    }
}
