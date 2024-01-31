using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Windows;

namespace WLO_Translator_WPF
{
    class Database
    {
        private DataTable       mDataTable;
        private DbConnection    mDBConnection;
        private string          mDatabaseName;

        public string DataSource                { get; private set; }
        public string AttachedDBFileName        { get; private set; }
        public string InitialCatalog            { get; private set; }
        public string UserID                    { get; private set; }
        public string Password                  { get; private set; }
        public bool   TrustServerCertificate    { get; private set; }

        public Database(string dataSource, string attachedDBFileName, string initialCatalog,
            string userID, string password, bool trustServerCertificate)
        {
            DataSource              = dataSource;
            AttachedDBFileName      = attachedDBFileName;
            InitialCatalog          = initialCatalog;
            UserID                  = userID;
            Password                = password;
            TrustServerCertificate  = trustServerCertificate;

            mDatabaseName           = initialCatalog + "DB";
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
            MessageBox.Show("AttachedDBFileName: " + AttachedDBFileName);
            SqlClientFactory sQLClient = SqlClientFactory.Instance;
            mDBConnection = sQLClient.CreateConnection();
            mDBConnection.ConnectionString = new SqlConnectionStringBuilder()
            {
                DataSource          = DataSource,//"(LocalDB)\\MSSQLLocalDB",
                AttachDBFilename    = AttachedDBFileName,//"|DataDirectory|\\IO\\DatabaseProgramSettings.mdf",
                //UserID                  = UserID,//"sa", //DESKTOP-9ANKOSD\\ChriW
                //Password                = Password,//"svmp@9108",
                //TrustServerCertificate  = TrustServerCertificate//true
                IntegratedSecurity  = true,
                ConnectTimeout      = 30
            }.ConnectionString;

            mDBConnection.Open();

            //CreateDatabase(mDatabaseName);
            //SaveProgramSettings();
        }

        private void SendCommand(string commandText)
        {
            //DbCommand dbCommand = mDBConnection.CreateCommand();
            //dbCommand.CommandText = commandText;
            //DbDataReader reader = dbCommand.ExecuteReader();
            //    Console.WriteLine("SQL Command Message: " + reader.Read());
            //reader.Close();
            //dbCommand.Dispose();

            using (SqlConnection connection = new SqlConnection(mDBConnection.ConnectionString))
            {
                connection.Open();

                Console.WriteLine("Sql command: " + commandText);

                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            var myString = sqlDataReader.GetString(0); //The 0 stands for "the 0'th column", so the first column of the result.
                                                                       // Do somthing with this rows string, for example to put them in to a list
                            string column = sqlDataReader["ThemeName"].ToString();
                            Console.WriteLine("Sql command output: " + column);
                            Console.WriteLine("Sql command output: " + myString);
                        }
                    }
                }
            }
        }

        private void CreateDatabase(string name)
        {
            SendCommand(@"USE master
            GO

            IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '"+ name + @"')
            BEGIN
                CREATE DATABASE " + name + @";
            END
            GO
                USE " + name + @";
            GO");
        }

        private void CreateTable(string name, string[] tableVariables)
        {
            mDataTable = new DataTable(name);
            
            string variablesWithCommasString = "";
            Array.ForEach(tableVariables, (string variable) =>
            {
                variablesWithCommasString += variable + ",";
                mDataTable.Columns.Add(new DataColumn(variable.Split(' ')[0]));
            });
            //SendCommand(@"USE " + InitialCatalog + @"
            //    CREATE TABLE " + name + @"
            //    (
            //    " + variablesWithCommasString + @"
            //    ); ");

            //USE " + "MSSQLLocalDB" + @"
            SendCommand(@"--You need to check if the table exists

            IF NOT EXISTS(SELECT * FROM sysobjects WHERE name = '" + name + @"' and xtype = 'U')
            BEGIN
                CREATE TABLE " + name + @"(
                    " + variablesWithCommasString + @"
                );
            END");
        }

        private void ViewLastCreatedTableDuringRuntime()
        {
            string columnsWithCommasString = "";
            for (int i = 0; i < mDataTable.Columns.Count; ++i)
                columnsWithCommasString += mDataTable.Columns[i] + ",";

            SendCommand("CREATE OR ALTER VIEW " + mDataTable.TableName + @"View AS
            SELECT * FROM " + mDataTable);
        }

        public void SaveProgramSettings()
        {
            CreateTable("ProgramSettings", new[] { "ThemeName varchar(8000)", "ThemeID int" });
            ViewLastCreatedTableDuringRuntime();
        }
    }
}
