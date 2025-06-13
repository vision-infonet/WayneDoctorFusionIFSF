namespace Text
{
    /// <summary>
    /// Regex
    /// </summary>
    public class Regex 
    {
        #region Variables
        /// <summary>
        /// Expression Object
        /// </summary>
        private static Expression expressions;
        #endregion

        #region Constructors
        /// <summary>
        /// Static Constructor
        /// </summary>
        static Regex()
        {
            expressions = new Expression();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the Regular Expression.
        /// </summary>
        /// <param name="exp">Expression</param>
        /// <returns>String</returns>
        private static string RegularExpression(ref System.Enum exp)
        {
            return expressions.Get(ref exp);
        }
        /// <summary>
        /// Gets all the matches to this pattern in this input.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="exp">Expression</param>
        /// <returns>Match Collection</returns>
        public static System.Text.RegularExpressions.MatchCollection Matches(string input, System.Enum exp)
        {
            return System.Text.RegularExpressions.Regex.Matches(input, RegularExpression(ref exp));
        }
        /// <summary>
        /// Checks for a match.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="exp">Expression</param>
        /// <returns>Is Match</returns>
        public static bool IsMatch(string input, System.Enum exp)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, RegularExpression(ref exp));
        }
        /// <summary>
        /// Checks for a match.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="exp">Expression</param>
        /// <returns>Is Match</returns>
        public static bool IsMatch(string input, string exp)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, exp);
        }
        /// <summary>
        /// Gets the first match.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="exp">Expression</param>
        /// <returns>First Match</returns>
        public static string FirstMatch(string input, System.Enum exp)
        {
            return System.Text.RegularExpressions.Regex.Match(input, RegularExpression(ref exp)).Value;
        }
        /// <summary>
        /// Replaces all the matches to the exppression with the replacement string.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="exp">Expression</param>
        /// <param name="replacement">Replacement</param>
        /// <returns>Replace</returns>
        public static string Replace(string input, System.Enum exp, string replacement)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, RegularExpression(ref exp), replacement);
        }
        /// <summary>
        /// Gets the number of pattern instances found in the input.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="pattern">Pattern</param>
        /// <returns>Match Count</returns>
        public static int MatchCount(string input, System.Enum exp)
        {
            return MatchCount(input, RegularExpression(ref exp));
        }
        /// <summary>
        /// Gets the number of pattern instances found in the input.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="pattern">Pattern To Match</param>
        /// <returns>Match Count</returns>
        public static int MatchCount(string input, string pattern)
        {
            return System.Text.RegularExpressions.Regex.Matches(input, pattern).Count;
        }
        /// <summary>
        /// Gets the match captures.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="exp">Expression</param>
        /// <returns>Group Collection</returns>
        public static System.Text.RegularExpressions.GroupCollection Groups(string input, System.Enum exp)
        {
            return System.Text.RegularExpressions.Regex.Match(input, RegularExpression(ref exp)).Groups;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Sets the expression list.
        /// </summary>
        public static Text.Expression ExpressionList
        {
            set
            {
                lock (expressions)
                {
                    expressions = value;
                }
            }
        }
        #endregion
    }
}
