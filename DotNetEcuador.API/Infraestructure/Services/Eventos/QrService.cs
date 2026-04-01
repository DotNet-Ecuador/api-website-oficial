using QRCoder;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public class QrService : IQrService
{
    public byte[] GenerarQr(string tokenQr)
    {
        var datos = $"DOTNET-EC|{tokenQr}|{DateTime.UtcNow:yyyyMMdd}";
        using var generator = new QRCodeGenerator();
        var qrData = generator.CreateQrCode(datos, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(
            pixelsPerModule: 10,
            darkColorRgba: [123, 47, 190, 255],
            lightColorRgba: [255, 255, 255, 255]);
    }

    public string GenerarQrBase64(string tokenQr)
        => Convert.ToBase64String(GenerarQr(tokenQr));
}
