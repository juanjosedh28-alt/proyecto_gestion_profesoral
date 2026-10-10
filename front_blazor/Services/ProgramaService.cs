using System.Net;
using System.Net.Http.Json;
using FrontBlazor.Models;

namespace FrontBlazor.Services;

public class ProgramaService
{
    private readonly HttpClient _http;

    public ProgramaService(HttpClient http) => _http = http;

    public async Task<List<Programa>> ObtenerTodosAsync()
    {
        var res = await _http.GetAsync("api/programa");
        if (res.StatusCode == HttpStatusCode.NoContent) return new List<Programa>(); // la API responde 204 si no hay datos
        res.EnsureSuccessStatusCode();
        var cuerpo = await res.Content.ReadFromJsonAsync<RespuestaApi<Programa>>();
        return cuerpo?.Datos ?? new List<Programa>();
    }

    public async Task<bool> CrearAsync(Programa programa)
    {
        var res = await _http.PostAsJsonAsync("api/programa", programa);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/programa/{id}");
        return res.IsSuccessStatusCode;
    }
}

public class RespuestaApi<T>
{
    public string Tabla { get; set; } = string.Empty;
    public int Total { get; set; }
    public List<T> Datos { get; set; } = new();
}
