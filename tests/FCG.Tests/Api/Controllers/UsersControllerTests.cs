using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FCG.Api.Controllers;
using FCG.Api.Common;
using FCG.Api.Users;
using FCG.Application.Abstractions.Persistence;
using FCG.Application.Abstractions.Security;
using FCG.Application.Users.Deactivate;
using FCG.Application.Users.Register;
using FCG.Application.Users.UpdateCurrent;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FCG.Tests.Api.Controllers;

[Trait("Category", "Unit")]
public sealed class UsersControllerTests
{
    [Fact]
    public async Task RegisterAsync_QuandoDadosForemValidos_DeveRetornarCreatedComUserId()
    {
        // Arrange
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var deactivateUseCase = new DeactivateUserUseCase(userRepository);
        var updateCurrentUserUseCase = new UpdateCurrentUserUseCase(userRepository);

        userRepository
            .ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        passwordHasher.Hash(Arg.Any<Password>()).Returns(passwordHash);

        var useCase = new RegisterUserUseCase(userRepository, passwordHasher);
        var controller = new UsersController(deactivateUseCase, useCase, updateCurrentUserUseCase);
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

    [Fact]
    public async Task UpdateMeAsync_QuandoUsuarioAutenticadoERequestValido_DeveRetornarNoContent()
    {
        // Arrange
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("maicon.guedes@email.com"), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var deactivateUseCase = new DeactivateUserUseCase(userRepository);
        var registerUseCase = new RegisterUserUseCase(userRepository, passwordHasher);
        var updateCurrentUserUseCase = new UpdateCurrentUserUseCase(userRepository);
        var controller = new UsersController(deactivateUseCase, registerUseCase, updateCurrentUserUseCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(user.Id, nameof(UserRole.User))
            }
        };
        var request = new UpdateCurrentUserRequest(
            "Maicon Guedes",
            "maicon.guedes@email.com");

        // Act
        var actionResult = await controller.UpdateMeAsync(request, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon.guedes@email.com"));
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateAsync_QuandoAdminAutenticadoEUsuarioExistir_DeveRetornarNoContent()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var deactivateUseCase = new DeactivateUserUseCase(userRepository);
        var registerUseCase = new RegisterUserUseCase(userRepository, passwordHasher);
        var updateCurrentUserUseCase = new UpdateCurrentUserUseCase(userRepository);
        var controller = new UsersController(deactivateUseCase, registerUseCase, updateCurrentUserUseCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(adminId)
            }
        };

        // Act
        var actionResult = await controller.DeactivateAsync(user.Id, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        user.IsActive.ShouldBeFalse();
        user.UpdatedBy.ShouldBe(adminId);
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void DeactivateAsync_DeveExigirRoleAdministrator()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.DeactivateAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBe(nameof(UserRole.Administrator));
    }

    [Fact]
    public void UpdateMeAsync_DeveExigirUsuarioAutenticado()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.UpdateMeAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBeNull();
    }

    [Fact]
    public void DeactivateAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.DeactivateAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status404NotFound].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void UpdateMeAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.UpdateMeAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status409Conflict].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    private static DefaultHttpContext CreateHttpContext(Guid userId, string role = nameof(UserRole.Administrator))
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtClaimNames.Role, role)
        };

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
    }

    private static User CreateUser()
    {
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        return User.Create("Maicon Guedes", email, passwordHash);
    }
}
