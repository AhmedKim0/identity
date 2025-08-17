namespace Identity.Infrastructure.Reposatory
{
    using global::Identity.DAL;
    using global::Identity.Domain.Entities;
    using global::Identity.Domain.IReposatory;



    namespace Identity.Infrastructure.Reposatory
    {
        public class OTPCodeRepository : AsyncReposatory<OTPCode>, IOTPCodeRepository
        {
            public OTPCodeRepository(AppDbContext context) : base(context) { }
        }
    }

}
