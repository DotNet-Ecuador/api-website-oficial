namespace DotNetEcuador.API.Common;

public static class MessageKeys
{
    // Validation messages
    public const string ValidationFullName = "v.fullName";
    public const string ValidationEmail = "v.email";
    public const string ValidationAreas = "v.areas";
    public const string ValidationOtherAreas = "v.otherAreas";

    // Success messages
    public const string SuccessVolunteerSent = "s.volunteerSent";
    public const string SuccessMemberAdded = "s.memberAdded";

    // Error messages
    public const string ErrorServer = "e.server";
    public const string ErrorToken = "e.token";
    public const string ErrorUnauthorized = "e.unauthorized";
    public const string ErrorInvalidToken = "e.invalidToken";
}