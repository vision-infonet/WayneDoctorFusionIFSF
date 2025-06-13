namespace Data
{
    #region Enums
    /// <summary>
    /// Database action
    /// </summary>
    internal enum Action : byte
    {
        OpeningConnection = 0,
        OpenedConnection = 1,
        ExecutingQuery = 2,
        ExecutedQuery = 3,
        BeginingTransaction = 4,
        BegunTransaction = 5,
        CommittingTransaction = 6,
        CommittedTransaction = 7,
        RollingBackTransaction = 8,
        RolledBackTransaction = 9,
        ClosingConnection = 10,
        ClosedConnection = 11,
        Disposing = 12,
        Disposed = 13,
        Dispatch = 14,
        ReturnToPool = 15,
        Constructed = 16
    }

    /// <summary>
    /// Default query statement type
    /// </summary>
    internal enum Statements : byte
    {
        SELECT = 0,
        INSERT = 1,
        UPDATE = 2,
        DELETE = 3,
        STORED_PROC = 4,
        FUNCTION = 5,
        SELECT_ALL = 6,
        SELECT_ACTIVE = 7,
        SELECT_BY_REFERENCE = 8,
        SELECT_BY_PARENT_ID = 9,
        SELECT_BY_IAPPLICATION = 10,
        SELECT_BY_IPROJECT = 11,
        SELECT_BY_ICONTACT = 12,
        INSERT_BY_IAPPLICATION = 13,
        SELECT_BY_ILOCATION = 14,
        SELECT_PARENTS = 15,
        UPDATE_PARENT_ID = 16,
        SELECT_BY_IACCOUNT = 17,
        SELECT_BY_TYPE = 18,
        CHECK_REPLICATION_STATUS = 19
    }

    public enum CommandType : byte
    {
        Text = 0,
        StoredProcedure = 1,
        TableDirect = 2,
        TableInsert = 3
    }

    public enum DatabaseActionType : byte
    {
        Insert = 0,
        Delate = 1,
        Update = 2
    }

    /// <summary>
    /// Keys in database access 
    /// </summary>
    public enum ConnectKeys : int
    {
        //ConnectTimeOut = 120
        ConnectTimeOut = 240 //2016-Nov-16 Vision modified
    }
    #endregion
}
