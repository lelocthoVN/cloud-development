using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Vehicle.Api.Entities;
using Vehicle.Api.Services;

namespace Vehicle.Api.Controllers;

/// <summary>
/// Контроллер для управления транспортными средствами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VehiclesController(VehicleService vehicleService) : ControllerBase
{
    private readonly VehicleService _vehicleService = vehicleService;

    /// <summary>
    /// Создаёт указанное количество случайных транспортных средств
    /// </summary>
    /// <param name="count">Количество генерируемых записей (должно быть больше 0)</param>
    /// <param name="cancellationToken">Токен для отмены запроса</param>
    /// <returns>Список сгенерированных транспортных средств</returns>
    [HttpGet("generate")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate([FromQuery] int? count, CancellationToken cancellationToken = default)
    {
        if (count is null || count <= 0)
        {
            return BadRequest("count must be >= 0");
        }

        var vehicles = await _vehicleService.GenerateAsync(count.Value, cancellationToken);
        return Ok(vehicles);
    }

    /// <summary>
    /// Возвращает транспортное средство по его идентификатору
    /// </summary>
    /// <param name="id">Идентификатор транспортного средства (должен быть больше 0)</param>
    /// <param name="cancellationToken">Токен для отмены запроса</param>
    /// <returns>Информация о найденном транспортном средстве</returns>
    [HttpGet]
    [ProducesResponseType(typeof(VehicleEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById([FromQuery] int? id, CancellationToken cancellationToken = default)
    {
        if (id is null || id <= 0)
        {
            return BadRequest("id must be > 0");
        }

        var vehicle = await _vehicleService.GetByIdAsync(id.Value, cancellationToken);
        return Ok(vehicle);
    }

}