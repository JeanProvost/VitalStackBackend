using Backend.Core.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Backend.Core.Entities.Supplement;

public class Supplement : BaseEntity<Guid>
{
    public required string  Name { get; set; }
    public string? Brand { get; set; }
    public required string Form { get; set; }
    public required decimal DosageAmount { get; set; }
    public required string DosageUnit { get; set; }
    public required string TimeOfDay { get; set; }
    public bool? RequiresFood { get; set; }
    public List<string> Aliases { get; set; } = new();
    public JsonDocument? ScientificContext { get; set; }
}
