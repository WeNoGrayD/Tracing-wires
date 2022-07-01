using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Controls;

namespace TraceWiresViewModel
{
    public class TMAlphabetValidationRule// : ValidationRule
    {
        /*
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string alphabet = (string)value;
            string errMes = "";

            if (!TuringMachine.CheckAlphabetOnRepeatingChars(alphabet.ToCharArray()))
            {
                errMes = "Алфавит состоит из повторяющихся символов. Пожалуйста, уберите повторяющиеся символы.";
                return new ValidationResult(false, errMes);
            }
            if (alphabet.Contains('_'))
            {
                errMes = "Алфавит содержит пробел ('_') - символ по умолчанию. Пожалуйста, уберите '_'.";
                return new ValidationResult(false, errMes);
            }

            return new ValidationResult(true, null);
        }
        */
    }
}
