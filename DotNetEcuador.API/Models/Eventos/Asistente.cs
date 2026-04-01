using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DotNetEcuador.API.Models.Eventos;

public class Asistente
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("empresa")]
    public string Empresa { get; set; } = string.Empty;

    [BsonElement("cargo")]
    public string Cargo { get; set; } = string.Empty;

    [BsonElement("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [BsonElement("aceptaMarketing")]
    public bool AceptaMarketing { get; set; }

    [BsonElement("creadoEn")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
