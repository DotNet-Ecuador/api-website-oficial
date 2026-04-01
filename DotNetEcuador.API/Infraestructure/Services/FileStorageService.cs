namespace DotNetEcuador.API.Infraestructure.Services;

public class FileStorageService : IFileStorageService
{
    private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        { "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { "image/png",  new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { "application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }
    };

    private static readonly Dictionary<string, string> ExtensionMap = new()
    {
        { "image/jpeg", ".jpg" },
        { "image/png",  ".png" },
        { "application/pdf", ".pdf" }
    };

    private readonly string _basePath;

    public FileStorageService()
    {
        _basePath = Environment.GetEnvironmentVariable("UPLOADS_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "uploads", "comprobantes");
    }

    public async Task<string> GuardarComprobanteAsync(IFormFile archivo, string registroId)
    {
        if (archivo is null || archivo.Length == 0)
            throw new InvalidOperationException("El archivo está vacío.");

        if (archivo.Length > MaxSizeBytes)
            throw new InvalidOperationException("El archivo supera el tamaño máximo de 5 MB.");

        var mimeReal = await DetectarMimeAsync(archivo).ConfigureAwait(false);
        if (mimeReal is null)
            throw new InvalidOperationException("Tipo de archivo no permitido. Solo se aceptan JPG, PNG o PDF.");

        var extension = ExtensionMap[mimeReal];
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var subDir = Path.Combine(DateTime.UtcNow.Year.ToString(), DateTime.UtcNow.Month.ToString("D2"));
        var directorioFisico = Path.Combine(_basePath, subDir);

        Directory.CreateDirectory(directorioFisico);

        var rutaFisica = Path.Combine(directorioFisico, nombreArchivo);
        using var stream = new FileStream(rutaFisica, FileMode.Create, FileAccess.Write);
        await archivo.CopyToAsync(stream).ConfigureAwait(false);

        return Path.Combine(subDir, nombreArchivo).Replace('\\', '/');
    }

    public Task<(Stream stream, string contentType, string fileName)> ObtenerComprobanteAsync(string rutaRelativa)
    {
        var rutaFisica = Path.Combine(_basePath, rutaRelativa.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(rutaFisica))
            throw new FileNotFoundException("Comprobante no encontrado.");

        var extension = Path.GetExtension(rutaFisica).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf"  => "application/pdf",
            ".png"  => "image/png",
            ".jpg"  => "image/jpeg",
            _       => "application/octet-stream"
        };

        Stream stream = new FileStream(rutaFisica, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult((stream, contentType, Path.GetFileName(rutaFisica)));
    }

    private static async Task<string?> DetectarMimeAsync(IFormFile archivo)
    {
        var header = new byte[8];
        using var stream = archivo.OpenReadStream();
        var leidos = await stream.ReadAsync(header.AsMemory(0, header.Length)).ConfigureAwait(false);
        if (leidos < 4) return null;

        foreach (var (mime, magic) in MagicBytes)
        {
            if (header.Take(magic.Length).SequenceEqual(magic))
                return mime;
        }

        return null;
    }
}
