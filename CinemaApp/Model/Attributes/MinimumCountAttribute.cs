using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model.Attributes;

public class MinimumCountAttribute : ValidationAttribute
{
    private readonly int _minCount;

    public MinimumCountAttribute(int minCount)
    {
        _minCount = minCount;
        ErrorMessage = $"The collection must contain at least {minCount} item(s).";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is ICollection collection)
        {
            if (collection.Count < _minCount)
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}
