using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text.Json;
using static APIProjectDocs.DTOs.DTO;
using static APIProjectDocs.Services.Service;

namespace APIProjectDocs.Services
{
    public class DataSourceService : IDataSourceService
    {
        private readonly ILogger<DataSourceService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public DataSourceService(
            ILogger<DataSourceService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Obtiene datos desde una vista de SQL Server
        /// </summary>
        public async Task<object> GetFromSqlServerAsync(string configuracion)
        {
            try
            {
                var config = JsonSerializer.Deserialize<SqlServerConfigDto>(configuracion);
                if (config == null)
                {
                    throw new ArgumentException("Configuración de SQL Server inválida");
                }

                var datos = new List<dynamic>();

                using (var connection = new SqlConnection(config.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(config.ViewName, connection))
                    {
                        command.CommandType = CommandType.Text;

                        // Si la vista tiene parámetros, agregarlos
                        if (config.Parameters != null && config.Parameters.Count > 0)
                        {
                            foreach (var param in config.Parameters)
                            {
                                command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                            }
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new ExpandoObject() as IDictionary<string, object>;

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var columnName = reader.GetName(i);
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    row[columnName] = value;
                                }

                                datos.Add(row);
                            }
                        }
                    }
                }

                return new
                {
                    Origen = "SQL Server",
                    Vista = config.ViewName,
                    TotalRegistros = datos.Count,
                    FechaConsulta = DateTime.UtcNow,
                    Datos = datos
                };
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error al consultar SQL Server: {Message}", ex.Message);
                throw new Exception($"Error al consultar SQL Server: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar configuración de SQL Server");
                throw new ArgumentException("Configuración de SQL Server con formato inválido", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener datos de SQL Server");
                throw;
            }
        }

        /// <summary>
        /// Obtiene datos desde una API externa
        /// </summary>
        public async Task<object> GetFromApiExternaAsync(string configuracion)
        {
            try
            {
                var config = JsonSerializer.Deserialize<ApiExternaConfigDto>(configuracion);
                if (config == null)
                {
                    throw new ArgumentException("Configuración de API externa inválida");
                }

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

                // Crear el request
                var request = new HttpRequestMessage
                {
                    Method = new HttpMethod(config.Method ?? "GET"),
                    RequestUri = new Uri(config.Url)
                };

                // Agregar headers si existen
                if (config.Headers != null && config.Headers.Count > 0)
                {
                    foreach (var header in config.Headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Agregar body si existe y el método lo permite
                if (config.Body != null &&
                    (config.Method == "POST" || config.Method == "PUT" || config.Method == "PATCH"))
                {
                    var jsonBody = JsonSerializer.Serialize(config.Body);
                    request.Content = new StringContent(
                        jsonBody,
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );
                }

                // Ejecutar la petición
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "API externa retornó código {StatusCode}: {Error}",
                        response.StatusCode,
                        errorContent
                    );
                    throw new HttpRequestException(
                        $"API externa retornó código {response.StatusCode}: {errorContent}"
                    );
                }

                var content = await response.Content.ReadAsStringAsync();
                var datos = JsonSerializer.Deserialize<object>(content);

                return new
                {
                    Origen = "API Externa",
                    Url = config.Url,
                    Metodo = config.Method,
                    StatusCode = (int)response.StatusCode,
                    FechaConsulta = DateTime.UtcNow,
                    Datos = datos
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al consultar API externa: {Message}", ex.Message);
                throw new Exception($"Error al consultar API externa: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout al consultar API externa");
                throw new Exception("La API externa no respondió en el tiempo esperado", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar configuración de API externa");
                throw new ArgumentException("Configuración de API externa con formato inválido", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener datos de API externa");
                throw;
            }
        }

        /// <summary>
        /// Valida configuración de SQL Server
        /// </summary>
        public async Task<bool> ValidarConfiguracionSqlServerAsync(string configuracion)
        {
            try
            {
                var config = JsonSerializer.Deserialize<SqlServerConfigDto>(configuracion);
                if (config == null)
                {
                    return false;
                }

                // Validar que tenga los campos requeridos
                if (string.IsNullOrWhiteSpace(config.ConnectionString) ||
                    string.IsNullOrWhiteSpace(config.ViewName))
                {
                    return false;
                }

                // Intentar conectar a la base de datos
                using (var connection = new SqlConnection(config.ConnectionString))
                {
                    await connection.OpenAsync();

                    // Verificar que la vista existe
                    var checkQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.VIEWS 
                        WHERE TABLE_NAME = @ViewName";

                    using (var command = new SqlCommand(checkQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ViewName", config.ViewName);
                        var exists = (int)await command.ExecuteScalarAsync();

                        if (exists == 0)
                        {
                            _logger.LogWarning(
                                "La vista {ViewName} no existe en la base de datos",
                                config.ViewName
                            );
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar configuración de SQL Server");
                return false;
            }
        }

        /// <summary>
        /// Valida configuración de API externa
        /// </summary>
        public async Task<bool> ValidarConfiguracionApiExternaAsync(string configuracion)
        {
            try
            {
                var config = JsonSerializer.Deserialize<ApiExternaConfigDto>(configuracion);
                if (config == null)
                {
                    return false;
                }

                // Validar que tenga URL
                if (string.IsNullOrWhiteSpace(config.Url))
                {
                    return false;
                }

                // Validar que la URL sea válida
                if (!Uri.TryCreate(config.Url, UriKind.Absolute, out var uri))
                {
                    _logger.LogWarning("URL inválida: {Url}", config.Url);
                    return false;
                }

                // Validar método HTTP
                var validMethods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE" };
                if (!string.IsNullOrWhiteSpace(config.Method) &&
                    !validMethods.Contains(config.Method.ToUpper()))
                {
                    _logger.LogWarning("Método HTTP inválido: {Method}", config.Method);
                    return false;
                }

                // Intentar hacer una petición de prueba (HEAD o OPTIONS)
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                try
                {
                    var response = await httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Head, config.Url)
                    );
                    // No importa el status code, solo que responda
                    return true;
                }
                catch (HttpRequestException)
                {
                    _logger.LogWarning("No se pudo conectar con la API: {Url}", config.Url);
                    return false;
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Timeout al validar API: {Url}", config.Url);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar configuración de API externa");
                return false;
            }
        }
    }
}
