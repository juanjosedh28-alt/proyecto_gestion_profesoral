namespace front_razor.Infraestructura;

/// <summary>
/// (v3) La API respondió 401 a una petición QUE SÍ LLEVABA token: el token
/// venció o ya no es válido. El middleware ManejadorAcceso cierra la sesión y
/// manda a la persona a iniciar sesión de nuevo.
///
/// No hereda de ErrorApiException a propósito: las páginas atrapan
/// ErrorApiException para mostrar el error en el formulario, y esta no se
/// puede quedar atrapada en una página: tiene que llegar al middleware.
/// </summary>
public class SesionVencidaException : Exception
{
    public SesionVencidaException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// (v3) La API respondió 403: se sabe quién es la persona, y su rol no tiene
/// permiso. El middleware ManejadorAcceso muestra la página «No tiene permiso»
/// con código 403. Tampoco hereda de ErrorApiException, por la misma razón.
/// Lleva el mensaje y el detalle de la API («permiso requerido: interfaz.x»).
/// </summary>
public class AccesoDenegadoException : Exception
{
    public List<string> Detalle { get; }

    public AccesoDenegadoException(string mensaje, List<string>? detalle = null) : base(mensaje) =>
        Detalle = detalle ?? new List<string>();
}
