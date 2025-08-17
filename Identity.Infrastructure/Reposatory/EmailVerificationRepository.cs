namespace Identity.Infrastructure.Reposatory
{
    using global::Identity.DAL;
    using global::Identity.Domain.Entities;
    using global::Identity.Domain.IReposatory;



    namespace Identity.Infrastructure.Reposatory
    {
        public class EmailVerificationRepository : AsyncReposatory<EmailVerification>, IEmailVerificationRepository
        {
            public EmailVerificationRepository(AppDbContext context) : base(context) { }
        }
    }

}
