using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace SetupDatabase;

class Program
{
    static void Main()
    {                
        string stringConexao = "Data Source=GIGIOMACHINE;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True";

        int funcionalidade = 2;
        string hostname = Environment.MachineName;        

        while (funcionalidade != 0) {
            Console.Clear();
            Console.WriteLine("Qual operação deseja realizar?");
            Console.WriteLine("1 - Create Master Key \n" +
                                "2 - Check TDE \n" +
                                "3 - Create certificate TDE \n" +
                                "4 - Backup certificate TDE \n\n" +
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