using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.PermissionDTOs
{
    public class PermissionDTO
    {
        public int? Id { get; set; }
        public string? Name { get; set; } = default!;
    }
}
