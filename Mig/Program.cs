using System.Data;
using System.Threading.Tasks.Dataflow;
using System.Net.Http;
using System.Dynamic;
using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Collections;
using System.Text;
using RestoreDatabase;
public class Program
{
    private static void Main(string[] args)
    {
        string server, database, pathBackup, pathData, pathLog;

        Console.WriteLine("Nome do server:");
        server = Console.ReadLine();
        // server = "FENIX";

        Console.WriteLine("Nome do banco de dados a ser restaurado:");
        database = Console.ReadLine();
        // database = "alba_erp_head";

        Console.WriteLine("Path do backup:");
        pathBackup = Console.ReadLine();
        // pathBackup = "\\\\venom\\BACKUP_SQL\\FENIX\\SQL2019\\alba_erp_head.bak";

        Console.WriteLine("Path do arquivo de dados:");
        pathData = Console.ReadLine();
        // pathData = "G:\\Data\\"; ;

        Console.WriteLine("Path do arquivo de log:");
        pathLog = Console.ReadLine();
        // pathLog = "H:\\Log\\";

        string sourceConnStr    = Connection.ConnStr(server);

        string cmdSingleUser    = GenerateCommand.setSingleUser(database); 
        string cmdMultiUser     = GenerateCommand.setMultiUser(database);
        string cmdRestoreHeader = GenerateCommand.setRestoreHeader(database, pathBackup);
        string sqlRestFileList  = GenerateCommand.setCmdFileList(pathBackup);

        StringBuilder cmdMoveToRestore = new StringBuilder();
        
        try
        {
            using (SqlConnection sourceConnection = new SqlConnection(sourceConnStr))
            {                
                sourceConnection.Open();
                Messages.msgConn(server);

                //Monta "move to" 
                using (SqlCommand cmdRestFileList = new SqlCommand(sqlRestFileList, sourceConnection))
                {
                    using (SqlDataReader reader = cmdRestFileList.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmdMoveToRestore.AppendLine(GenerateCommand.pathFile(reader.GetString(0), reader.GetString(1), reader.GetString(2), pathData, pathLog));
                        }
                    }
                }
            }

            Messages.msgLineDivisor();
            System.Console.WriteLine(cmdSingleUser + cmdRestoreHeader);
            System.Console.WriteLine(cmdMoveToRestore);
            System.Console.WriteLine(cmdMultiUser);
            Messages.msgLineDivisor();
            Messages.msgExit();
            System.Console.WriteLine("Digite algo para sair...");
            System.Console.ReadLine();
             
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }
}
