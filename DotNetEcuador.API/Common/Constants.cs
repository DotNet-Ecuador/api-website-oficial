namespace DotNetEcuador.API.Common;

public static class Constants
{
    public const string MONGODATABASE = "dotnet_ecuador";

    public static class TimeZones
    {
        public const string Ecuador = "America/Guayaquil";
    }

    public static class MongoCollections
    {
        public const string USERS = "users";
        public const string AREAINTEREST = "area_interes";
        public const string COMMUNITYMEMBER = "community_member";
        public const string VOLUNTEERAPPLICATION = "volunteer_applications";
        public const string EVENTOS = "eventos";
        public const string ASISTENTES = "asistentes";
        public const string REGISTROS = "inscripciones_eventos";
        public const string EMAIL_LOG = "emailLog";
        public const string DATOS_PAGO = "datos_pago";
        public const string PROMO_CODES = "promo_codes";
    }
}
