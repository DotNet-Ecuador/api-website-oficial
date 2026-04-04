using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DotNetEcuador.API.Models.Eventos;

public class Registro
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("eventoId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string EventoId { get; set; } = string.Empty;

    [BsonElement("asistenteId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string AsistenteId { get; set; } = string.Empty;

    [BsonElement("estado")]
    public string Estado { get; set; } = EstadoRegistro.Pendiente;

    [BsonElement("idCorto")]
    public string IdCorto { get; set; } = string.Empty;

    [BsonElement("referenciaPago")]
    public string? ReferenciaPago { get; set; }

    [BsonElement("comprobanteUrl")]
    public string? ComprobanteUrl { get; set; }

    [BsonElement("tokenQr")]
    public string TokenQr { get; set; } = string.Empty;

    [BsonElement("sessionToken")]
    public string SessionToken { get; set; } = string.Empty;

    [BsonElement("notasAdmin")]
    public string? NotasAdmin { get; set; }

    [BsonElement("registradoEn")]
    public DateTime RegistradoEn { get; set; } = DateTime.UtcNow;

    [BsonElement("confirmadoEn")]
    public DateTime? ConfirmadoEn { get; set; }
}

public static class EstadoRegistro
{
    public const string Pendiente = "pendiente";
    public const string Pagado = "pagado";
    public const string Rechazado = "rechazado";
    public const string Cancelado = "cancelado";
}
