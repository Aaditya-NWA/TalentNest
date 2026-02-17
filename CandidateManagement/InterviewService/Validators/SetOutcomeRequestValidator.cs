using FluentValidation;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.Models.Enums;
using System.Diagnostics.CodeAnalysis;

namespace InterviewService.Validators;

[ExcludeFromCodeCoverage]
public class SetOutcomeRequestValidator : AbstractValidator<SetOutcomeRequest>
{
    public SetOutcomeRequestValidator()
    {
        // Validate Outcome
        RuleFor(x => x.Outcome)
            .NotNull()
            .WithMessage("Outcome is required.")

            .IsInEnum()
            .WithMessage("Invalid outcome value. Valid values: 0 = Pending, 1 = Selected, 2 = Rejected.")

            .NotEqual(InterviewOutcome.Pending)
            .WithMessage("Final outcome cannot be set to Pending. Please select either Selected or Rejected.")

            .Must(outcome => outcome == InterviewOutcome.Selected || outcome == InterviewOutcome.Rejected)
            .WithMessage("Outcome must be either Selected (1) or Rejected (2).");

        // Validate DecisionMaker
        RuleFor(x => x.DecisionMaker)
            .NotNull()
            .WithMessage("Decision maker is required.")

            .IsInEnum()
            .WithMessage("Invalid decision maker value. Valid values: 0 = NotApplicable, 1 = Client, 2 = Internal, 3 = CandidateWithdrawal.")

            .NotEqual(DecisionMaker.NotApplicable)
            .WithMessage("Decision maker cannot be NotApplicable. Please select who made the decision (Client, Internal, or CandidateWithdrawal).")

            .Must(decisionMaker => decisionMaker == DecisionMaker.Client ||
                                   decisionMaker == DecisionMaker.Internal ||
                                   decisionMaker == DecisionMaker.CandidateWithdrawal)
            .WithMessage("Decision maker must be Client (1), Internal (2), or CandidateWithdrawal (3).");

        // Custom Rule: If Outcome is Selected, DecisionMaker cannot be CandidateWithdrawal
        RuleFor(x => x)
            .Must(x => !(x.Outcome == InterviewOutcome.Selected && x.DecisionMaker == DecisionMaker.CandidateWithdrawal))
            .WithMessage("Decision maker cannot be CandidateWithdrawal when outcome is Selected.");

        // Custom Rule: If Outcome is Rejected, DecisionMaker should be specified
        RuleFor(x => x.DecisionMaker)
            .NotEqual(DecisionMaker.NotApplicable)
            .When(x => x.Outcome == InterviewOutcome.Rejected)
            .WithMessage("Decision maker must be specified when outcome is Rejected.");
    }
}