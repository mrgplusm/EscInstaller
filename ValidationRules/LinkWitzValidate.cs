#region

using System.Globalization;
using System.Windows.Controls;

#endregion

namespace EscInstaller.ValidationRules
{
    public class LinkWitzValidate : ValidationRule
    {
        private static bool IsValid(string number)
        {
            int n;
            if (!int.TryParse(number, out n)) return false;
            return n%2 == 0;
        }

        public override ValidationResult Validate(object value, CultureInfo ultureInfo)
        {
            if (value != null && IsValid(value.ToString()))
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, "Only even numbers are valid");
        }
    }
}