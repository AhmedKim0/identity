using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.GoogleDTOs
{
    public class GoogleUserInfo
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Picture { get; set; } = default!;
    }

}
