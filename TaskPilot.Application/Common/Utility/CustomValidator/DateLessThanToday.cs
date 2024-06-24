using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Application.Common.Utility.CustomValidator
{
    public class DateLessThanToday : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var DateValue = value as DateTime? ?? new DateTime();
            if (DateValue.Date >= DateTime.Now.Date)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Due date cannot be in the past.");

        }
    }
}
