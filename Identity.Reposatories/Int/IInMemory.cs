using Identity.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Int
{
    public interface IInMemory
    {
        Task<Permission> GetPermissionByNameAsync(string permissionName);
        Task LoadAllPermissionsToCacheAsync();
    }
}
