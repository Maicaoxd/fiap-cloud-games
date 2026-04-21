using FCG.Domain.Users;

namespace FCG.Application.Abstractions.Security
{
    public interface IAccessTokenGenerator
    {
        string Generate(User user);
    }
}
