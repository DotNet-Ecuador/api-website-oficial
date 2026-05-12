using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
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

    // ─── GetBySlugAsync ───────────────────────────────────────────────────────

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

    // ─── GetBySlugAdminAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetBySlugAdmin_CuandoEventoActivo_RetornaEvento()
    {
        var evento = new Evento { Id = "id1", Slug = "conf-2025", Activo = true };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(evento);

        var result = await _service.GetBySlugAdminAsync("conf-2025");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("conf-2025");
    }

    [Fact]
    public async Task GetBySlugAdmin_CuandoEventoInactivo_TambiénRetornaEvento()
    {
        var evento = new Evento { Id = "id2", Slug = "conf-vieja", Activo = false };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(evento);

        var result = await _service.GetBySlugAdminAsync("conf-vieja");

        result.Should().NotBeNull();
        result!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task GetBySlugAdmin_CuandoNoExiste_RetornaNull()
    {
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync((Evento?)null);

        var result = await _service.GetBySlugAdminAsync("no-existe");

        result.Should().BeNull();
    }

    // ─── GetCuposDisponiblesAsync ─────────────────────────────────────────────

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

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_CuandoSlugNoExiste_LanzaKeyNotFoundException()
    {
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync((Evento?)null);

        var act = () => _service.UpdateAsync("no-existe", new ActualizarEventoRequestDto { Nombre = "X" });

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*no-existe*");
    }

    [Fact]
    public async Task Update_MapeaTodosLosCamposMutables()
    {
        var original = new Evento
        {
            Id = "507f1f77bcf86cd799439011",
            Slug = "meetup-jun",
            Nombre = "Viejo",
            Precio = 5m,
            CreadoEn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(original);

        Evento? capturado = null;
        _mockEventoRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Evento>()))
            .Callback<string, Evento>((_, e) => capturado = e);

        var dto = new ActualizarEventoRequestDto
        {
            Nombre = "Nuevo Nombre",
            Descripcion = "Descripción actualizada",
            FechaEvento = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc),
            Lugar = "Quito",
            Precio = 20m,
            CapacidadMaxima = 100,
            Activo = false,
            Tipo = "meetup",
            Subtipo = "tech",
            Formato = "presencial",
            Networking = true,
            Tags = ["dotnet", "csharp"],
            CoverImage = "https://img.example.com/cover.jpg",
            HostedBy = "DotNet Ecuador",
            RegistroUrl = "https://registro.example.com"
        };

        await _service.UpdateAsync("meetup-jun", dto);

        capturado.Should().NotBeNull();
        capturado!.Nombre.Should().Be("Nuevo Nombre");
        capturado.Descripcion.Should().Be("Descripción actualizada");
        capturado.FechaEvento.Should().Be(dto.FechaEvento);
        capturado.Lugar.Should().Be("Quito");
        capturado.Precio.Should().Be(20m);
        capturado.CapacidadMaxima.Should().Be(100);
        capturado.Activo.Should().BeFalse();
        capturado.Tipo.Should().Be("meetup");
        capturado.Subtipo.Should().Be("tech");
        capturado.Formato.Should().Be("presencial");
        capturado.Networking.Should().BeTrue();
        capturado.Tags.Should().BeEquivalentTo(["dotnet", "csharp"]);
        capturado.CoverImage.Should().Be("https://img.example.com/cover.jpg");
        capturado.HostedBy.Should().Be("DotNet Ecuador");
        capturado.RegistroUrl.Should().Be("https://registro.example.com");
    }

    [Fact]
    public async Task Update_PreservaIdSlugYCreadoEn()
    {
        var fechaCreacion = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var original = new Evento
        {
            Id = "507f1f77bcf86cd799439011",
            Slug = "evento-original",
            CreadoEn = fechaCreacion
        };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(original);

        Evento? capturado = null;
        _mockEventoRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Evento>()))
            .Callback<string, Evento>((_, e) => capturado = e);

        await _service.UpdateAsync("evento-original", new ActualizarEventoRequestDto { Nombre = "Nuevo" });

        capturado!.Id.Should().Be("507f1f77bcf86cd799439011");
        capturado.Slug.Should().Be("evento-original");
        capturado.CreadoEn.Should().Be(fechaCreacion);
    }

    [Fact]
    public async Task Update_CuandoFechaEventoEsNull_ConservaCampoOriginal()
    {
        var fechaOriginal = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var original = new Evento { Id = "id1", Slug = "ev", FechaEvento = fechaOriginal };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(original);

        Evento? capturado = null;
        _mockEventoRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Evento>()))
            .Callback<string, Evento>((_, e) => capturado = e);

        await _service.UpdateAsync("ev", new ActualizarEventoRequestDto { Nombre = "X", FechaEvento = null });

        capturado!.FechaEvento.Should().Be(fechaOriginal);
    }

    [Fact]
    public async Task Update_CuandoPrecioEsNull_ConservaPrecioOriginal()
    {
        var original = new Evento { Id = "id1", Slug = "ev", Precio = 15m };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(original);

        Evento? capturado = null;
        _mockEventoRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Evento>()))
            .Callback<string, Evento>((_, e) => capturado = e);

        await _service.UpdateAsync("ev", new ActualizarEventoRequestDto { Nombre = "X", Precio = null });

        capturado!.Precio.Should().Be(15m);
    }

    [Fact]
    public async Task Update_ActualizaActualizadoEnConFechaActual()
    {
        var antes = DateTime.UtcNow.AddMinutes(-1);
        var original = new Evento { Id = "id1", Slug = "ev" };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(original);

        Evento? capturado = null;
        _mockEventoRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Evento>()))
            .Callback<string, Evento>((_, e) => capturado = e);

        await _service.UpdateAsync("ev", new ActualizarEventoRequestDto { Nombre = "X" });

        capturado!.ActualizadoEn.Should().BeAfter(antes);
        capturado.ActualizadoEn.Should().BeBefore(DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task Update_LlamaUpdateRepoConIdCorrecto()
    {
        var original = new Evento { Id = "507f1f77bcf86cd799439011", Slug = "ev" };
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(original);

        await _service.UpdateAsync("ev", new ActualizarEventoRequestDto { Nombre = "X" });

        _mockEventoRepo.Verify(r => r.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<Evento>()), Times.Once);
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_CuandoSlugNoExiste_LanzaKeyNotFoundException()
    {
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync((Evento?)null);
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var act = () => _service.DeleteAsync("no-existe");

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*no-existe*");
    }

    [Fact]
    public async Task Delete_CuandoHayRegistrosPendientes_LanzaInvalidOperationException()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(new Evento { Id = eventoId, Slug = "ev" });
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new Registro { EventoId = eventoId, Estado = EstadoRegistro.Pendiente, EliminadoEn = null }
        ]);

        var act = () => _service.DeleteAsync("ev");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*1 registro(s) activo(s)*");
    }

    [Fact]
    public async Task Delete_CuandoHayRegistrosPagados_LanzaInvalidOperationException()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(new Evento { Id = eventoId, Slug = "ev" });
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new Registro { EventoId = eventoId, Estado = EstadoRegistro.Pagado, EliminadoEn = null },
            new Registro { EventoId = eventoId, Estado = EstadoRegistro.Pagado, EliminadoEn = null }
        ]);

        var act = () => _service.DeleteAsync("ev");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*2 registro(s) activo(s)*");
    }

    [Theory]
    [InlineData(EstadoRegistro.Cancelado)]
    [InlineData(EstadoRegistro.Rechazado)]
    [InlineData(EstadoRegistro.Eliminado)]
    public async Task Delete_RegistrosNoActivos_NoBloqueanEliminacion(string estado)
    {
        var eventoId = "507f1f77bcf86cd799439011";
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(new Evento { Id = eventoId, Slug = "ev" });
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new Registro { EventoId = eventoId, Estado = estado, EliminadoEn = null }
        ]);

        var act = () => _service.DeleteAsync("ev");

        await act.Should().NotThrowAsync();
        _mockEventoRepo.Verify(r => r.DeleteAsync(eventoId), Times.Once);
    }

    [Fact]
    public async Task Delete_RegistrosSoftDeleted_NoBloqueanEliminacion()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(new Evento { Id = eventoId, Slug = "ev" });
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new Registro { EventoId = eventoId, Estado = EstadoRegistro.Pendiente, EliminadoEn = DateTime.UtcNow }
        ]);

        var act = () => _service.DeleteAsync("ev");

        await act.Should().NotThrowAsync();
        _mockEventoRepo.Verify(r => r.DeleteAsync(eventoId), Times.Once);
    }

    [Fact]
    public async Task Delete_RegistrosDeOtroEvento_NoBloqueanEliminacion()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(new Evento { Id = eventoId, Slug = "ev" });
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new Registro { EventoId = "otro-evento-id", Estado = EstadoRegistro.Pendiente, EliminadoEn = null }
        ]);

        var act = () => _service.DeleteAsync("ev");

        await act.Should().NotThrowAsync();
        _mockEventoRepo.Verify(r => r.DeleteAsync(eventoId), Times.Once);
    }

    [Fact]
    public async Task Delete_CuandoSinRegistrosActivos_EliminaEvento()
    {
        var eventoId = "507f1f77bcf86cd799439011";
        _mockEventoRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Evento, bool>>>()))
            .ReturnsAsync(new Evento { Id = eventoId, Slug = "ev" });
        _mockRegistroRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        await _service.DeleteAsync("ev");

        _mockEventoRepo.Verify(r => r.DeleteAsync(eventoId), Times.Once);
    }
}
