using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DotNetEcuador.API.Models.Mentorias;

public class SolicitudMentoria
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("nombreCompleto")]
    public string NombreCompleto { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [BsonElement("institucionId")]
    public string InstitucionId { get; set; } = string.Empty;

    [BsonElement("institucionNombre")]
    public string InstitucionNombre { get; set; } = string.Empty;

    [BsonElement("otraInstitucion")]
    public string? OtraInstitucion { get; set; }

    [BsonElement("temaConsulta")]
    public string TemaConsulta { get; set; } = string.Empty;

    [BsonElement("estado")]
    public string Estado { get; set; } = EstadoSolicitud.Pendiente;

    [BsonElement("creadaEn")]
    public DateTime CreadaEn { get; set; }
}

public static class EstadoSolicitud
{
    public const string Pendiente = "pendiente";
    public const string Atendida = "atendida";
    public const string Cancelada = "cancelada";
}
