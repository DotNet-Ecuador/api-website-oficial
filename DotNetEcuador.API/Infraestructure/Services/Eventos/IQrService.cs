namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public interface IQrService
{
    byte[] GenerarQr(string tokenQr);
    string GenerarQrBase64(string tokenQr);
}
