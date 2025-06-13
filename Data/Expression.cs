namespace Text
{
    /// <summary>
    /// Expression
    /// </summary>
    public class Expression : object
    {
        #region Varaibles
        /// <summary>
        /// List of Regular Expressions.
        /// </summary>
        private const string
            Anchor = @"(?<=#)(?:\w+)",
            Sql = @"(?<=@)(?:\w+)",
            Jpeg = @"(?i:\.jp(e)?g)",
            FileName = @"[^\w-]+?";

        #endregion

        #region Enum
        /// <summary>
        /// Enum of Expressions
        /// </summary>
        public enum Expressions : byte
        {
            Sql = 1,
            Jpeg = 2,
            FileName = 3,
            Anchor = 4
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets Expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>Expression</returns>
        public virtual string Get(ref System.Enum expression)
        {
            if (expression is Text.Expression.Expressions)
            {
                switch ((Expressions)expression)
                {
                    case Expressions.Sql:
                        return Sql;
                    case Expressions.Jpeg:
                        return Jpeg;
                    case Expressions.FileName:
                        return FileName;
                    case Expressions.Anchor:
                        return Anchor;
                }
            }
            return string.Empty;
        }
        #endregion
    }
}
