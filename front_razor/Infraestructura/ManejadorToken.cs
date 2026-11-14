using System.Net.Http.Headers;

namespace front_razor.Infraestructura;

/// <summary>
/// (v3) Le pone el token a CADA petición que el front le hace a la API:
///
///     Authorization: Bearer eyJhbGciOi...
///
/// «Bearer» significa «portador»: quien porta este token, entra (hasta que
/// venza). Por eso el token se cuida como una contraseña y vive solo en la
/// sesión del servidor.
///
/// Está registrado en todos los clientes HTTP (Program.cs), así que ninguna
/// página tiene que acordarse de mandarlo. Si no hay sesión (por ejemplo, en
/// el inicio de sesión), la petición sale sin token.
/// </summary>
public class ManejadorToken : DelegatingHandler
{
    private readonly IHttpContextAccessor _contexto;

    public ManejadorToken(IHttpContextAccessor contexto) => _contexto = contexto;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
    {
        var sesion = _contexto.HttpContext?.Session;
        var token = sesion is null ? null : SesionUsuario.Token(sesion);
        if (!string.IsNullOrEmpty(token))
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(peticion, cancelacion);
    }
}
