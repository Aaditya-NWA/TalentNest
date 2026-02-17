using FluentValidation;
using InterviewService.DTOs.Requests.Feedbacks;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Validators;

[ExcludeFromCodeCoverage]
public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackRequest>
{
    public CreateFeedbackValidator()
    {
        RuleFor(x => x.InterviewId)
            .GreaterThan(0)
            .WithMessage("Valid InterviewId is required.");

        RuleFor(x => x.Comments)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Feedback comments are required and cannot exceed 2000 characters.");

        RuleFor(x => x.RecommendedOutcome)
            .IsInEnum()
            .WithMessage("Valid recommended outcome is required.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .MaximumLength(150)
            .EmailAddress()
            .WithMessage("Valid email for CreatedBy is required.");
    }
}