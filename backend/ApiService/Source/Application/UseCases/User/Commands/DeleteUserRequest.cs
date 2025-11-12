using CSharpFunctionalExtensions;
using FluentValidation.Results;
using MediatR;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Commands
{
    /// <summary>
    /// Request deleting a User from Room.
    /// </summary>
    /// <param name="UserCode">User authorization code.</param>
    /// <param name="UserId">User's unique identifier.</param>
    public record DeleteUserRequest(string UserCode, ulong UserId)
        : IRequest<UnitResult<ValidationResult>>;
}