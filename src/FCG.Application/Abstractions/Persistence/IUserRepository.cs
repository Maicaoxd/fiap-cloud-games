using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Abstractions.Persistence
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);
    }
}
