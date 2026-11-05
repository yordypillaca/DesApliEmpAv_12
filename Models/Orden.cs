using System.ComponentModel.DataAnnotations;

namespace TallerApi.Models;

public partial class Orden
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El número de orden es obligatorio.")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "El número debe tener entre 4 y 20 caracteres.")]
    public string Numero { get; set; } = null!;

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(10, MinimumLength = 6, ErrorMessage = "La placa debe tener entre 6 y 10 caracteres.")]
    public string Placa { get; set; } = null!;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "La descripción debe tener entre 5 y 250 caracteres.")]
    public string Descripcion { get; set; } = null!;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [StringLength(20)]
    [RegularExpression("^(Recibida|EnReparacion|Entregada)$", ErrorMessage = "El estado debe ser Recibida, EnReparacion o Entregada.")]
    public string Estado { get; set; } = null!;

    [Range(typeof(decimal), "0", "100000", ErrorMessage = "El costo estimado debe estar entre 0 y 100000.")]
    public decimal CostoEstimado { get; set; }

    public DateOnly FechaIngreso { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El cliente es obligatorio.")]
    public int ClienteId { get; set; }

    public virtual Cliente? Cliente { get; set; }
}
