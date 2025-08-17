using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Reposatory
{
    using global::Identity.DAL;
    using global::Identity.Domain.Entities;
    using global::Identity.Domain.IReposatory;



    namespace Identity.Infrastructure.Reposatory
    {
        public class EmailBodyRepository : AsyncReposatory<EmailBody>, IEmailBodyRepository
        {
            public EmailBodyRepository(AppDbContext context) : base(context) { }
        }
    }

}
