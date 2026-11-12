using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

// ===================================================================
// Modelos de las TABLAS PUENTE (clave primaria compuesta, dos claves foráneas)
// y del detalle 1:1 del estudio (apoyo profesoral y beca).
//
// Las claves foráneas son int? y se eligen de un DESPLEGABLE: así, si la persona
// no elige nada, llega null y la validación dice "seleccione...", en vez de
// un error de conversión.
// ===================================================================

public class DocenteDepartamentoVista
{
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
    public int Departamento { get; set; }
    public string DepartamentoNombre { get; set; } = string.Empty;
    public string Dedicacion { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public string FechaIngreso { get; set; } = string.Empty;
    public string? FechaSalida { get; set; }
}

public class DocenteDepartamentoFormulario
{
    public int Docente { get; set; }
    [Required(ErrorMessage = "Seleccione un programa.")]
    public int? Departamento { get; set; }
    [Required(ErrorMessage = "La dedicación es obligatoria.")]
    [StringLength(15, ErrorMessage = "La dedicación no puede superar 15 caracteres.")]
    public string Dedicacion { get; set; } = string.Empty;
    [Required(ErrorMessage = "La modalidad es obligatoria.")]
    [StringLength(45, ErrorMessage = "La modalidad no puede superar 45 caracteres.")]
    public string Modalidad { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    public string FechaIngreso { get; set; } = string.Empty;
    public string? FechaSalida { get; set; }

    public static DocenteDepartamentoFormulario Desde(DocenteDepartamentoVista v) => new()
    {
        Docente = v.Docente, Departamento = v.Departamento, Dedicacion = v.Dedicacion, Modalidad = v.Modalidad,
        FechaIngreso = v.FechaIngreso, FechaSalida = v.FechaSalida
    };
}

public class RedDocenteVista
{
    public int Red { get; set; }
    public string RedNombre { get; set; } = string.Empty;
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaFin { get; set; }
    public string ActDestacadas { get; set; } = string.Empty;
}

public class RedDocenteFormulario
{
    [Required(ErrorMessage = "Seleccione una red.")]
    public int? Red { get; set; }
    public int Docente { get; set; }
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaFin { get; set; }
    [Required(ErrorMessage = "Las actividades destacadas son obligatorias.")]
    [StringLength(4000, ErrorMessage = "Las actividades destacadas no pueden superar 4000 caracteres.")]
    public string ActDestacadas { get; set; } = string.Empty;

    public static RedDocenteFormulario Desde(RedDocenteVista v) => new()
    {
        Red = v.Red, Docente = v.Docente, FechaInicio = v.FechaInicio, FechaFin = v.FechaFin, ActDestacadas = v.ActDestacadas
    };
}

public class InteresFuturoVista
{
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
    public string TerminoClave { get; set; } = string.Empty;
    public string? TerminoIngles { get; set; }
}

/// <summary>Puente puro: solo las dos claves. No tiene formulario de edición (una pareja existe o no).</summary>
public class InteresFuturoFormulario
{
    public int Docente { get; set; }
    [Required(ErrorMessage = "Seleccione un término clave.")]
    public string? TerminoClave { get; set; }
}

public class EstudioAcVista
{
    public int Estudio { get; set; }
    public string EstudioTitulo { get; set; } = string.Empty;
    public string AreaConocimiento { get; set; } = string.Empty;
    public string AreaConocimientoDisciplina { get; set; } = string.Empty;
}

/// <summary>Puente puro: el estudio sale de la ruta y el área de un desplegable.</summary>
public class EstudioAcFormulario
{
    public int Estudio { get; set; }
    [Required(ErrorMessage = "Seleccione un área de conocimiento.")]
    public string? AreaConocimiento { get; set; }
}

public class ApoyoProfesoralVista
{
    public int Estudios { get; set; }
    public string EstudioTitulo { get; set; } = string.Empty;
    public int ConApoyo { get; set; }
    public string Institucion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

public class ApoyoProfesoralFormulario
{
    public int Estudios { get; set; }
    [Range(0, 1, ErrorMessage = "Indique si tuvo apoyo.")]
    public int ConApoyo { get; set; } = 1;
    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(45, ErrorMessage = "La institución no puede superar 45 caracteres.")]
    public string Institucion { get; set; } = string.Empty;
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;

    public static ApoyoProfesoralFormulario Desde(ApoyoProfesoralVista v) => new()
    { Estudios = v.Estudios, ConApoyo = v.ConApoyo, Institucion = v.Institucion, Tipo = v.Tipo };
}

public class BecaVista
{
    public int Estudios { get; set; }
    public string EstudioTitulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Institucion { get; set; } = string.Empty;
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaFin { get; set; }
}

public class BecaFormulario
{
    public int Estudios { get; set; }
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(80, ErrorMessage = "La institución no puede superar 80 caracteres.")]
    public string Institucion { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaFin { get; set; }

    public static BecaFormulario Desde(BecaVista v) => new()
    { Estudios = v.Estudios, Tipo = v.Tipo, Institucion = v.Institucion, FechaInicio = v.FechaInicio, FechaFin = v.FechaFin };
}
