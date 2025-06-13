namespace Data
{
    /// <summary>
    /// Builds SQL Statements
    /// </summary>
    internal sealed class CommandFactory
    {
        #region Constructors
        /// <summary>
        /// Static Constructor
        /// </summary>
        static CommandFactory(){ }
        #endregion

        #region Methods
        
        /// <summary>
        /// Builds all the queries without a forced query type.
        /// </summary>
        /// <param name="sql">SQL Statement or Proceture Name</param>
        /// <param name="transaction">Transaction To Execute</param>
        /// <param name="connection">Connection For Transaction</param>
        /// <param name="commandtype">Command Type</param>
        /// <returns>SQL Command Object</returns>
        internal System.Data.SqlClient.SqlCommand CreateCommand(ref string sql, ref System.Data.SqlClient.SqlTransaction transaction, ref System.Data.SqlClient.SqlConnection connection, CommandType commandtype)
        {
            System.Data.SqlClient.SqlCommand _command = new System.Data.SqlClient.SqlCommand();
            _command.CommandTimeout = Database.QueryTimeOut;
            _command.Connection = connection;
            _command.Transaction = transaction;
            _command.CommandText = sql;
            _command.CommandType = ((byte)commandtype == 1) ? System.Data.CommandType.StoredProcedure : ((byte)commandtype == 2) ? System.Data.CommandType.TableDirect : System.Data.CommandType.Text;

            return _command;
        }
        /// <summary>
        /// Builds queries with a forced query type.
        /// </summary>
        /// <exception cref="System.Exception">
        ///	If SQL Statement is Null
        /// </exception>
        /// <param name="sql">SQL Statement or Proceture Name</param>
        /// <param name="paramobjects">Object For Query</param>
        /// <param name="transaction">Transaction To Execute</param>
        /// <param name="connection">Connection For Transaction</param>
        /// <param name="commandtype">Command Type</param>
        /// <returns>SQL Command Object</returns>
        internal System.Data.SqlClient.SqlCommand CreateCommand(ref string sql, ref System.Collections.Generic.Dictionary<string, object> paramobjects,
            ref System.Data.SqlClient.SqlTransaction transaction, ref System.Data.SqlClient.SqlConnection connection,CommandType commandtype)
        {
            try
            {
                if(commandtype == CommandType.TableInsert)  //sql string only table name
                {
                    sql = "INSERT INTO " + sql + " VALUES(";
                    foreach (string key in paramobjects.Keys)
                        sql += $"@{key},";

                    sql = sql.Substring(0, sql.Length - 1) + ") ";
                    commandtype = CommandType.Text;
                }

                System.Data.SqlClient.SqlCommand _command = this.CreateCommand(ref sql, ref transaction, ref connection, commandtype);
                this.SetCommandParameters(ref paramobjects, ref _command, sql, commandtype);
                return _command;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.ToString());
            }
        }
        
        /// <summary>
        /// Creates Command with Parameters from DataRow
        /// </summary>
        /// <param name="row">Data Row</param>
        /// <param name="transaction">Transaction To Execute</param>
        /// <param name="connection">Connection For Transaction</param>
        /// <returns>Sql Command Object</returns>
        internal System.Data.SqlClient.SqlCommand CreateCommand(ref System.Data.DataRow row, ref System.Data.SqlClient.SqlTransaction transaction,
            ref System.Data.SqlClient.SqlConnection connection)
        {
            const string comaSpace = ", ";
            System.Data.SqlClient.SqlCommand _command = new System.Data.SqlClient.SqlCommand();
            _command.CommandTimeout = Data.Database.QueryTimeOut;
            _command.Connection = connection;
            _command.Transaction = transaction;

            System.Text.StringBuilder sql = new System.Text.StringBuilder(), values = new System.Text.StringBuilder();

            foreach (System.Data.DataColumn column in row.Table.Columns)
            {
                sql.Append(((0 < sql.Length) ? comaSpace : string.Empty) + column.ColumnName);
                values.Append(((0 < values.Length) ? comaSpace : string.Empty) + "@" + column.ColumnName);

                this.AddParameter(ref _command, "@" + (string)column.ColumnName, row[column]);
            }

            _command.CommandText = string.Format("INSERT INTO {0}({1})VALUES({2});", row.Table.TableName, sql.ToString(), values.ToString());
            return _command;
        }

        /// <summary>
        /// Sets the command parameters with Return parameter
        /// </summary>
        /// <param name="paramobjects">Objects</param>
        /// <param name="sql">SQL Statemnet or Proceture Name</param>
        /// <param name="command">Command Object</param>
        /// <param name="returntype">Return Parameter Data Type</param>
        private void SetCommandParameters(ref System.Collections.Generic.Dictionary<string, object> paramobjects,
                    ref System.Data.SqlClient.SqlCommand command, string sql, CommandType commandtype,System.Data.DbType returntype)
        {
            if(null != paramobjects)
                SetCommandParameters(ref paramobjects, ref command, sql, commandtype);
            
            System.Data.SqlClient.SqlParameter returnparam = new System.Data.SqlClient.SqlParameter("@ReturnValue", returntype);
            returnparam.Direction = System.Data.ParameterDirection.ReturnValue;

            command.Parameters.Add(returnparam);
        }

        /// <summary>
        /// Sets the command parameters.
        /// </summary>
        /// <param name="paramobjects">Objects</param>
        /// <param name="sql">SQL Statemnet or Proceture Name</param>
        /// <param name="command">Command Object</param>
        private void SetCommandParameters(ref System.Collections.Generic.Dictionary<string, object> paramobjects, ref System.Data.SqlClient.SqlCommand command, string sql,CommandType commandtype)
        {
            if (commandtype == CommandType.Text)
            {
                System.Text.RegularExpressions.MatchCollection matches = Text.Regex.Matches(sql, Text.Expression.Expressions.Sql);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (!(command.Parameters.Contains("@" + match.Value)))
                        command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@" + match.Value, paramobjects[match.Value]));
                }
            }
            else   //command type is stored proceture
                foreach (string key in paramobjects.Keys)
                    command.Parameters.Add(new System.Data.SqlClient.SqlParameter(key, paramobjects[key]));
        }


        /// <summary>
        /// Adds A Paratmeter
        /// </summary>
        /// <param name="_command">Command Object</param>
        /// <param name="key">Key</param>
        /// <param name="val">Value</param>
        private void AddParameter(ref System.Data.SqlClient.SqlCommand _command, string key, object val)
        {
            if (!string.IsNullOrEmpty(key))
                if (!(_command.Parameters.Contains(key)))
                    _command.Parameters.Add(new System.Data.SqlClient.SqlParameter(key, val));
        }
        #endregion
    }
}
