@page
@model front_razor.Pages.EvaluacionesDocente.CrearModel
@{ ViewData["Title"] = "Nueva Evaluación"; }

<h1 class="h4 mb-3">Registrar Evaluación Docente</h1>
<form method="post" class="col-md-6">
    <div asp-validation-summary="ModelOnly" class="text-danger mb-2"></div>
    <div class="mb-3">
        <label class="form-label">Docente</label>
        <select asp-for="Formulario.Docente" class="form-select">
            <option value="">-- Seleccione un docente --</option>
            @foreach (var d in Model.Docentes) { <option value="@d.Cedula">@d.NombreCompleto (@d.Cedula)</option> }
        </select>
        <span asp-validation-for="Formulario.Docente" class="text-danger"></span>
    </div>
    <div class="mb-3"><label class="form-label">Semestre</label><input asp-for="Formulario.Semestre" class="form-control" placeholder="2026-1" /><span asp-validation-for="Formulario.Semestre" class="text-danger"></span></div>
    <div class="mb-3"><label class="form-label">Calificación (0 a 5)</label><input asp-for="Formulario.Calificacion" type="number" step="0.1" class="form-control" /><span asp-validation-for="Formulario.Calificacion" class="text-danger"></span></div>
    <button class="btn btn-primary">Guardar</button>
    <a class="btn btn-secondary" href="/EvaluacionesDocente">Cancelar</a>
</form>
