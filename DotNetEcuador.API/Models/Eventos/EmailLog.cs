using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DotNetEcuador.API.Models.Eventos;

public class EmailLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("registroId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string RegistroId { get; set; } = string.Empty;

    [BsonElement("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [BsonElement("destinatario")]
    public string Destinatario { get; set; } = string.Empty;

    [BsonElement("enviadoEn")]
    public DateTime EnviadoEn { get; set; } = DateTime.UtcNow;

    [BsonElement("exitoso")]
    public bool Exitoso { get; set; }

    [BsonElement("error")]
    public string? Error { get; set; }
}

public static class TipoEmail
{
    public const string ConfirmacionPendiente = "confirmacion-pendiente";
    public const string ConfirmacionPagada = "confirmacion-pagada";
    public const string Rechazo = "rechazo";
}
