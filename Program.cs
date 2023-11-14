using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace SetupDatabase;

class Program
{
    static void Main()
    {           
        Console.WriteLine("Qual servidor deseja se conectar:"); 
        string hostname = Console.ReadLine();

        string stringConexao = $"Data Source={hostname};Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True";

        int modulo          = 99;
        int funcionalidade  = 99;

         Console.WriteLine("Qual módulo deseja implementar? \n" +
                                "1 - TDE \n" +
                                "2 - Jobs \n\n" +                                                                
                                "0 - Sair"
                                );

        modulo = int.Parse(Console.ReadLine());
        
        if (modulo == 1){        
            while (funcionalidade != 0) {
                Console.Clear();
                Console.WriteLine("Qual operação deseja realizar?");
                Console.WriteLine("1 - Create Master Key \n" +
                                    "2 - Check TDE \n" +
                                    "3 - Create certificate TDE \n" +
                                    "4 - Backup certificate TDE \n" +
                                    "5 - Check CertTDE Databases \n\n" +
                                    "0 - Sair"
                                    );

                funcionalidade = int.Parse(Console.ReadLine());


                if (funcionalidade == 1)
                {
                    TDE createMasterKey = new TDE(stringConexao);
                    string resulCreate = createMasterKey.CreateMasterKey(stringConexao, hostname);

                    Console.ReadLine();
                }
                else if(funcionalidade == 2)
                {
                    // Crie uma instância da classe TDE
                    TDE consultaTDE = new TDE(stringConexao);                       

                    // Execute a consulta e obtenha o resultado
                    string resultado2 = consultaTDE.CheckTDE();

                    // Faça algo com o resultado
                    Console.WriteLine($" {resultado2}");

                    Console.ReadLine();
                }
                else if(funcionalidade == 3)
                {                
                    TDE createCert = new TDE(stringConexao);
                    //string resulCreate = 
                    createCert.CreateCertTDE(stringConexao, hostname);
                    //Console.WriteLine(resulCreate);

                    Console.ReadLine();
                }else if(funcionalidade == 4){
                    string senhaCert;
                    Console.WriteLine("Informe a senha do certificado: \n Mesma senha gerada para Master Key!");
                    senhaCert = Console.ReadLine();

                    TDE backupCert = new TDE(stringConexao);
                    //string resulCreate = 
                    backupCert.BackupCertTDE(stringConexao, hostname, senhaCert);

                }else if(funcionalidade == 5){                                                

                    TDE checkTdeDatabase = new TDE(stringConexao);
                    //string resulCreate = 
                    checkTdeDatabase.CheckTDEDatabase(stringConexao, hostname);

                }
            }
        }
        else if (modulo == 2){
            Console.Clear();

            while (funcionalidade != 0) {
                Console.Clear();
                Console.WriteLine("Qual operação deseja realizar?");
                Console.WriteLine("1 - Create Jobs Update Statistics \n" +
                                    "2 - ** \n" +
                                    "3 - ** \n" +
                                    "4 - ** \n" +
                                    "5 - ** \n\n" +
                                    "0 - Sair"
                                    );
            
                if(funcionalidade == 1){            
                    jobs    createJob = new jobs(stringConexao);
                   // Jobs createJobStats = new Jobs(stringConexao);
                    createJob.CreateJobStats(stringConexao);            
                }
            }
        }
    }
}
/*
 Install data.sqlclient
dotnet add package Microsoft.Data.SqlClient --version 5.1.2

"Data Source=GIGIOMACHINE;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True"
 */

/*
// Habilitar TDE no banco de dados
string enableTdeQuery = $"USE master; ALTER DATABASE {databaseName} SET ENCRYPTION ON;";
using (SqlCommand command = new SqlCommand(enableTdeQuery, connection))
{
    command.ExecuteNonQuery();
}
*/