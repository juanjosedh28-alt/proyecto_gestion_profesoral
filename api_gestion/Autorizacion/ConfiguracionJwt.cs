namespace ApiGestion.Autorizacion;

/// <summary>
/// (v3) Los datos con que se FIRMA y se VALIDA el token.
///
/// - Key: la clave secreta de la firma. Es un SECRETO: no está en el código ni
///   en appsettings.json; llega por la variable de entorno Jwt__Key, que
///   docker-compose lee del archivo .env (JWT_KEY). Con ella se fabrica un
///   token válido, así que quien la tenga puede hacerse pasar por cualquiera.
/// - Issuer / Audience: quién emite el token y para quién es. La API rechaza
///   un token firmado con la misma clave pero emitido para otra aplicación.
/// - DuracionMinutos: cuánto vive el token. Después de eso, 401 y hay que
///   volver a iniciar sesión.
/// </summary>
public class ConfiguracionJwt
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int DuracionMinutos { get; set; } = 60;
}
