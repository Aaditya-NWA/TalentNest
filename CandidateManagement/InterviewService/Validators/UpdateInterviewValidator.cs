using FluentValidation;
using InterviewService.DTOs.Requests.Interviews;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Validators;

[ExcludeFromCodeCoverage]
public class UpdateInterviewValidator : AbstractValidator<UpdateInterviewRequest>
{
    public UpdateInterviewValidator()
    {
        When(x => x.Project != null, () =>
        {
            RuleFor(x => x.Project)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.Account != null, () =>
        {
            RuleFor(x => x.Account)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.Interviewer != null, () =>
        {
            RuleFor(x => x.Interviewer)
                .NotEmpty()
                .MaximumLength(150)
                .EmailAddress();
        });

        When(x => x.InterviewDate != null, () =>
        {
            RuleFor(x => x.InterviewDate)
                .GreaterThan(DateTime.UtcNow.AddMinutes(-1));
        });

        When(x => x.Level != null, () =>
        {
            RuleFor(x => x.Level)
                .InclusiveBetween(1, 2);
        });
    }
}