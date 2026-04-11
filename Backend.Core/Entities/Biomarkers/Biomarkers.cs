using Backend.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Core.Entities.Biomarkers
{
    public class Biomarkers : BaseEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string standardUnit { get; set; } = string.Empty;
    }
}
