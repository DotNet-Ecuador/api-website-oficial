using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DotNetEcuador.API.Models.Eventos;

public class Evento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [BsonElement("fechaEvento")]
    public DateTime FechaEvento { get; set; }

    [BsonElement("fechaFin")]
    public DateTime? FechaFin { get; set; }

    [BsonElement("lugar")]
    public string Lugar { get; set; } = string.Empty;

    [BsonElement("precio")]
    public decimal Precio { get; set; }

    [BsonElement("capacidadMaxima")]
    public int CapacidadMaxima { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;

    [BsonElement("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [BsonElement("subtipo")]
    public string Subtipo { get; set; } = string.Empty;

    [BsonElement("formato")]
    public string Formato { get; set; } = string.Empty;

    [BsonElement("networking")]
    public bool Networking { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("speakers")]
    public List<Speaker> Speakers { get; set; } = new();

    [BsonElement("coverImage")]
    public string CoverImage { get; set; } = string.Empty;

    [BsonElement("hostedBy")]
    public string? HostedBy { get; set; }

    [BsonElement("partnerEvento")]
    public string? PartnerEvento { get; set; }

    [BsonElement("datosTransferencia")]
    public DatosTransferencia DatosTransferencia { get; set; } = new();

    [BsonElement("creadoEn")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [BsonElement("actualizadoEn")]
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}

public class Speaker
{
    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("rol")]
    public string Rol { get; set; } = string.Empty;

    [BsonElement("avatar")]
    public string Avatar { get; set; } = string.Empty;
}
