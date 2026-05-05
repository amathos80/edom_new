using System.Text.RegularExpressions;

public class PasswordPolicy
    {

        private static int Upper_Case_length = 1;
        private static int Lower_Case_length = 1;
        private static int NonAlpha_length = 1;
        private static int Numeric_length = 1;

        public static bool IsPasswordValid(string Password)
        {
            int tot = 0;




            //return false;
            if (UpperCaseCount(Password) < Upper_Case_length)
            {
                tot += 0;
            }
            else
            {
                tot += 1;
            }
            //  return false;
            if (LowerCaseCount(Password) < Lower_Case_length)
            {
                tot += 0;
            }
            else
            {
                tot += 1;
            }
            // return false;
            if (NumericCount(Password) < 1)
            {
                tot += 0;
            }
            else
            {
                tot += 1;
            }
            //return false;
            if (NonAlphaCount(Password) < NonAlpha_length)
            {
                tot += 0;
            }
            else
            {
                tot += 1;
            }
            // return false;
            if (tot < 3)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static int UpperCaseCount(string Password)
        {
            return Regex.Matches(Password, "[A-Z]").Count;
        }

        private static int LowerCaseCount(string Password)
        {
            return Regex.Matches(Password, "[a-z]").Count;
        }
        private static int NumericCount(string Password)
        {
            return Regex.Matches(Password, "[0-9]").Count;
        }
        private static int NonAlphaCount(string Password)
        {
            return Regex.Matches(Password, @"[^0-9a-zA-Z\._]").Count;
        }
    }