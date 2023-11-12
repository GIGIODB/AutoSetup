using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupDatabase
{
    public class TDE
    {
        private string connectionString;

        public TDE(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Método para executar a consulta e retornar nome do certificado caso exista
        public string CheckTDE(string consultaSql)
        {
            string resultado = "Não existe certificado TDE dentro dos padrões para este servidor;";

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
                            resultado = Convert.ToString(resultadoObj);
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

        public string CreateMasterKey(string consultaSql)
        {
            string resultado = "Não existe certificado TDE dentro dos padrões para este servidor;";

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
                            resultado = Convert.ToString(resultadoObj);
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
