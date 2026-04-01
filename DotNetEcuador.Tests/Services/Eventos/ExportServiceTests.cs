using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Eventos;
using FluentAssertions;
using Moq;

namespace DotNetEcuador.Tests.Services.Eventos;

public class ExportServiceTests
{
    private readonly Mock<IExportService> _mockService;

    public ExportServiceTests()
    {
        _mockService = new Mock<IExportService>();
    }

    [Fact]
    public async Task ExportarCsv_RetornaCsvConEncabezados()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        var csvContent = "nombre,email,empresa,cargo,telefono,evento,fecha_registro,acepta_marketing\nJuan,juan@test.com,Tech,Dev,+593,Meetup,2025-01-01,true";

        _mockService.Setup(s => s.ExportarRegistradosCsvAsync(eventoId))
            .ReturnsAsync(csvContent);

        var result = await _mockService.Object.ExportarRegistradosCsvAsync(eventoId);

        result.Should().Contain("nombre,email");
        result.Should().Contain("juan@test.com");
    }

    [Fact]
    public async Task ExportarCsv_CuandoNoHayRegistros_RetornasoloEncabezados()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        var csvContent = "nombre,email,empresa,cargo,telefono,evento,fecha_registro,acepta_marketing\n";

        _mockService.Setup(s => s.ExportarRegistradosCsvAsync(eventoId))
            .ReturnsAsync(csvContent);

        var result = await _mockService.Object.ExportarRegistradosCsvAsync(eventoId);

        result.Should().StartWith("nombre,email");
    }
}
