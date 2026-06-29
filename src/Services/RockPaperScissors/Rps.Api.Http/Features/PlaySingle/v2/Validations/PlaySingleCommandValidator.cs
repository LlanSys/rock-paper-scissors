using FluentValidation;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Features.PlaySingle.v2.Handlers.CommandHandlers;
using Rps.Api.Http.Shared.Extensions;

namespace Rps.Api.Http.Features.PlaySingle.v2.Validations;

public class PlaySingleCommandValidator : AbstractValidator<PlaySingleCommand>
{
    public PlaySingleCommandValidator()
    {
        RuleFor(x => x.PlayRequest)
            .NotNull().WithMessage("Request body is required.");

        RuleFor(x => x.PlayRequest.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(action => {
                try
                {
                    action.ToEnum<Move>();
                    return true;
                }
                catch
                {
                    return false;
                }
            })
            .WithMessage("You have to play fire, water, wood, metal or earth.");
    }
}
