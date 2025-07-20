using System.Collections.Generic;

namespace WorkflowPlatform.Domain.Workflows.NodeExecution
{
    /// <summary>
    /// Validation result for node execution
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public List<string> Errors { get; private set; }
        public List<string> Warnings { get; private set; }

        public ValidationResult()
        {
            IsValid = true;
            Errors = new List<string>();
            Warnings = new List<string>();
        }

        public static ValidationResult Success()
        {
            return new ValidationResult();
        }

        public static ValidationResult Failure(params string[] errors)
        {
            var result = new ValidationResult
            {
                IsValid = false
            };
            result.Errors.AddRange(errors);
            return result;
        }

        public static ValidationResult Failure(IEnumerable<string> errors)
        {
            var result = new ValidationResult
            {
                IsValid = false
            };
            result.Errors.AddRange(errors);
            return result;
        }

        public ValidationResult AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
            return this;
        }

        public ValidationResult AddWarning(string warning)
        {
            Warnings.Add(warning);
            return this;
        }

        public ValidationResult Combine(ValidationResult other)
        {
            if (!other.IsValid)
            {
                IsValid = false;
                Errors.AddRange(other.Errors);
            }
            Warnings.AddRange(other.Warnings);
            return this;
        }
    }
}
