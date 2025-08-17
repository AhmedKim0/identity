namespace Identity.Infrastructure.Reposatory
{
    using global::Identity.DAL;
    using global::Identity.Domain.Entities;
    using global::Identity.Domain.IReposatory;



    namespace Identity.Infrastructure.Reposatory
    {
        public class OTPTryRepository : AsyncReposatory<OTPTry>, IOTPTryRepository
        {
            public OTPTryRepository(AppDbContext context) : base(context) { }
        }
    }

}
