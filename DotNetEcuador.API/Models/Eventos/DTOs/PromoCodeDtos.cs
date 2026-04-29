namespace DotNetEcuador.API.Models.Eventos.DTOs;

public class PromoCodeValidateRequestDto
{
    public string Code { get; set; } = string.Empty;
}

public class PromoCodeValidateResponseDto
{
    public bool Valid { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AplicarPromoRequestDto
{
    public string Code { get; set; } = string.Empty;
}
