using Backend.Core.Entities.Base;
using Backend.Core.Entities.Supplements;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Core.Entities.UserStackEntries
{
    public class UserStackEntry : BaseEntity<Guid>
    {
        public required string UserId { get; set; }
        public Guid? MasterSupplementId { get; set; }
        public Supplement? MasterSupplement { get; set; }
        public string? CustomName { get; set; }

        [Column(TypeName = "jsonb")]
        public StackCustomization Cusomization { get; set; } = new();
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
