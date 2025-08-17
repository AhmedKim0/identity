namespace Identity.Infrastructure.Reposatory
{
    using global::Identity.DAL;
    using global::Identity.Domain.Entities;
    using global::Identity.Domain.IReposatory;



    namespace Identity.Infrastructure.Reposatory
    {
        public class UserTokenRepository : AsyncReposatory<UserToken>, IUserTokenRepository
        {
            public UserTokenRepository(AppDbContext context) : base(context) { }
        }
    }

}
