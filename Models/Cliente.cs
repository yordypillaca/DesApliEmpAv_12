using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TallerApi.Models;

public partial class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El documento es obligatorio.")]
    [StringLength(15, MinimumLength = 8, ErrorMessage = "El documento debe tener entre 8 y 15 caracteres.")]
    public string Documento { get; set; } = null!;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(20, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 20 caracteres.")]
    public string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(120, ErrorMessage = "El correo no puede superar 120 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
}
