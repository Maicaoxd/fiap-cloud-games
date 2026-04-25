using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common.Exceptions;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private const string UniqueEmailIndexName = "IX_Users_Email";
        private const string UniqueCpfIndexName = "IX_Users_Cpf";

        private readonly FcgDbContext _dbContext;

        public UserRepository(FcgDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AnyAsync(user => user.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AnyAsync(user => user.Cpf == cpf, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        public async Task<User?> GetByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .SingleOrDefaultAsync(user => user.Cpf == cpf, cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueEmailIndexName))
            {
                throw new EmailAlreadyRegisteredException();
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueCpfIndexName))
            {
                throw new CpfAlreadyRegisteredException();
            }
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Update(user);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueEmailIndexName))
            {
                throw new EmailAlreadyRegisteredException();
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueCpfIndexName))
            {
                throw new CpfAlreadyRegisteredException();
            }
        }
    }
}
