using FluentValidation;
using InterviewService.DTOs.Requests.Interviews;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Validators;

[ExcludeFromCodeCoverage]
public class CreateInterviewValidator : AbstractValidator<CreateInterviewRequest>
{
    public CreateInterviewValidator()
    {
        RuleFor(x => x.CandidateId)
            .GreaterThan(0)
            .WithMessage("Valid CandidateId is required.");

        RuleFor(x => x.RequirementId)
            .GreaterThan(0)
            .WithMessage("Valid RequirementId is required.");

        RuleFor(x => x.Project)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Project name is required and cannot exceed 100 characters.");

        RuleFor(x => x.Account)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Account name is required and cannot exceed 100 characters.");

        RuleFor(x => x.Interviewer)
            .NotEmpty()
            .MaximumLength(150)
            .EmailAddress()
            .WithMessage("Valid interviewer email is required.");

        RuleFor(x => x.InterviewDate)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
            .WithMessage("Interview date must be in the future.");

        RuleFor(x => x.Level)
            .InclusiveBetween(1, 2)
            .WithMessage("Interview level must be 1 (Internal) or 2 (Client).");

    }
}