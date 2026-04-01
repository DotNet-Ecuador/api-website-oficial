using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Eventos;
using FluentAssertions;
using Moq;

namespace DotNetEcuador.Tests.Services.Eventos;

public class EventoServiceTests
{
    private readonly Mock<IRepository<Evento>> _mockEventoRepo;
    private readonly Mock<IRepository<Registro>> _mockRegistroRepo;
    private readonly IEventoService _service;

    public EventoServiceTests()
    {
        _mockEventoRepo = new Mock<IRepository<Evento>>();
        _mockRegistroRepo = new Mock<IRepository<Registro>>();
        _service = new EventoService(_mockEventoRepo.Object, _mockRegistroRepo.Object);
    }

    [Fact]
    public async Task GetBySlug_CuandoExiste_RetornaEvento()
    {
        var evento = new Evento { Id = "abc123", Slug = "meetup-mayo", Nombre = "Meetup Mayo", Activo = true };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(evento);

        var result = await _service.GetBySlugAsync("meetup-mayo");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("meetup-mayo");
    }

    [Fact]
    public async Task GetBySlug_CuandoNoExiste_RetornaNull()
    {
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync((Evento?)null);

        var result = await _service.GetBySlugAsync("no-existe");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCuposDisponibles_CuandoHayRegistros_RetornaCapacidadMenosOcupados()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        var evento = new Evento { Id = eventoId, CapacidadMaxima = 50 };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>())).ReturnsAsync(evento);

        var registros = Enumerable.Range(0, 20)
            .Select(_ => new Registro { EventoId = eventoId, Estado = EstadoRegistro.Pendiente })
            .ToList();
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(registros);

        var result = await _service.GetCuposDisponiblesAsync(eventoId);

        result.Should().Be(30);
    }

    [Fact]
    public async Task GetCuposDisponibles_NoContaRegistrosCancelados()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        var evento = new Evento { Id = eventoId, CapacidadMaxima = 10 };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>())).ReturnsAsync(evento);

        var registros = new List<Registro>
        {
            new() { EventoId = eventoId, Estado = EstadoRegistro.Pagado },
            new() { EventoId = eventoId, Estado = EstadoRegistro.Cancelado },
            new() { EventoId = eventoId, Estado = EstadoRegistro.Pendiente }
        };
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(registros);

        var result = await _service.GetCuposDisponiblesAsync(eventoId);

        result.Should().Be(8);
    }
}
