using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace front_razor.Infraestructura;

/// <summary>
/// (v2) Convierte los errores de validación del formulario en frases para la pantalla.
///
/// POR QUÉ: en los formularios de VARIOS renglones los campos se llaman
/// "estudios[0].Titulo", "estudios[1].Titulo"... Si la validación falla, esa
/// clave no le dice nada a la persona. Aquí se traduce a "Renglón 2: El título es
/// obligatorio." (los renglones se cuentan desde 1, como los ve la persona).
/// </summary>
public static class ErroresFormulario
{
    public static List<string> Leer(ModelStateDictionary estado)
    {
        var mensajes = new List<string>();
        foreach (var (clave, entrada) in estado)
        {
            foreach (var error in entrada.Errors)
            {
                var texto = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Hay un dato con un formato que no se reconoce." : error.ErrorMessage;
                var renglon = Regex.Match(clave, @"\[(\d+)\]");
                mensajes.Add(renglon.Success ? $"Renglón {int.Parse(renglon.Groups[1].Value) + 1}: {texto}" : texto);
            }
        }
        return mensajes.Distinct().ToList();
    }
}
