using MongoDB.Bson.Serialization.Attributes;

namespace DotNetEcuador.API.Models.Eventos;

public class DatosTransferencia
{
    [BsonElement("banco")]
    public string Banco { get; set; } = string.Empty;

    [BsonElement("tipoCuenta")]
    public string TipoCuenta { get; set; } = string.Empty;

    [BsonElement("numeroCuenta")]
    public string NumeroCuenta { get; set; } = string.Empty;

    [BsonElement("titular")]
    public string Titular { get; set; } = string.Empty;

    [BsonElement("ruc")]
    public string? Ruc { get; set; }
}
