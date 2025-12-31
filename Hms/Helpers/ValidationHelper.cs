using FluentValidation.Results;

namespace Hms.Helpers
{
    public static class ValidationHelper
    {
        public static List<string> GetErrorMessages(ValidationResult validationResult)
        {
            return validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();
        }

        public static Dictionary<string, string> GetPropertyErrors(ValidationResult validationResult)
        {
            return validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(e => e.ErrorMessage))
                );
        }
    }
}

