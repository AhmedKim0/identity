using Identity.Domain.SharedEntities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    public class EmailBody : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string Subject { get; set; } = null!;

        public string Body { get; set; } = null!;
    }
}
