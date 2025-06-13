namespace Data
{
    /// <summary>
    /// Database object used for all connections to the database.
    /// </summary>
    public class Database : System.IDisposable
    {
        #region Variables
        private const ushort _timeoutinit = 240;    //2016-Nov-16 Vision modified from 120           // SQL Time Out Initialization Variable
        private static volatile ushort _timeout = _timeoutinit;                     // SQL Time out Duration
        private static volatile string _connectionstring = string.Empty;            // Connection String
        private System.Data.SqlClient.SqlConnection _connection = null;             // Connection Object
        private System.Data.SqlClient.SqlTransaction _transaction = null;           // Transaction Object
        private System.Data.IsolationLevel _isolationlevel;                         // Transaction IsolationLevel
        private System.Data.SqlClient.SqlDataAdapter _adapter = null;               // SQL Data Adapter
        private readonly Data.CommandFactory _factory = new Data.CommandFactory();  // Command Factory
        protected bool _transactional = false;                                      // If Connection Is To Be Transactional
        private static bool _firstload = true;                                      // If first load
        private string _transactionname = string.Empty;                             // Transaction Name
        private bool _disposed = false;                                             // Track whether Dispose has been called
        #endregion

        #region Constructors
        static Database()
        {
           
        }
        /// <summary>
        /// Opens a transactional connection with specific Isolation Level with connection string or not
        /// </summary>
        /// <param name="transactional">Boolean of Transaction</param>
        /// <param name="isolationlevel">Transaction Isolation Level</param>
        /// <param name="connectionstring">Connection String</param>
        public Database(bool transactional, System.Data.IsolationLevel isolationlevel,string connectionstring) : base()
        {
            this._isolationlevel = isolationlevel;
            this._transactional = transactional;

            if (null != connectionstring && connectionstring.Length > 0)
                _connectionstring = connectionstring;

            if (!(_firstload))
                _timeout = TimeOutInit();

            _firstload = false;
            this.OpenConnection();
        }
        /// <summary>
        /// Opens a transactional/non-trannsactional connection with connection string or not.
        /// </summary>
        /// <param name="transactional">if Connection Should be Transactional</param>
        public Database(bool transactional,string connectionstring) : this(transactional, System.Data.IsolationLevel.Serializable,connectionstring)
        { }
        /// <summary>
        /// Opens a non-transactional connection.
        /// </summary>
        public Database() : this(false,null)
        {}
        /// <summary>
        /// Opens a non-transactional connection with connection string
        /// </summary>
        /// <param name="connectionstring">Connection String</param>
        public Database(string connectionstring) : this(false,connectionstring)
        {}

        /// <summary>
        /// De-Constructor
        /// </summary>
        ~Database()
        {
            this.Dispose(false);
        }
        #endregion

        #region Methods
        private static ushort TimeOutInit()
        {
            return (null != Data.ConnectKeys.ConnectTimeOut) ? (ushort)(int)Data.ConnectKeys.ConnectTimeOut : _timeoutinit;
        }

        #region Execution Methods
        /// <summary>
        /// Executes the object on the database.
        /// </summary>
        /// <param name="obj">Object For Query</param>
        /// <returns>array of results</returns>
        //public virtual object[] Execute(System.Collections.Generic.Dictionary<string,object> paramobjects)
        //{
        //    if (null != paramobjects)
        //    {
        //        bool select = false;
        //        System.Data.SqlClient.SqlCommand _command = factory.CreateCommand(ref paramobjects, ref this._transaction, ref this._connection, ref select);

        //        return (select) ? this.BuildArray(this.ExecuteQuery(ref _command), obj.ObjectType) : this.BuildArray(this.ExecuteNonQuery(ref _command));
        //    }
        //    else
        //    {
        //        return new object[0];
        //    }
        //}
        /// <summary>
        /// Executes a query with a forced query type.
        /// </summary>
        /// <param name="obj">Object For Query</param>
        /// <param name="query">is Query with Return</param>
        /// <returns>Object Array</returns>
        //public virtual object[] Execute(Data.IDatabaseInteroperable obj, bool query)
        //{
        //    if (null != obj)
        //    {
        //        System.Data.SqlClient.SqlCommand _command = _factory.CreateCommand(ref obj, ref this._transaction, ref this._connection);
        //        return (query) ? this.BuildArray(this.ExecuteQuery(ref _command), obj.ObjectType) : this.BuildArray(this.ExecuteNonQuery(ref _command));
        //    }
        //    else
        //    {
        //        return new object[0];
        //    }
        //}

        /// <summary>
        /// Execute no query command
        /// </summary>
        /// <param name="strsql">SQL string or Proceture Name for access</param>
        /// <param name="paramobjects">Parameter Objects</param>
        /// <param name="commandtype">Command Type</param>
        /// <returns>Rows Affacted</returns>
        public virtual int ExecuteNonQuery(string strsql, System.Collections.Generic.Dictionary<string, object> paramobjects, CommandType commandtype)
        {
            if (!string.IsNullOrEmpty(strsql))
            {
                System.Data.SqlClient.SqlCommand _command = this._factory.CreateCommand(ref strsql, ref paramobjects, ref _transaction, ref _connection,commandtype);
                return this.ExecuteNonQuery(ref _command);
            }
            else
                return 0;
        }
        /// <summary>
        /// For Basic SQL Statements
        /// </summary>
        /// <param name="statement">Statement or Proceture Name To Execute</param>
        /// <returns>Rows Affected</returns>
        public virtual int ExecuteNonQuery(string strsql, CommandType commandtype)
        {
            if (!string.IsNullOrEmpty(strsql))
            {
                System.Data.SqlClient.SqlCommand _command = this._factory.CreateCommand(ref strsql, ref this._transaction, ref this._connection, commandtype);
                return this.ExecuteNonQuery(ref _command);
            }
            else
                return 0;
        }

        /// <summary>
        /// Executes Insert Table
        /// </summary>
        /// <param name="tablename">Table Name</param>
        /// <param name="paramobjects">Objects for Insert</param>
        /// <param name="commandtype">Command Type</param>
        /// <returns>Rows Affected</returns>
        public virtual int ExecuteInsert(string tablename, System.Collections.Generic.Dictionary<string, object> paramobjects,CommandType commandtype)
        {
            if (!string.IsNullOrEmpty(tablename) && null != paramobjects)
            {
                System.Data.SqlClient.SqlCommand _command = this._factory.CreateCommand(ref tablename, ref paramobjects, ref this._transaction, ref this._connection, commandtype);
                return this.ExecuteNonQuery(ref _command);
            }
            else
                return 0;
        }
        /// <summary>
        /// Executes Insert Statement
        /// </summary>
        /// <param name="row">Data Row</param>
        /// <returns>Rows Affected</returns>
        public virtual int ExecuteInsert(System.Data.DataRow row)
        {
            System.Data.SqlClient.SqlCommand _command = this._factory.CreateCommand(ref row, ref this._transaction, ref this._connection);
            return this.ExecuteNonQuery(ref _command);
        }
        /// <summary>
        /// Executes the object on the database.
        /// </summary>
        /// <param name="obj">Object For Query</param>
        /// <returns>datatable of the results</returns>
        public virtual System.Data.DataTable ExecuteDataTable(string strsql, System.Collections.Generic.Dictionary<string, object> paramobjects, CommandType commandtype)
        {
            if (!string.IsNullOrEmpty(strsql) && null != paramobjects)
            {
                System.Data.SqlClient.SqlCommand _command = this._factory.CreateCommand(ref strsql, ref paramobjects, ref this._transaction, ref this._connection,commandtype);
                return this.ExecuteQuery(ref _command);
            }
            else
                return null;
        }
        /// <summary>
        /// Executes a dynamic sql statement. The obj parameter can be set to null if no extra parameters
        /// are needed.
        /// </summary>
        /// <param name="sql">SQL Statement</param>
        /// <param name="commandtype">Command Type</param>
        /// <returns>Data Table</returns>
        public virtual System.Data.DataTable ExecuteDataTable(string strsql, CommandType commandtype)
        {
            if (!string.IsNullOrEmpty(strsql))
            {
                System.Data.SqlClient.SqlCommand _command = this._factory.CreateCommand(ref strsql, ref this._transaction, ref this._connection, commandtype);
                return this.ExecuteQuery(ref _command);
            }
            else
                return null;
        }
        /// <summary>
        /// Executes theobject on the database
        /// </summary>
        /// <param name="strsql">Sql statement or stored procedure name</param>
        /// <param name="paramobjects">parameters objects</param>
        /// <param name="commandtype">Command type</param>
        /// <returns></returns>
        public virtual System.Data.DataTableCollection ExecuteDataTables(string strsql,System.Collections.Generic.Dictionary<string,object> paramobjects, CommandType commandtype)
        {
            if(!string.IsNullOrEmpty(strsql))
            {
                System.Data.SqlClient.SqlCommand _command = this._factory.CreateCommand(ref strsql,ref paramobjects, ref this._transaction, ref this._connection, commandtype);
                return this.ExecuteQuerys(ref _command);
            }
            else
                return null;
        }

       

        /// <summary>
        /// Execute Query to return value in Datatable format single value
        /// </summary>
        /// <param name="strsql">SQL Statement</param>
        /// <param name="commandtype">Command Type</param>
        /// <returns>String converted from datatable</returns>
        public virtual string ExecuteSingleString(string strsql, System.Collections.Generic.Dictionary<string, object> paramobjects, CommandType commandtype)    
        {
            if (!string.IsNullOrEmpty(strsql))
            {
                System.Data.SqlClient.SqlCommand _command = this._factory.CreateCommand(ref strsql, ref paramobjects, ref _transaction, ref _connection,commandtype);
                System.Data.DataTable _datatable = this.ExecuteQuery(ref _command);
                return (_datatable.Rows.Count > 0) ? _datatable.Rows[0][0].ToString() : null;
            }
            else
                return null;
        }
        /// <summary>
        /// Execute a query on the database
        /// </summary>
        /// <param name="command">Query command</param>
        /// <returns>String</returns>
        protected virtual string ExecuteQuery(ref System.Data.SqlClient.SqlCommand command,System.Data.DbType returntype)
        {
            string _returnvalue = "";

            command.ExecuteNonQuery();
            if(command.Parameters.Contains("@ReturnValue"))
                _returnvalue = command.Parameters["@ReturnValue"].Value.ToString();

            command.Dispose();
            command = null;

            return _returnvalue; 
        }

        /// <summary>
        /// Executes a query on the database.
        /// </summary>
        /// <param name="command">query</param>
        /// <returns>data table</returns>
        protected virtual System.Data.DataTable ExecuteQuery(ref System.Data.SqlClient.SqlCommand command)
        {
            System.Data.DataTable _datatable = new System.Data.DataTable();
            using (this._adapter = new System.Data.SqlClient.SqlDataAdapter(command))
            {
                this._adapter.Fill(_datatable);
            }
            command.Dispose();
            command = null;
            this._adapter = null;

            return _datatable;
        }

        /// <summary>
        /// Executes more query on the database
        /// </summary>
        /// <param name="command">Query</param>
        /// <returns>DataTable Collection</returns>
        protected virtual System.Data.DataTableCollection ExecuteQuerys(ref System.Data.SqlClient.SqlCommand command)
        {
            System.Data.DataSet _dataset = new System.Data.DataSet();
            try
            { 
                using (this._adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                {
                    this._adapter.Fill(_dataset);
                }
                command.Dispose();
                command = null;
                this._adapter = null;

                return _dataset.Tables;
            }
            catch (System.Data.SqlClient.SqlException sqlex)
            {
                System.Console.WriteLine(sqlex);
                return _dataset.Tables;
            }
        }

        /// <summary>
        /// Executes a non-query on the database.
        /// </summary>
        /// <param name="command">non-query</param>
        /// <returns>rows affected</returns>
        protected virtual int ExecuteNonQuery(ref System.Data.SqlClient.SqlCommand command)
        {
            int _rows = command.ExecuteNonQuery();
            command.Dispose();
            command = null;

            return _rows;
        }

        /// <summary>
        /// Builds the array using a returned data table, casting the objects inserted to the specified type.
        /// </summary>
        /// <param name="data">returned data table</param>
        /// <param name="type">type of objects in the array</param>
        /// <returns>array of objects</returns>
        //protected virtual object[] BuildArray(System.Data.DataTable data, System.Type type)
        //{
        //    uint i = 0;
        //    object[] array = new object[data.Rows.Count];
        //    foreach (System.Data.DataRow row in data.Rows)
        //    {
        //        array[i] = System.Activator.CreateInstance(type, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
        //            System.Reflection.BindingFlags.CreateInstance, null, null, null);
        //        if (array[i] is Data.IFillable)
        //        {
        //            ((Data.IFillable)array[i]).FillFromDataSource(row, data.Columns);
        //        }
        //        i++;
        //    }
        //    return array;
        //}
        /// <summary>
        /// Builds the array using the returned affected rows.
        /// </summary>
        /// <param name="rows">Number of Rows</param>
        /// <returns>Object Array</returns>
        protected virtual object[] BuildArray(int rows)
        {
            return new object[1] { rows };
        }
        #endregion

        #region Connection Methods
        /// <summary>
        /// Opens a transactional/non-trannsactional _connection.
        /// </summary>
        protected virtual void OpenConnection()
        {
            if (string.IsNullOrEmpty(_connectionstring))
                throw new System.NullReferenceException("Connection String Has Not Been Initialized.");

            this._connection = new System.Data.SqlClient.SqlConnection(_connectionstring);

            if (System.Data.ConnectionState.Open != this._connection.State)
                try
                {
                    this._connection.Open();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    System.Console.Write(ex.ToString());
                }
            if (this._transactional)
                this.BeginTransaction();
        }
        /// <summary>
        /// Closes the _connection.
        /// </summary>
        internal virtual void CloseConnection()
        {
            if (null != this._transaction)
            {
                this._transaction.Dispose();
                this._transaction = null;
            }
            if (null != this._connection)
            {
                if (this._connection.State == System.Data.ConnectionState.Open)
                    this._connection.Close();

                this._connection.Dispose();
                this._connection = null;
            }
            if (null != this._adapter)
            {
                this._adapter.Dispose();
                this._adapter = null;
            }
        }
        #endregion


        #region Transaction Methods
        /// <summary>
        /// Begins the Transaction.
        /// </summary>
        public void BeginTransaction()
        {
            this.BeginTransaction(this._isolationlevel);
        }
        /// <summary>
        /// Begins the Transaction.
        /// </summary>
        /// <param name="isolation">Isolation Level</param>
        public void BeginTransaction(System.Data.IsolationLevel isolation)
        {
            string id = System.Guid.NewGuid().ToString();
            this.BeginTransaction(isolation, ((id.Length > 32) ? id.Substring(0, 32) : id));
        }
        /// <summary>
        /// Begins the Transaction.
        /// </summary>
        /// <param name="isolation">Isolation Level</param>
        /// <param name="name">Transaction Name</param>
        public virtual void BeginTransaction(System.Data.IsolationLevel isolation, string name)
        {
            this._transactionname = name;
            this._transaction = this._connection.BeginTransaction(isolation, name);
        }

        /// <summary>
        /// Commits the transactions to the database.
        /// </summary>
        public virtual void CommitTransaction()
        {
            if (null != this._transaction)
            {
                this._transaction.Commit();
                this._transaction.Dispose();
                this._transaction = null;
            }
        }
        /// <summary>
        /// Rolls back the _transaction.
        /// </summary>
        /// <remarks>
        /// todo - could throw an exception.
        /// </remarks>
        public virtual void RollbackTransaction()
        {
            if (null != this._transaction)
            {
                this._transaction.Rollback(this._transactionname);
                this._transaction.Dispose();
                this._transaction = null;
            }
        }
        #endregion

        #region Dispose Methods
        /// <summary>
        /// Disposes all resources used by the database.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes all resources used by the database.
        /// </summary>
        /// <param name="disposing">Object Disposing</param>
        public virtual void Dispose(bool disposing)
        {
            if (disposing)  // If disposing equals true, dispose all managed and unmanaged resources.
            {
                this.CloseConnection(); // Dispose managed resources.
            }
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Specifies the Query Time Duration for all queries
        /// </summary>
        public static ushort QueryTimeOut
        {
            get{return _timeout;}
            set{_timeout = value;}
        }
        #endregion
    }
}
