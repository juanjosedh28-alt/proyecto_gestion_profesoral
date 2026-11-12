using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

// ===================================================================
// Modelos del DETALLE del docente (estudios, evaluaciones, experiencias,
// reconocimientos). Cada tabla tiene su "Vista" (lo que llega de la API)
// y su "Formulario" (lo que se envía).
//
// En los formularios, el campo Docente NO se escribe: lo pone la página con la
// cédula del docente que se está viendo (viene en la ruta). Por eso no lleva
// anotaciones. En el envío de varios renglones la API lo toma de la ruta.
// ===================================================================

public class EstudioVista
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Universidad { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public int InsAcreditada { get; set; }
    public string Metodologia { get; set; } = string.Empty;
    public string PerfilEgresado { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
}

public class EstudioFormulario
{
    [Required(ErrorMessage = "El id es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id debe ser mayor que 0.")]
    public int? Id { get; set; }
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(45, ErrorMessage = "El título no puede superar 45 caracteres.")]
    public string Titulo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La universidad es obligatoria.")]
    [StringLength(50, ErrorMessage = "La universidad no puede superar 50 caracteres.")]
    public string Universidad { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public string Fecha { get; set; } = string.Empty;
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(45, ErrorMessage = "La ciudad no puede superar 45 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;
    [Range(0, 1, ErrorMessage = "Indique si la institución es acreditada.")]
    public int InsAcreditada { get; set; } = 1;
    [Required(ErrorMessage = "La metodología es obligatoria.")]
    [StringLength(45, ErrorMessage = "La metodología no puede superar 45 caracteres.")]
    public string Metodologia { get; set; } = string.Empty;
    [Required(ErrorMessage = "El perfil del egresado es obligatorio.")]
    [StringLength(4000, ErrorMessage = "El perfil del egresado no puede superar 4000 caracteres.")]
    public string PerfilEgresado { get; set; } = string.Empty;
    [Required(ErrorMessage = "El país es obligatorio.")]
    [StringLength(45, ErrorMessage = "El país no puede superar 45 caracteres.")]
    public string Pais { get; set; } = string.Empty;
    public int Docente { get; set; }

    public static EstudioFormulario Desde(EstudioVista v) => new()
    {
        Id = v.Id, Titulo = v.Titulo, Universidad = v.Universidad, Fecha = v.Fecha, Tipo = v.Tipo, Ciudad = v.Ciudad,
        InsAcreditada = v.InsAcreditada, Metodologia = v.Metodologia, PerfilEgresado = v.PerfilEgresado, Pais = v.Pais,
        Docente = v.Docente
    };
}

public class EvaluacionVista
{
    public int Id { get; set; }
    public decimal Calificacion { get; set; }
    public string Semestre { get; set; } = string.Empty;
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
}

public class EvaluacionFormulario
{
    [Required(ErrorMessage = "La calificación es obligatoria.")]
    [Range(typeof(decimal), "0", "5", ErrorMessage = "La calificación debe estar entre 0 y 5.")]
    public decimal? Calificacion { get; set; }
    [Required(ErrorMessage = "El semestre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El semestre no puede superar 45 caracteres.")]
    public string Semestre { get; set; } = string.Empty;
    public int Docente { get; set; }

    public static EvaluacionFormulario Desde(EvaluacionVista v) => new()
    { Calificacion = v.Calificacion, Semestre = v.Semestre, Docente = v.Docente };
}

public class ExperienciaVista
{
    public int Id { get; set; }
    public string NombreCargo { get; set; } = string.Empty;
    public string Institucion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaFin { get; set; }
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
}

public class ExperienciaFormulario
{
    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El cargo no puede superar 45 caracteres.")]
    public string NombreCargo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(45, ErrorMessage = "La institución no puede superar 45 caracteres.")]
    public string Institucion { get; set; } = string.Empty;
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public string FechaInicio { get; set; } = string.Empty;
    /// <summary>Opcional: vacía significa que todavía está en el cargo.</summary>
    public string? FechaFin { get; set; }
    public int Docente { get; set; }

    public static ExperienciaFormulario Desde(ExperienciaVista v) => new()
    {
        NombreCargo = v.NombreCargo, Institucion = v.Institucion, Tipo = v.Tipo,
        FechaInicio = v.FechaInicio, FechaFin = v.FechaFin, Docente = v.Docente
    };
}

public class ReconocimientoVista
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Institucion { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ambito { get; set; } = string.Empty;
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
}

public class ReconocimientoFormulario
{
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public string Fecha { get; set; } = string.Empty;
    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(45, ErrorMessage = "La institución no puede superar 45 caracteres.")]
    public string Institucion { get; set; } = string.Empty;
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre no puede superar 45 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "El ámbito es obligatorio.")]
    [StringLength(45, ErrorMessage = "El ámbito no puede superar 45 caracteres.")]
    public string Ambito { get; set; } = string.Empty;
    public int Docente { get; set; }

    public static ReconocimientoFormulario Desde(ReconocimientoVista v) => new()
    {
        Tipo = v.Tipo, Fecha = v.Fecha, Institucion = v.Institucion, Nombre = v.Nombre,
        Ambito = v.Ambito, Docente = v.Docente
    };
}
