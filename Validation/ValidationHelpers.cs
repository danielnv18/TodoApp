using System.ComponentModel.DataAnnotations;

namespace TodoApi.Validation;

/// <summary>
/// Runs data-annotation validation and returns any errors as a dictionary suitable for ValidationProblem responses.
/// </summary>
public static class ValidationHelpers
{
    public static Dictionary<string, string[]>? Validate<T>(T model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model!);
        Validator.TryValidateObject(model!, context, validationResults, validateAllProperties: true);

        if (validationResults.Count == 0)
        {
            return null;
        }

        return validationResults
            .SelectMany(result =>
            {
                var members = result.MemberNames?.Any() == true ? result.MemberNames : new[] { string.Empty };
                return members.Select(member => new
                {
                    Member = string.IsNullOrWhiteSpace(member) ? "request" : member,
                    Error = result.ErrorMessage ?? "Invalid value."
                });
            })
            .GroupBy(entry => entry.Member, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.Error).ToArray());
    }
}
