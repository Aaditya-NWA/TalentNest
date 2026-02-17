using FluentValidation;
using InterviewService.DTOs.Requests.Feedbacks;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Validators;

[ExcludeFromCodeCoverage]
public class UpdateFeedbackValidator : AbstractValidator<UpdateFeedbackRequest>
{
    public UpdateFeedbackValidator()
    {
        When(x => x.Comments != null, () =>
        {
            RuleFor(x => x.Comments)
                .NotEmpty()
                .MaximumLength(2000);
        });

        When(x => x.RecommendedOutcome != null, () =>
        {
            RuleFor(x => x.RecommendedOutcome)
                .IsInEnum();
        });
    }
}