using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagementApp.CustomValidator
{
    public class DateLessThanToday : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var DateValue = value as DateTime? ?? new DateTime();
            if(DateValue.Date >= DateTime.Now.Date)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Due date cannot be in the past.");

        }
    }
}