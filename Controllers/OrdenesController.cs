using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerApi.Models;

namespace TallerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdenesController : ControllerBase
{
    private readonly TallerContext _context;

    public OrdenesController(TallerContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Orden>>> Get(int? clienteId, string? estado, string? buscar)
    {
        if (!string.IsNullOrWhiteSpace(estado) && estado is not ("Recibida" or "EnReparacion" or "Entregada"))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Estado no válido",
                Detail = "El estado debe ser Recibida, EnReparacion o Entregada.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var consulta = _context.Ordenes
            .AsNoTracking()
            .Include(o => o.Cliente)
            .AsQueryable();

        if (clienteId != null)
            consulta = consulta.Where(o => o.ClienteId == clienteId);

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(o => o.Estado == estado);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(o =>
                o.Numero.Contains(texto) ||
                o.Placa.Contains(texto) ||
                o.Descripcion.Contains(texto));
        }

        var ordenes = await consulta
            .OrderByDescending(o => o.FechaIngreso)
            .ThenBy(o => o.Numero)
            .ToListAsync();

        return Ok(ordenes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Orden>> GetById(int id)
    {
        var orden = await _context.Ordenes
            .AsNoTracking()
            .Include(o => o.Cliente)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return NotFound();

        return Ok(orden);
    }

    [HttpPost]
    public async Task<ActionResult<Orden>> Post(Orden orden)
    {
        if (orden.FechaIngreso == default)
        {
            ModelState.AddModelError(nameof(orden.FechaIngreso), "La fecha de ingreso es obligatoria.");
            return ValidationProblem(ModelState);
        }

        if (!await _context.Clientes.AnyAsync(c => c.Id == orden.ClienteId))
        {
            ModelState.AddModelError(nameof(orden.ClienteId), "El cliente indicado no existe.");
            return ValidationProblem(ModelState);
        }

        if (await _context.Ordenes.AnyAsync(o => o.Numero == orden.Numero))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Número duplicado",
                Detail = $"Ya existe una orden con el número '{orden.Numero}'.",
                Status = StatusCodes.Status409Conflict
            });
        }

        orden.Cliente = null;
        _context.Ordenes.Add(orden);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Orden orden)
    {
        if (id != orden.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Identificador inconsistente",
                Detail = "El id de la ruta no coincide con el id del cuerpo.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (orden.FechaIngreso == default)
        {
            ModelState.AddModelError(nameof(orden.FechaIngreso), "La fecha de ingreso es obligatoria.");
            return ValidationProblem(ModelState);
        }

        var actual = await _context.Ordenes.FindAsync(id);
        if (actual == null)
            return NotFound();

        if (actual.Estado == "Entregada")
        {
            return Conflict(new ProblemDetails
            {
                Title = "Orden entregada",
                Detail = "No se puede modificar una orden que ya fue entregada.",
                Status = StatusCodes.Status409Conflict
            });
        }

        if (!await _context.Clientes.AnyAsync(c => c.Id == orden.ClienteId))
        {
            ModelState.AddModelError(nameof(orden.ClienteId), "El cliente indicado no existe.");
            return ValidationProblem(ModelState);
        }

        if (await _context.Ordenes.AnyAsync(o => o.Numero == orden.Numero && o.Id != id))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Número duplicado",
                Detail = $"Ya existe una orden con el número '{orden.Numero}'.",
                Status = StatusCodes.Status409Conflict
            });
        }

        actual.Numero = orden.Numero;
        actual.Placa = orden.Placa;
        actual.Descripcion = orden.Descripcion;
        actual.Estado = orden.Estado;
        actual.CostoEstimado = orden.CostoEstimado;
        actual.FechaIngreso = orden.FechaIngreso;
        actual.ClienteId = orden.ClienteId;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var orden = await _context.Ordenes.FindAsync(id);
        if (orden == null)
            return NotFound();

        if (orden.Estado == "Entregada")
        {
            return Conflict(new ProblemDetails
            {
                Title = "Orden entregada",
                Detail = "No se puede eliminar una orden que ya fue entregada.",
                Status = StatusCodes.Status409Conflict
            });
        }

        _context.Ordenes.Remove(orden);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
