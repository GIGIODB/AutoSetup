namespace RestoreDatabase
{
    public class Messages
    {
        public static string msgConn(string server){
            string msg = $"\n\n Conexão realizada com sucesso no server: {server} \n\n";
            System.Console.WriteLine(msg);
            return msg;
        }        
        public static string msgLineDivisor(){
            string msg = "\n\n ************************** Comando do restore ************************** \n\n ";
            System.Console.WriteLine(msg);
            return msg;
        }
        public static string msgExit(){
            string msg = "Pressiona qualquer tecla para sair...";
            System.Console.WriteLine(msg);
            return msg;
        }
    }
}