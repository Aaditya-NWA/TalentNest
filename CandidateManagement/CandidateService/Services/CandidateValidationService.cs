using CandidateService.DTOs;

namespace CandidateService.Services
{
    public static class CandidateValidationService
    {
        public static List<string> Validate(CreateCandidateRequest request, int rowNumber = 0)
        {
            var errors = new List<string>();
            var prefix = rowNumber > 0 ? $"Row {rowNumber}: " : string.Empty;

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add($"{prefix}Name is required.");

            if (string.IsNullOrWhiteSpace(request.MailId))
                errors.Add($"{prefix}MailId is required.");

            if (string.IsNullOrWhiteSpace(request.SkillSet))
                errors.Add($"{prefix}SkillSet is required.");

            if (request.ExperienceMonths <= 0)
                errors.Add($"{prefix}ExperienceMonths cannot be less than or equal to zero.");

            if (request.PrimarySkillLevel is null ||
                !System.Text.RegularExpressions.Regex.IsMatch(
                    request.PrimarySkillLevel,
                    @"(?i)^p[0-5]$"))
            {
                errors.Add($"{prefix}PrimarySkillLevel must be P0–P5.");
            }

            if (request.AvailabilityDate == default)
                errors.Add($"{prefix}AvailabilityDate is required.");

            return errors;
        }
    }
}
