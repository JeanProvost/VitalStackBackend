using Backend.Core.Entities.Base;
using Backend.Core.Entities.Supplements;
using Backend.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Core.Entities.UserStackEntries
{
    public class UserStackEntry : BaseEntity<Guid>
    {
        public required string UserId { get; set; }
        public int? SupplementProductId { get; set; }
        public SupplementProduct? SupplementProduct { get; set; }
        public string? CustomName { get; set; }

        [Column(TypeName = "jsonb")]
        public StackCustomization Cusomization { get; set; } = new();
        public ScheduleTimeBlock IntendedTime { get; set; } = ScheduleTimeBlock.Morning;
        public string? ContextualInstruction { get; set; }
        public decimal ServingMultiplier { get; set; } = 1.0m;
        public bool IsActive { get; set; } = true;
    }

    public class StackCustomization
    {
        public string? Form { get; set; }
        public string? Dosage { get; set; }
        public string? Brand { get; set; }
        public string? TimeOfDayTarget { get; set; }
    }
}
