using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Abstractions.Persistence
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

        Task<bool> ExistsByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

        Task<User?> GetByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);

        Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
