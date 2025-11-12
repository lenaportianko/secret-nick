using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentValidation.Results;
using MediatR;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers
{
    /// <summary>
    /// Handler for Users delete.
    /// </summary>
    /// <param name="roomRepository">Implementation of <see cref="IRoomRepository"/> for operating with database.</param>
    /// <param name="userRepository">Implementation of <see cref="IUserReadOnlyRepository"/> for operating with database.</param>
    public class DeleteUserHandler(IRoomRepository roomRepository, IUserReadOnlyRepository userRepository)
        : IRequestHandler<DeleteUserRequest, UnitResult<ValidationResult>>
    {
        ///<inheritdoc/>
        public async Task<UnitResult<ValidationResult>> Handle(DeleteUserRequest request,
            CancellationToken cancellationToken)
        {
            var userByIdResult = await userRepository.GetByIdAsync(request.UserId, cancellationToken, true);
            if (userByIdResult.IsFailure)
            {
                return UnitResult.Failure<ValidationResult>(userByIdResult.Error);
            }

            var userByCodeResult = await userRepository.GetByCodeAsync(request.UserCode, cancellationToken, true);
            if (userByCodeResult.IsFailure)
            {
                return UnitResult.Failure<ValidationResult>(userByCodeResult.Error);
            }

            if (!userByCodeResult.Value.IsAdmin)
            {
                return UnitResult.Failure<ValidationResult>(new ForbiddenError([
                    new ValidationFailure("userCode", "Only admin can delete participants.")
                ]));
            }

            if (userByCodeResult.Value.RoomId != userByIdResult.Value.RoomId)
            {
                return UnitResult.Failure<ValidationResult>(new NotAuthorizedError([
                    new ValidationFailure("id", "User with userCode and user with Id belongs to different rooms.")
                ]));
            }

            if (userByCodeResult.Value.Id == userByIdResult.Value.Id)
            {
                return UnitResult.Failure<ValidationResult>(new BadRequestError([
                    new ValidationFailure("id", "Admin cannot remove yourself.")
                ]));
            }

            var roomResult = await roomRepository.GetByUserCodeAsync(request.UserCode, cancellationToken);
            if (roomResult.IsFailure)
            {
                return UnitResult.Failure<ValidationResult>(roomResult.Error);
            }

            var room = roomResult.Value;
            var deleteResult = room.DeleteUser(request.UserId);
            if (deleteResult.IsFailure)
            {
                return UnitResult.Failure<ValidationResult>(deleteResult.Error);
            }

            var updateResult = await roomRepository.UpdateAsync(room, cancellationToken);
            if (updateResult.IsFailure)
            {
                return UnitResult.Failure<ValidationResult>(new BadRequestError([
                    new ValidationFailure(string.Empty, updateResult.Error)
                ]));
            }

            return UnitResult.Success<ValidationResult>();
        }
    }
}
