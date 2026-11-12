namespace Inventario.Api.Services;

public enum EstadoProductoCatalogo
{
    Existe,
    NoExiste,
    NoDisponible
}

public sealed class CatalogoApiClient
{
    private readonly HttpClient _httpClient;

    public CatalogoApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EstadoProductoCatalogo> ConsultarProductoAsync(int idProducto, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"api/productos/{idProducto}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return EstadoProductoCatalogo.Existe;
            }

            return response.StatusCode == System.Net.HttpStatusCode.NotFound
                ? EstadoProductoCatalogo.NoExiste
                : EstadoProductoCatalogo.NoDisponible;
        }
        catch (HttpRequestException)
        {
            return EstadoProductoCatalogo.NoDisponible;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return EstadoProductoCatalogo.NoDisponible;
        }
    }
}
