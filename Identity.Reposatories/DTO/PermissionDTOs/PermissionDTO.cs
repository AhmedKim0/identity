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
        public string? NameLogical { get; set; } = default!;
        public string? NameAr { get; set; } = default!;
        public string? NameEn { get; set; } = default!;
    }
}
