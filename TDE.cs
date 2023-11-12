using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SetupDatabase
{
    public class TDE
    {
        private string connectionString;

        public TDE(string connectionString){
            this.connectionString = connectionString;
        }

        // Método para executar a consulta e retornar nome do certificado caso exista
        public string CheckTDE(){
            string resultado = "Não existe certificado TDE ou está fora dos padrões para este servidor.";
            string queryCheckTde = "declare @hostname varchar(50) = 'cert_tde_' + @@servername + '%' select name from sys.certificates where name like @hostname";

            try {
                using (SqlConnection conexao = new SqlConnection(connectionString)){
                    // Abre a conexão com o banco de dados
                    conexao.Open();

                    using (SqlCommand comando = new SqlCommand(queryCheckTde, conexao)){
                        // Executa a consulta e retorna a primeira coluna da primeira linha
                        // Se nenhum resultado for encontrado, retorna 0
                        object resultadoObj = comando.ExecuteScalar();
                        if (resultadoObj != null && resultadoObj != DBNull.Value){
                            resultado = Convert.ToString(resultadoObj);
                        }
                    }
                }
                return "Certificado existente: "+resultado;
            }
            catch (Exception ex){
                // Trate exceções conforme necessário
                resultado = $"Erro ao executar a consulta: {ex.Message}";
                return resultado;
            }            
        }
        public string CreateMasterKey(string stringConexao, string hostname){
            string senhaGerada, masterKeyPassword, result;
            int desvioAcao;
            StringBuilder errorMessages = new StringBuilder();

            Console.WriteLine("Deseja gerar uma senha forte? \n1 - Sim | 2 - Não ");
            desvioAcao = int.Parse(Console.ReadLine());

            if (desvioAcao == 1){
                GeradorSenha geradorSenha = new GeradorSenha();
                senhaGerada = geradorSenha.GerarSenha(32); // Especifica o comprimento desejado

                Console.WriteLine("Senha Gerada: " + senhaGerada);
                masterKeyPassword = senhaGerada;
            }
            else{
                Console.WriteLine("Informe a senha desejada:");
                masterKeyPassword = Console.ReadLine();
            }
                       

            try{
                using (SqlConnection connection = new SqlConnection(stringConexao)){
                    connection.Open();

                    // Criar chave mestra do TDE
                    string createMasterKeyQuery = $"USE master; CREATE MASTER KEY ENCRYPTION BY PASSWORD = '{masterKeyPassword}';";                        

                        using (SqlCommand command = new SqlCommand(createMasterKeyQuery, connection)){
                            command.ExecuteNonQuery();
                        }                    
                }

                Console.WriteLine("MasterKey criada com sucesso no database [master].");
                return masterKeyPassword;                  
            }
            catch (SqlException ex) {

                for (int i = 0; i < ex.Errors.Count; i++) {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                        
                }
                result = errorMessages.ToString();
                Console.WriteLine(errorMessages.ToString());
                return result;
            }                  
        }
        public void CreateCertTDE(string stringConexao, string hostname) {
            string result;
            StringBuilder errorMessages = new StringBuilder();

            try {                
                using (SqlConnection connection = new SqlConnection(stringConexao)){
                    connection.Open();
                    
                    string createCertTdeQuery = $@"USE master; 
                                                CREATE CERTIFICATE cert_tde_{hostname} WITH SUBJECT = 'cert_tde_{hostname}';";
                    using (SqlCommand command = new SqlCommand(createCertTdeQuery, connection))
                    {
                        try{
                            command.ExecuteNonQuery();
                            result = $"Certificado criado com sucesso! cert_tde_{hostname}";
                            Console.WriteLine(result);
                        }
                        catch (SqlException ex){

                            for (int i = 0; i < ex.Errors.Count; i++) {
                                errorMessages.Append("Index #" + i + "\n" +
                                    "Message: " + ex.Errors[i].Message + "\n" +
                                    "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                    "Source: " + ex.Errors[i].Source + "\n");
                            }
                            Console.WriteLine(errorMessages.ToString());
                        }

                    }                   
                }      
            }
            catch (SqlException ex) {

                for (int i = 0; i < ex.Errors.Count; i++) {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
            }
        }
        public void BackupCertTDE(string stringConexao, string hostname, string masterKeyPassword) {
            string result, pathBackupCert;
            StringBuilder errorMessages = new StringBuilder();

            Console.WriteLine("Informe o path que o certificado será exportado:");
            pathBackupCert = Console.ReadLine();
            if (pathBackupCert == null) {
                Console.WriteLine("Obrigatório informar um path.");
                return;
            }
            try {
                using (SqlConnection connection = new SqlConnection(stringConexao)) {
                    connection.Open();

                    string createCertTdeQuery = $@"USE master;                                  
                                                BACKUP CERTIFICATE cert_tde_{hostname} 
                                                TO FILE = '{pathBackupCert}\cert_tde_{hostname}.cer'   
                                                	WITH PRIVATE KEY (
                                                	    FILE = N'{pathBackupCert}\cert_tde_{hostname}.key',   
                                                	    ENCRYPTION BY PASSWORD = '{masterKeyPassword}'
                                                		);";
                    using (SqlCommand command = new SqlCommand(createCertTdeQuery, connection)) {
                        try {
                            command.ExecuteNonQuery();                            
                        }
                        catch (SqlException ex) {

                            for (int i = 0; i < ex.Errors.Count; i++) {
                                errorMessages.Append("Index #" + i + "\n" +
                                    "Message: " + ex.Errors[i].Message + "\n" +
                                    "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                    "Source: " + ex.Errors[i].Source + "\n");
                            }
                            Console.WriteLine(errorMessages.ToString());
                        }
                    }
                }
                result = $"Certificado exportado com sucesso! Verifique em C:\\temp\\certificadotde\\cert_tde_{hostname}";
                Console.WriteLine(result);
            }
            catch (SqlException ex) {

                for (int i = 0; i < ex.Errors.Count; i++) {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
            }
        }
    }
}
