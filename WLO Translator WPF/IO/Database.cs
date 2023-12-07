using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Windows;

namespace WLO_Translator_WPF
{
    class Database
    {
        DbConnection mDBConnection;

        public string DataSourcePath            { get; private set; }
        public string InitialCatalog            { get; private set; }
        public string UserID                    { get; private set; }
        public string Password                  { get; private set; }
        public bool   TrustServerCertificate    { get; private set; }
        public string Name                      { get; private set; }

        public Database(string name, string dataSourcePath, string initialCatalog, string userID, string password, bool trustServerCertificate)
        {
            Name                    = name;
            DataSourcePath          = dataSourcePath;
            InitialCatalog          = initialCatalog;
            UserID                  = userID;
            Password                = password;
            TrustServerCertificate  = trustServerCertificate;
        }

        //public void Create()
        //{
        //    string cmdText;
        //    SqlConnection myConnection = new SqlConnection("Server=localhost;Integrated security=SSPI;database=master");

        //    cmdText = "CREATE DATABASE " + Name + " ON PRIMARY " +
        //     "(NAME = " + Name + "_Data, " +
        //     "FILENAME = '" + DataSourcePath + "\\" + Name +".mdf', " +
        //     "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%)" +
        //     "LOG ON (NAME = MyDatabase_Log, " +
        //     "FILENAME = '" + DataSourcePath + "\\" + Name + "Log.ldf', " +
        //     "SIZE = 1MB, " +
        //     "MAXSIZE = 5MB, " +
        //     "FILEGROWTH = 10%)";

        //    SqlCommand myCommand = new SqlCommand(cmdText, myConnection);
        //    try
        //    {
        //        myConnection.Open();
        //        myCommand.ExecuteNonQuery();
        //        MessageBox.Show("DataBase is Created Successfully", "DataBase Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString(), "DataBase Error", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    finally
        //    {
        //        if (myConnection.State == ConnectionState.Open)
        //        {
        //            myConnection.Close();
        //        }
        //    }
        //}

        public void Connect()
        {
            SqlClientFactory sQLClient = SqlClientFactory.Instance;
            mDBConnection = sQLClient.CreateConnection();
            mDBConnection.ConnectionString = new SqlConnectionStringBuilder()
            {
                DataSource              = DataSourcePath + "\\" + Name + ".mdf",//"DESKTOP-9ANKOSD\\CHRISSQLSERVER", // Server name
                InitialCatalog          = Name,//"SQLTestDatabase",
                //UserID                  = UserID,//"sa", //DESKTOP-9ANKOSD\\ChriW
                //Password                = Password,//"svmp@9108",
                //TrustServerCertificate  = TrustServerCertificate//true
            }.ConnectionString;

            mDBConnection.Open();
        }

        private void SendCommand(string commandText)
        {
            DbCommand dbCommand = mDBConnection.CreateCommand();
            dbCommand.CommandText = @"USE SQLTestDatabase
                CREATE TABLE TestTable
                (
                ID int,
                TableName varchar(8000),
                BigNumber bigint,
                ); ";
            DbDataReader reader = dbCommand.ExecuteReader();
            Console.WriteLine(reader.Read());
        }

        public void CreateTable(string name, string tableVariables)
        {
            SendCommand(@"USE " + InitialCatalog + @"
                CREATE TABLE " + name + @"
                (
                " + tableVariables + @"
                ); ");
        }
    }
}
