using FluentValidation;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Features.PlayMultiple.v2.Hanlders.CommandHandlers;
using Rps.Api.Http.Shared.Extensions;

namespace Rps.Api.Http.Features.PlayMultiple.v2.Validations;

public class PlayMultipleCommandValidator : AbstractValidator<PlayMultipleCommand>
{
    public PlayMultipleCommandValidator()
    {
        RuleFor(x => x.PlayMultipleRequest)
            .NotNull().WithMessage("Request body is required.");

        RuleFor(x => x.PlayMultipleRequest.Actions)
            .NotEmpty().WithMessage("Actions list is required and cannot be empty.");

        RuleForEach(x => x.PlayMultipleRequest.Actions)
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
