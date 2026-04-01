namespace DotNetEcuador.API.Infraestructure.Services;

public interface IFileStorageService
{
    Task<string> GuardarComprobanteAsync(IFormFile archivo, string registroId);
    Task<(Stream stream, string contentType, string fileName)> ObtenerComprobanteAsync(string rutaRelativa);
}
