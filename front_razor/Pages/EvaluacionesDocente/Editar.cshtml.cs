@page
@model front_razor.Pages.EvaluacionesDocente.EditarModel
@{ ViewData["Title"] = "Editar Evaluación"; }

<h1 class="h4 mb-3">Editar Evaluación #@Model.Id</h1>
<form method="post" class="col-md-6">
    <input type="hidden" asp-for="Id" />
    <input type="hidden" asp-for="Formulario.Docente" />
    <div asp-validation-summary="ModelOnly" class="text-danger mb-2"></div>
    <div class="mb-3"><label class="form-label">Docente</label><input value="@Model.Formulario.Docente" class="form-control" disabled /></div>
    <div class="mb-3"><label class="form-label">Semestre</label><input asp-for="Formulario.Semestre" class="form-control" /><span asp-validation-for="Formulario.Semestre" class="text-danger"></span></div>
    <div class="mb-3"><label class="form-label">Calificación</label><input asp-for="Formulario.Calificacion" type="number" step="0.1" class="form-control" /><span asp-validation-for="Formulario.Calificacion" class="text-danger"></span></div>
    <button class="btn btn-primary">Actualizar</button>
    <a class="btn btn-secondary" href="/EvaluacionesDocente">Cancelar</a>
</form>
