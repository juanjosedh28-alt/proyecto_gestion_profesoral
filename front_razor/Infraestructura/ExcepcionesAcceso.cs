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
/// </summary>
public class AccesoDenegadoException : Exception
{
    public AccesoDenegadoException(string mensaje) : base(mensaje) { }
}
