using FCG.Api.Controllers;
using FCG.Api.Users;
using FCG.Application.Abstractions.Persistence;
using FCG.Application.Abstractions.Security;
using FCG.Application.Users.Register;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FCG.Tests.Api.Controllers;

public sealed class UsersControllerTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public async Task RegisterAsync_QuandoDadosForemValidos_DeveRetornarCreatedComUserId()
    {
        // Arrange
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        passwordHasher.Hash(Arg.Any<Password>()).Returns(passwordHash);

        var useCase = new RegisterUserUseCase(userRepository, passwordHasher);
        var controller = new UsersController(useCase);
        var request = new RegisterUserRequest(
            "Maicon Guedes",
            "maicon@email.com",
            "Senha@123",
            "Senha@123");

        // Act
        var actionResult = await controller.RegisterAsync(request, CancellationToken.None);

        // Assert
        var createdResult = actionResult.Result.ShouldBeOfType<CreatedResult>();
        var response = createdResult.Value.ShouldBeOfType<RegisterUserResponse>();

        response.UserId.ShouldNotBe(Guid.Empty);
        createdResult.Location.ShouldBe($"/api/users/{response.UserId}");
        await userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
