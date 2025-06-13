//namespace Data
//{
//    #region Interfaces
//    /// <summary>
//    /// Database Thread Interface
//    /// </summary>
//    internal interface IDatabaseThread
//    {
//        #region Methods
//        /// <summary>
//        /// For Suspending Running Thread
//        /// </summary>
//        void Suspend();
//        /// <summary>
//        /// For Resuming Running Thread
//        /// </summary>
//        void Resume();
//        #endregion

//        #region Properties
//        /// <summary>
//        /// Sets The Database Connection
//        /// </summary>
//        Data.PooledDatabase Database
//        {
//            set;
//        }
//        #endregion
//    }
//    /// <summary>
//    /// Request Database Thread Interface
//    /// </summary>
//    internal interface IRequestDatabaseThread
//    {
//        #region Events
//        /// <summary>
//        /// Request Database Event
//        /// </summary>
//        event InfoNet.Windows.Services.Cluster.RequestDatabase RequestDatabase;
//        #endregion

//        #region Properties
//        /// <summary>
//        /// Pooled Database
//        /// </summary>
//        FCSystem.Library.Data.PooledDatabase PooledDatabase
//        {
//            get;
//            set;
//        }
//        #endregion
//    }
//    /// <summary>
//    /// Sql Key Interface
//    /// </summary>
//    public interface ISqlKey
//    {
//        #region Properties
//        /// <summary>
//        /// Sql Key
//        /// </summary>
//        string SqlKey
//        {
//            get;
//        }
//        #endregion
//    }
//    /// <summary>
//    /// Data Fillable
//    /// </summary>
//    public interface IFillable
//    {
//        #region Methods
//        /// <summary>
//        /// Fills Data from Data Source
//        /// </summary>
//        /// <param name="row">Row (Tuple)</param>
//        /// <param name="columns">Columns</param>
//        void FillFromDataSource(System.Data.DataRow row, System.Data.DataColumnCollection columns);
//        #endregion

//        #region Properties
//        /// <summary>
//        /// Object To Construct On Fill
//        /// </summary>
//        System.Type ObjectType
//        {
//            get;
//        }
//        #endregion
//    }
//    /// <summary>
//    /// Combines:
//    ///		Fillable
//    ///		SQL Key
//    ///		INameValue
//    ///	To make the object interact with the dynamic SQL engine
//    /// </summary>
//    /// 
//    //public interface IDatabaseInteroperable : Data.IFillable, Data.ISqlKey, Runtime.INameValue { }
//    #endregion
//}
