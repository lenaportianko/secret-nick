using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Queries;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Aggregate.Room;
using Epam.ItMarathon.ApiService.Domain.Entities.User;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentAssertions;
using FluentValidation.Results;
using NSubstitute;

namespace Epam.ItMarathon.ApiService.Application.Tests.UserCases.Commands
{
    /// <summary>
    /// Unit tests for the <see cref="DeleteUserHandler"/> class.
    /// </summary>
    public class DeleteUserHandlerTests
    {
        private readonly IRoomRepository _roomRepositoryMock;
        private readonly IUserReadOnlyRepository _userReadOnlyRepositoryMock;
        private readonly DeleteUserHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteUserHandlerTests"/> class with mocked dependencies.
        /// </summary>
        public DeleteUserHandlerTests()
        {
            _roomRepositoryMock = Substitute.For<IRoomRepository>();
            _userReadOnlyRepositoryMock = Substitute.For<IUserReadOnlyRepository>();
            _handler = new DeleteUserHandler(_roomRepositoryMock, _userReadOnlyRepositoryMock);
        }

        /// <summary>
        /// Tests that the handler returns a NotFoundError when the user with provided UserId not found.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserWithProvidedUserIdNotFound()
        {
            // Arrange
            var request = new DeleteUserRequest(string.Empty, 2);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, true)
                .Returns(Result.Failure<Domain.Entities.User.User, ValidationResult>(
                    new NotFoundError([
                        new ValidationFailure("id", string.Empty)
                    ])
                ));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<NotFoundError>();
            result.Error.Errors.Should().Contain(e => e.PropertyName == "id");
        }

        /// <summary>
        /// Tests that the handler returns a NotFoundError when the user by provided UserCode is not found.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAuthUserNotFound()
        {
            // Arrange
            var userToDelete = DataFakers.ValidUserBuilder.Build();
            var request = new DeleteUserRequest("test-user-code", userToDelete.Id);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, true)
                .Returns(userToDelete);

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.UserCode, CancellationToken.None, true)
                .Returns(new NotFoundError([
                    new ValidationFailure("userCode", string.Empty)
                ]));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<NotFoundError>();
            result.Error.Errors.Should().Contain(e => e.PropertyName == "userCode");
        }

        /// <summary>
        /// Tests that the handler returns a ForbiddenError when the user provided UserCode is not admin.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAuthUserIsNotAdmin()
        {
            // Arrange
            var userToDelete = DataFakers.ValidUserBuilder.Build();
            var notAdminUser = DataFakers.ValidUserBuilder.WithIsAdmin(false).Build();
            var request = new DeleteUserRequest(notAdminUser.AuthCode, userToDelete.Id);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, true)
                .Returns(userToDelete);

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.UserCode, CancellationToken.None, true)
                .Returns(notAdminUser);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ForbiddenError>();
            result.Error.Errors.Should().Contain(e => e.PropertyName == "userCode");
        }

        /// <summary>
        /// Tests that the handler returns a NotAuthorizedError when users belong to different rooms.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUsersBelongToDifferentRooms()
        {
            // Arrange
            var userToDelete = DataFakers.ValidUserBuilder.WithRoomId(1).Build();
            var adminUser = DataFakers.ValidUserBuilder.WithRoomId(2).WithIsAdmin(true).Build();
            var request = new DeleteUserRequest(adminUser.AuthCode, userToDelete.Id);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, true)
                .Returns(userToDelete);

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.UserCode, CancellationToken.None, true)
                .Returns(adminUser);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<NotAuthorizedError>();
            result.Error.Errors.Should().Contain(e => e.PropertyName == "id");
        }

        /// <summary>
        /// Tests that the handler returns a BadRequestError when admin tries to delete yourself.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAdminTriesToDeleteYourSelf()
        {
            // Arrange
            var adminUser = DataFakers.ValidUserBuilder.WithIsAdmin(true).Build();
            var request = new DeleteUserRequest(adminUser.AuthCode, adminUser.Id);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, true)
                .Returns(adminUser);

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.UserCode, CancellationToken.None, true)
                .Returns(adminUser);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<BadRequestError>();
            result.Error.Errors.Should().Contain(e => e.PropertyName == "id");
        }

        /// <summary>
        /// Tests that the handler returns a BadRequestError when the room is already closed.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRoomIsAlreadyClosed()
        {
            // Arrange
            var adminUser = DataFakers.ValidUserBuilder.WithIsAdmin(true).Build();
            var userToDelete = DataFakers.ValidUserBuilder.WithRoomId(adminUser.RoomId).WithId(adminUser.Id+1).Build();

            var existingRoom = DataFakers.RoomFaker
                .RuleFor(room => room.ClosedOn, faker => faker.Date.Past())
                .RuleFor(room => room.Users, _ => new List<User> { adminUser, userToDelete })
                .Generate();

            var request = new DeleteUserRequest(adminUser.AuthCode, userToDelete.Id);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, true)
                .Returns(userToDelete);

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.UserCode, CancellationToken.None, true)
                .Returns(adminUser);

            _roomRepositoryMock
                .GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(existingRoom);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<BadRequestError>();
            result.Error.Errors.Should().Contain(e => e.PropertyName == "room.ClosedOn");
        }

        /// <summary>
        /// Tests that the handler deletes user.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenAdminDeletesUser()
        {
            // Arrange
            var roomId = 1;

            var adminUser = DataFakers.ValidUserBuilder.WithIsAdmin(true).WithRoomId((ulong)roomId).Build();
            var userToDelete = DataFakers.ValidUserBuilder.WithId(adminUser.Id + 1).WithRoomId((ulong)roomId).Build();

            var existingRoom = DataFakers.RoomFaker
                .RuleFor(r => r.Id, _ => (ulong)roomId)
                .RuleFor(r => r.Users, _ => new List<User> { adminUser, userToDelete })
                .Generate();

            var request = new DeleteUserRequest(adminUser.AuthCode, userToDelete.Id);

            _userReadOnlyRepositoryMock
                .GetByIdAsync(request.UserId, CancellationToken.None, true)
                .Returns(userToDelete);

            _userReadOnlyRepositoryMock
                .GetByCodeAsync(request.UserCode, CancellationToken.None, true)
                .Returns(adminUser);

            _roomRepositoryMock
                .GetByUserCodeAsync(request.UserCode, CancellationToken.None)
                .Returns(existingRoom);


            //room
            //    .DeleteUser(participant.Id)
            //    .Returns(Result.Success<Room, ValidationResult>(room));

            _roomRepositoryMock
                .UpdateAsync(existingRoom, CancellationToken.None)
                .Returns(Result.Success(string.Empty));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}