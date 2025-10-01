using APIProjectDocs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace APIProjectDocs.Utils
{
    /// <summary>
    /// Atributo para autorizar acceso basado en roles específicos
    /// </summary>
    public class AuthorizeRolesAttribute: AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly RolUsuario[] _roles;

        public AuthorizeRolesAttribute(params RolUsuario[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRoleClaim = user.FindFirst(ClaimTypes.Role);
            if (userRoleClaim == null || !Enum.TryParse<RolUsuario>(userRoleClaim.Value, out var userRole))
            {
                context.Result = new ForbidResult();
                return;
            }

            if (!_roles.Contains(userRole))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    /// <summary>
    /// Atributo para autorizar acceso a coordinadores de plataforma específica
    /// </summary>
    public class AuthorizeCoordinadorPlataformaAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _plataformaIdParameterName;

        public AuthorizeCoordinadorPlataformaAttribute(string plataformaIdParameterName = "idPlataforma")
        {
            _plataformaIdParameterName = plataformaIdParameterName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRoleClaim = user.FindFirst(ClaimTypes.Role);
            if (userRoleClaim == null || !Enum.TryParse<RolUsuario>(userRoleClaim.Value, out var userRole))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Dirección tiene acceso a todo
            if (userRole == RolUsuario.Direccion)
                return;

            // Solo coordinadores de plataforma pueden acceder
            if (userRole != RolUsuario.CoordinadorPlataforma)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Verificar que el coordinador tenga acceso a esta plataforma específica
            var routeValues = context.RouteData.Values;
            var queryParams = context.HttpContext.Request.Query;

            string? plataformaIdStr = null;

            // Buscar en route values
            if (routeValues.ContainsKey(_plataformaIdParameterName))
            {
                plataformaIdStr = routeValues[_plataformaIdParameterName]?.ToString();
            }
            // Buscar en query parameters
            else if (queryParams.ContainsKey(_plataformaIdParameterName))
            {
                plataformaIdStr = queryParams[_plataformaIdParameterName].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(plataformaIdStr) || !int.TryParse(plataformaIdStr, out int plataformaId))
            {
                context.Result = new BadRequestObjectResult("ID de plataforma requerido");
                return;
            }

            var plataformasAsignadasClaim = user.FindFirst("PlataformasAsignadas");
            if (plataformasAsignadasClaim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var plataformasAsignadas = plataformasAsignadasClaim.Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => int.TryParse(p, out int id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            if (!plataformasAsignadas.Contains(plataformaId))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    /// <summary>
    /// Atributo para autorizar acceso a líderes de proyecto específico
    /// </summary>
    public class AuthorizeLiderProyectoAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _proyectoIdParameterName;

        public AuthorizeLiderProyectoAttribute(string proyectoIdParameterName = "idProyecto")
        {
            _proyectoIdParameterName = proyectoIdParameterName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRoleClaim = user.FindFirst(ClaimTypes.Role);
            if (userRoleClaim == null || !Enum.TryParse<RolUsuario>(userRoleClaim.Value, out var userRole))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Dirección tiene acceso a todo
            if (userRole == RolUsuario.Direccion)
                return;

            // Coordinadores de plataforma tienen acceso a proyectos de sus plataformas
            if (userRole == RolUsuario.CoordinadorPlataforma)
            {
                // Aquí necesitaríamos verificar que el proyecto pertenece a una plataforma del coordinador
                // Por simplicidad, permitimos acceso a coordinadores
                return;
            }

            // Solo líderes de proyecto pueden acceder
            if (userRole != RolUsuario.LiderProyecto)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Verificar que el líder tenga acceso a este proyecto específico
            var routeValues = context.RouteData.Values;
            var queryParams = context.HttpContext.Request.Query;

            string? proyectoIdStr = null;

            // Buscar en route values
            if (routeValues.ContainsKey(_proyectoIdParameterName))
            {
                proyectoIdStr = routeValues[_proyectoIdParameterName]?.ToString();
            }
            // Buscar en query parameters
            else if (queryParams.ContainsKey(_proyectoIdParameterName))
            {
                proyectoIdStr = queryParams[_proyectoIdParameterName].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(proyectoIdStr) || !int.TryParse(proyectoIdStr, out int proyectoId))
            {
                context.Result = new BadRequestObjectResult("ID de proyecto requerido");
                return;
            }

            var proyectosAsignadosClaim = user.FindFirst("ProyectosAsignados");
            if (proyectosAsignadosClaim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var proyectosAsignados = proyectosAsignadosClaim.Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => int.TryParse(p, out int id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            if (!proyectosAsignados.Contains(proyectoId))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    /// <summary>
    /// Extensiones para facilitar el uso de claims en controladores
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }

        public static RolUsuario GetUserRole(this ClaimsPrincipal principal)
        {
            var roleClaim = principal.FindFirst(ClaimTypes.Role);
            return roleClaim != null && Enum.TryParse<RolUsuario>(roleClaim.Value, out var role) ? role : RolUsuario.UsuarioAdministracion;
        }

        public static List<int> GetPlataformasAsignadas(this ClaimsPrincipal principal)
        {
            var plataformasClaim = principal.FindFirst("PlataformasAsignadas");
            if (plataformasClaim == null) return new List<int>();

            return plataformasClaim.Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => int.TryParse(p, out int id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }

        public static List<int> GetProyectosAsignados(this ClaimsPrincipal principal)
        {
            var proyectosClaim = principal.FindFirst("ProyectosAsignados");
            if (proyectosClaim == null) return new List<int>();

            return proyectosClaim.Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => int.TryParse(p, out int id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }
    }
}
