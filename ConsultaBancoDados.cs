using Microsoft.Data.SqlClient;

namespace SetupDatabase
{
    public class ConsultaBancoDados
    {
        // String de conexão com o banco de dados
        private string connectionString;

        public ConsultaBancoDados(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Método para executar a consulta e retornar um valor inteiro
        public int ExecutarConsulta(string consultaSql)
        {
            int resultado = 0;

            try
            {
                using (SqlConnection conexao = new SqlConnection(connectionString))
                {
                    // Abre a conexão com o banco de dados
                    conexao.Open();

                    using (SqlCommand comando = new SqlCommand(consultaSql, conexao))
                    {
                        // Executa a consulta e retorna a primeira coluna da primeira linha
                        // Se nenhum resultado for encontrado, retorna 0
                        object resultadoObj = comando.ExecuteScalar();
                        if (resultadoObj != null && resultadoObj != DBNull.Value)
                        {
                            resultado = Convert.ToInt32(resultadoObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Trate exceções conforme necessário
                Console.WriteLine($"Erro ao executar a consulta: {ex.Message}");
            }

            return resultado;
        }
    }
}
