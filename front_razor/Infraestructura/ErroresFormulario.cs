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

    /// <summary>
    /// Pasa el error de la API al formulario, para que lo muestre el
    /// resumen de validación (asp-validation-summary="ModelOnly").
    /// - 409 o 422 (dato repetido o inválido): un renglón por cada detalle que mandó la API.
    /// - Cualquier otro código: el mensaje general.
    /// El prefijo es el nombre de la propiedad del formulario en la página
    /// (normalmente "Formulario"); se deja para que todas las páginas lo
    /// llamen igual, aunque los mensajes van al resumen y no a un campo.
    /// </summary>
    public static void Aplicar(ModelStateDictionary estado, ErrorApiException ex, string prefijo)
    {
        var mensajes = ex.EsDeFormulario ? ex.Mensajes() : new[] { ex.Message };
        foreach (var mensaje in mensajes) estado.AddModelError(string.Empty, mensaje);
    }
}
