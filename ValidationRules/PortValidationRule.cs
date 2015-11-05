#region

using System.Globalization;
using System.Windows.Controls;

#endregion

namespace EscInstaller.ValidationRules
{
    public class PortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int i;
            if (value != null && int.TryParse(value.ToString(), out i) && i > 0 && i < 65537)
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, "The value is not a valid e-mail address");
        }
    }
}