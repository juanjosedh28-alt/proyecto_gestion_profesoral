namespace front_razor.Infraestructura;

/// <summary>Toda respuesta no exitosa de la API (o la API caída = 503) llega a las páginas como esta excepción.</summary>
public class ErrorApiException : Exception
{
    public int Codigo { get; }
    public List<string> Detalle { get; }

    public ErrorApiException(int codigo, string mensaje, List<string>? detalle = null) : base(mensaje)
    {
        Codigo = codigo;
        Detalle = detalle ?? new List<string>();
    }

    /// <summary>Errores a mostrar en el formulario: cada ítem de detalle o, si no hay, el mensaje.</summary>
    public IEnumerable<string> Mensajes() => Detalle.Count > 0 ? Detalle : new List<string> { Message };

    /// <summary>409 y 422 se muestran en el formulario; los demás van a TempData y redirigen.</summary>
    public bool EsDeFormulario => Codigo is 409 or 422;
}
