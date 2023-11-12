using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

namespace SetupDatabase;

class Program
{
    static void Main()
    {                
        string stringConexao = "Data Source=GIGIOMACHINE;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True";

        int funcionalidade;
        string hostname = Environment.MachineName;

        Console.WriteLine("Qual operação deseja realizar?");
        Console.WriteLine("1 - Check TDE \n" +
                            "2 - Create TDE");

        funcionalidade = int.Parse(Console.ReadLine());

        if (funcionalidade == 1)
        {
            // Crie uma instância da classe TDE
            TDE consultaTDE = new TDE(stringConexao);

            //string queryCheckTde = "declare @hostname varchar(50) = '%MS%' select  name from sys.certificates where name like @hostname";
            string queryCheckTde = "declare @hostname varchar(50) = 'cert_tde_' + @@servername + '%' select name from sys.certificates where name like @hostname";

            // Execute a consulta e obtenha o resultado
            string resultado2 = consultaTDE.CheckTDE(queryCheckTde);

            // Faça algo com o resultado
            Console.WriteLine($" {resultado2}");

            // Aguarde o usuário pressionar Enter antes de fechar o programa
            Console.ReadLine();
        }

        if (funcionalidade == 2)
        {
            Console.WriteLine("Informe o banco de dados");
            string databaseName = Console.ReadLine();

            Console.WriteLine("Deseja gerar uma senha forte? \n1 - Sim | 2 - Não ");
            funcionalidade = int.Parse(Console.ReadLine());

            if (funcionalidade == 1)
            {
                GeradorSenha geradorSenha = new GeradorSenha();
                string senhaGerada = geradorSenha.GerarSenha(32); // Especifica o comprimento desejado

                Console.WriteLine("Senha Gerada: " + senhaGerada);
                string masterKeyPassword = senhaGerada;

                try
                {
                    using (SqlConnection connection = new SqlConnection(stringConexao))
                    {
                        connection.Open();

                        // Criar chave mestra do TDE
                        string createMasterKeyQuery = $"USE master; CREATE MASTER KEY ENCRYPTION BY PASSWORD = '{masterKeyPassword}';" +
                            $"USE {databaseName}; CREATE MASTER KEY ENCRYPTION BY PASSWORD = '{masterKeyPassword}'";
                            
                        using (SqlCommand command = new SqlCommand(createMasterKeyQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        Console.WriteLine($"MasterKey dos bancos master e {databaseName} criadas com sucesso.");

                        string createCertTdeQuery = $@"USE {databaseName}; 
                                                    CREATE CERTIFICATE cert_tde_{hostname} WITH SUBJECT = 'cert_tde_{hostname}'; 
                                                    BACKUP CERTIFICATE cert_tde_{hostname} 
                                                    TO FILE = 'C:\temp\certificadotde\cert_tde_{hostname}.cer'   
                                                    	WITH PRIVATE KEY (
                                                    	    FILE = N'C:\temp\certificadotde\cert_tde_{hostname}.key',   
                                                    	    ENCRYPTION BY PASSWORD = '{masterKeyPassword}'
                                                    		);";                            
                        using (SqlCommand command = new SqlCommand(createCertTdeQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        Console.WriteLine("Certificado do TDE criado com sucesso.");
                        Console.WriteLine($"cert_tde_{hostname}");



                        /*
                        // Habilitar TDE no banco de dados
                        string enableTdeQuery = $"USE master; ALTER DATABASE {databaseName} SET ENCRYPTION ON;";
                        using (SqlCommand command = new SqlCommand(enableTdeQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        */

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ocorreu um erro: {ex.Message}");
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