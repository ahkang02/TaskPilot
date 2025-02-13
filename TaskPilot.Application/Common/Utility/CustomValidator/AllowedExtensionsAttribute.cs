﻿using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TaskPilot.Application.Common.Utility.CustomValidator
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success!;
        }

        private string GetErrorMessage()
        {
            return $"Only files with extensions: {string.Join(", ", _extensions)} are allowed.";
        }
    }
}
