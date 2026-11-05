using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerApi.Models;

namespace TallerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly TallerContext _context;

    public ClientesController(TallerContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> Get()
    {
        var clientes = await _context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetById(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        return Ok(cliente);
    }

    [HttpGet("{id:int}/ordenes")]
    public async Task<ActionResult<IEnumerable<Orden>>> GetOrdenes(int id)
    {
        if (!await _context.Clientes.AnyAsync(c => c.Id == id))
            return NotFound();

        var ordenes = await _context.Ordenes
            .AsNoTracking()
            .Where(o => o.ClienteId == id)
            .OrderByDescending(o => o.FechaIngreso)
            .ThenBy(o => o.Numero)
            .ToListAsync();

        return Ok(ordenes);
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Post(Cliente cliente)
    {
        if (await _context.Clientes.AnyAsync(c => c.Documento == cliente.Documento))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Documento duplicado",
                Detail = $"Ya existe un cliente con el documento '{cliente.Documento}'.",
                Status = StatusCodes.Status409Conflict
            });
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Cliente cliente)
    {
        if (id != cliente.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Identificador inconsistente",
                Detail = "El id de la ruta no coincide con el id del cuerpo.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var actual = await _context.Clientes.FindAsync(id);
        if (actual == null)
            return NotFound();

        if (await _context.Clientes.AnyAsync(c => c.Documento == cliente.Documento && c.Id != id))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Documento duplicado",
                Detail = $"Ya existe un cliente con el documento '{cliente.Documento}'.",
                Status = StatusCodes.Status409Conflict
            });
        }

        actual.Documento = cliente.Documento;
        actual.Nombre = cliente.Nombre;
        actual.Telefono = cliente.Telefono;
        actual.Correo = cliente.Correo;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        if (await _context.Ordenes.AnyAsync(o => o.ClienteId == id))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Cliente con órdenes",
                Detail = "No se puede eliminar un cliente que tiene órdenes de servicio.",
                Status = StatusCodes.Status409Conflict
            });
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
