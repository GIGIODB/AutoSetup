using Microsoft.Data.SqlClient;
using System.Numerics;
using System.Text;


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
                return "Certificado: "+resultado;
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
        public string BackupCertTDE(string stringConexao, string hostname, string masterKeyPassword) {
            string result, pathBackupCert;
            StringBuilder errorMessages = new StringBuilder();

            Console.WriteLine("Informe o path que o certificado será exportado:");
            pathBackupCert = Console.ReadLine();
            if (pathBackupCert == null) {
                Console.WriteLine("Obrigatório informar um path.");
                result = "Obrigatório informar um path.";
                return result;
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
                result = $"\n\n Certificado exportado com sucesso! Verifique em {pathBackupCert}\\cert_tde_{hostname}";
                Console.WriteLine(result);
                Console.ReadLine();
                return result;                                
            }
            catch (SqlException ex) {

                for (int i = 0; i < ex.Errors.Count; i++) {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
                result = errorMessages.ToString();
                Console.ReadLine();
                return result;
                
            }
        }
        public void CheckTDEDatabase(string connectionString, string hostname)
        {
            string queryString = 
            @$"SELECT
	            A.[name] as name,
	            isnull(C.name,'No certificate') as certificate,	            
	            isnull(A.is_encrypted,0) as is_encrypted
            FROM sys.databases A
	        LEFT JOIN sys.dm_database_encryption_keys B ON B.database_id = A.database_id
	        LEFT JOIN sys.certificates c on c.thumbprint = b.encryptor_thumbprint
            where A.database_id > 4
            and isnull(C.name,'No certificate') not like 'cert_tde_{hostname}';";

            using (SqlConnection connection = new SqlConnection(connectionString)){
                SqlCommand command =
                    new SqlCommand(queryString, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows){
                    Console.Clear();
                    Console.WriteLine("Databases pendentes:");                    
                        Console.WriteLine("{0,-15} | {1,-20} | {2,-10}", "Database", "Certicate", "is_encrypted");

                    // Obtain a row from the query result.
                    while (reader.Read())
                    {
                        Console.WriteLine("{0,-15} | {1,-20} | {2,-10:C}",
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetBoolean(2)
                            );           
                    }
                }else{
                        Console.WriteLine("No rows found.");
                }
                
                // Call Close when done reading.
                reader.Close();
                Console.ReadLine();
            }
        }
        public string ImportCertTDE(string stringConexao) {
            string result, nameCert, pathImportCer, pathImportKey, masterKeyPassword;
            
            StringBuilder errorMessages = new StringBuilder();

            Console.WriteLine("Informe o do certificado:");
            nameCert = Console.ReadLine();

            Console.WriteLine("Informe o path do arquivo .cer:");
            pathImportCer = Console.ReadLine();

            Console.WriteLine("Informe o path do arquivo .key:");
            pathImportKey = Console.ReadLine();

            Console.WriteLine("Informe a senha do certificado:");
            masterKeyPassword = Console.ReadLine();

            try {
                using (SqlConnection connection = new SqlConnection(stringConexao)) {
                    connection.Open();

                    string createCertTdeQuery = $@"USE master;                                  
                                                CREATE CERTIFICATE {nameCert} 
                                                FROM FILE = '{pathImportCer}'   
                                                	WITH PRIVATE KEY (
                                                	FILE = N'{pathImportKey}',   
                                                	DECRYPTION BY PASSWORD = '{masterKeyPassword}'
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
                result = $"\n\n Certificado importado com sucesso!";
                Console.WriteLine(result);
                Console.ReadLine();
                return result;
            }
            catch (SqlException ex) {

                for (int i = 0; i < ex.Errors.Count; i++) {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
                result = errorMessages.ToString();
                Console.ReadLine();
                return result;
            }
        }
        public string EncryptDatabase(string stringConexao) {
            string result, databaseName, nameCert;

            StringBuilder errorMessages = new StringBuilder();

            Console.WriteLine("Informe o database a ser encryptado:");
            databaseName = Console.ReadLine();

            Console.WriteLine("Informe o do certificado:");
            nameCert = Console.ReadLine();

            try {
                using (SqlConnection connection = new SqlConnection(stringConexao)) {
                    connection.Open();

                    string createCertTdeQuery = $@"USE {databaseName};                                                                                  
                                                
                                    if not exists (	select 1 FROM sys.databases A 
                                        JOIN sys.dm_database_encryption_keys B ON B.database_id = A.database_id
	                                    where a.name = '{databaseName}')
                                    begin
                                                CREATE DATABASE ENCRYPTION KEY
                                                WITH ALGORITHM = AES_256
                                                ENCRYPTION BY SERVER CERTIFICATE {nameCert};
                                                                                               
                                                ALTER DATABASE {databaseName} SET ENCRYPTION ON;
                                    end";
                                    
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

                    TDE checkEncrypt = new TDE(stringConexao);
                    checkEncrypt.CheckEncryptDatabase(stringConexao, databaseName);
                }
                result = $"\n\n Banco de dados Encryptado com sucesso!";
                Console.WriteLine(result);
                Console.ReadLine();
                return result;
            }
            catch (SqlException ex) {

                for (int i = 0; i < ex.Errors.Count; i++) {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
                result = errorMessages.ToString();
                Console.ReadLine();
                return result;
            }
        }
        public string DecryptDatabase(string stringConexao) {
            string result, databaseName;
            char confirm, confirmOk;

            StringBuilder errorMessages = new StringBuilder();

            Console.WriteLine("Informe o database para Decrypt:");
            databaseName = Console.ReadLine();

            Console.WriteLine($"Tem certeza que deseja retirar o TDE do banco de dados: {databaseName}");
            Console.WriteLine("Digite 'S' para prosseguir:");

            confirmOk = 'S';
            confirm = char.Parse(Console.ReadLine());
            bool valida  = confirmOk.Equals( confirm );

            if ( valida == false) {
                Console.WriteLine("Você não confirmou a operação e a aplicação será encerrada!");
                Environment.Exit(0);
            }


            try {
                using (SqlConnection connection = new SqlConnection(stringConexao)) {
                    connection.Open();

                    string decryptedQuery = $@"USE {databaseName};                                                                                  
                                                
                                    if exists (	select 1 FROM sys.databases A 
                                        JOIN sys.dm_database_encryption_keys B ON B.database_id = A.database_id
	                                    where a.name = '{databaseName}' and encryption_state = 3)
                                    begin
                                        ALTER DATABASE {databaseName} SET ENCRYPTION OFF;    	                                
                                    end";

                    using (SqlCommand command = new SqlCommand(decryptedQuery, connection)) {
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

                    TDE checkEncrypt = new TDE(stringConexao);
                    checkEncrypt.CheckEncryptDatabase(stringConexao, databaseName);

                    string dropDecryptedQuery = $@"USE {databaseName};                                                                                  
                                                
                                    if exists (	select 1 FROM sys.databases A 
                                        JOIN sys.dm_database_encryption_keys B ON B.database_id = A.database_id
	                                    where a.name = '{databaseName}' and encryption_state = 1)
                                    begin
                    
                                        DROP DATABASE ENCRYPTION KEY;    	                                
                                    end";

                    using (SqlCommand command = new SqlCommand(dropDecryptedQuery, connection)) {
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
                result = $"\n\n DATABASE ENCRYPTION KEY EXCLUÍDO!";
                Console.WriteLine(result);
                Console.ReadLine();
                return result;
            }
            catch (SqlException ex) {

                for (int i = 0; i < ex.Errors.Count; i++) {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
                result = errorMessages.ToString();
                Console.ReadLine();
                return result;
            }
        }
        public void CheckEncryptDatabase(string connectionString, string databaseName) {

            float percentComplete = 1;

            string queryString =
            @$"	select percent_complete FROM sys.databases A 
                JOIN sys.dm_database_encryption_keys B ON B.database_id = A.database_id
	            where a.name = '{databaseName}';";

            while (percentComplete > 0) { 
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    SqlCommand command =
                        new SqlCommand(queryString, connection);
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows) {
                        Console.Clear();
                        
                        // Obtain a row from the query result.
                        while (reader.Read()) {
                            Console.WriteLine("Status % complete: {0,-15}", reader.GetFloat(0));
                            percentComplete = reader.GetFloat(0);
                        }
                    }
                    else {
                        Console.WriteLine("O banco de dados informado não atende os requisitos para esta operação.");                        
                        Console.ReadLine();
                        Environment.Exit(0);
                    }

                    // Call Close when done reading.
                    reader.Close();                       
                }
            }
            Console.Clear();
            Console.WriteLine("Status % complete: 100%");
            Console.WriteLine($"Database: [{databaseName}] - Processo concluído com sucesso!");
            Console.ReadLine();
        }
    }
}