using System.Security.Claims;
using System.Text;
using ApiGestion.Autorizacion;
using ApiGestion.Modelos;
using ApiGestion.Repositorios;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;   // el paquete BCrypt.Net-Next

namespace ApiGestion.Servicios;

/// <summary>Contrato del inicio de sesión (v3).</summary>
public interface IServicioSesion
{
    /// <summary>La sesión con su token si el correo y la contraseña son correctos; si no, null.</summary>
    Task<Sesion?> IniciarAsync(string email, string contrasena);
}

/// <summary>
/// (v3) AUTENTICACIÓN: «¿quién es usted?».
///
/// QUÉ HACE IniciarAsync:
///   1. Trae de la base el hash guardado para ese correo (sp_credenciales_usuario).
///   2. Compara la contraseña escrita con ese hash usando bcrypt.
///   3. Si no coincide —o el correo no existe, o está retirado— devuelve null.
///      Los tres casos son IGUALES para quien llama: decir «ese correo no
///      existe» le confirmaría a un desconocido qué correos sí existen.
///   4. Si coincide, trae los nombres de sus roles y arma el token firmado.
///
/// BCRYPT: la base no guarda «admin123», guarda «$2a$12$…». Del hash no se
/// puede volver a la contraseña; BC.Verify toma el salt y el costo del hash
/// guardado, calcula el hash de lo escrito con esos mismos datos y compara.
/// </summary>
public class ServicioSesion : IServicioSesion
{
    // Un hash cualquiera para gastar el MISMO tiempo cuando el correo no existe:
    // así el tiempo de respuesta tampoco delata qué correos están registrados.
    private const string HashDeRelleno = "$2a$12$P93xWWYZySE9L/XEum6DO.qO5eEvaObQe0DA9kQbQmu.6PQqg7qz.";

    private readonly IRepositorioAcceso _acceso;
    private readonly ConfiguracionJwt _jwt;

    public ServicioSesion(IRepositorioAcceso acceso, ConfiguracionJwt jwt)
    {
        _acceso = acceso;
        _jwt = jwt;
    }

    public async Task<Sesion?> IniciarAsync(string email, string contrasena)
    {
        email = email.Trim();
        var credenciales = await _acceso.CredencialesAsync(email);

        if (credenciales is null)
        {
            Comparar(contrasena, HashDeRelleno);   // mismo trabajo, mismo resultado: null
            return null;
        }
        if (!Comparar(contrasena, credenciales.Hash)) return null;

        var roles = await _acceso.RolesAsync(email);
        var expira = DateTime.UtcNow.AddMinutes(_jwt.DuracionMinutos);

        return new Sesion
        {
            Token = ArmarToken(credenciales.Email, roles, expira),
            Email = credenciales.Email,
            NombreCompleto = credenciales.NombreCompleto ?? credenciales.Email,
            Roles = roles,
            Expira = expira
        };
    }

    /// <summary>bcrypt: true si la contraseña escrita produce el hash guardado.</summary>
    private static bool Comparar(string contrasena, string hash)
    {
        try { return BC.Verify(contrasena, hash); }
        catch (BCrypt.Net.SaltParseException) { return false; }   // lo guardado no es un hash bcrypt
    }

    /// <summary>
    /// Arma el JWT: cabecera.contenido.firma.
    /// - CONTENIDO (claims): el correo (sub y name) y un "role" por rol. Nada
    ///   privado: el token está FIRMADO, no cifrado, y cualquiera lo lee.
    /// - FIRMA: HMAC-SHA256 con la clave secreta Jwt:Key. Sin esa clave nadie
    ///   puede fabricar ni alterar un token: si cambia una letra, la firma no
    ///   cuadra y la API responde 401.
    /// - Los PERMISOS no van: se consultan en cada petición (criterio 7).
    /// </summary>
    private string ArmarToken(string email, List<string> roles, DateTime expira)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, email),
            new("name", email),
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = expira,
            SigningCredentials = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
