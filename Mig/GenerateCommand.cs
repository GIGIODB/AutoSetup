namespace RestoreDatabase;
public class GenerateCommand{
    public static string pathFile(string nameLogic, string pathFileList, string typeFileList, string pathDataInput, string pathLogInput){
    string readerStrReverse = new string(pathFileList.Reverse().ToArray());
    string fnNameLogic = nameLogic;
    string type = typeFileList;
    string cmdRestore = "";

    //string pathBackupReverse = new string(reader.GetString(1).Reverse().ToArray());
    int posicaoBarra = readerStrReverse.IndexOf('\\');
    readerStrReverse = readerStrReverse.Substring(0,posicaoBarra);
                
    readerStrReverse = new string(readerStrReverse.Reverse().ToArray());                    

        //Altera o path do arquivos fisicos            
        switch (type){
            case "D":                                                   
                cmdRestore = ("            move '"+ fnNameLogic + "' to '"+ pathDataInput+ readerStrReverse + "',");
                //System.Console.WriteLine(cmdRestore);                                                
            break;
            case "L":                                                    
                cmdRestore = ("            move '"+ fnNameLogic + "' to '"+ pathLogInput + readerStrReverse + "'");
                //System.Console.WriteLine(cmdRestore);
                cmdRestore = cmdRestore.TrimEnd(',');                    
            break;                        
        };  
    return cmdRestore;
    }
    public static string setSingleUser(string database){
        string strSingleUser = $@"        
            USE master;
            GO
            ALTER DATABASE [{database}]
            SET SINGLE_USER
            WITH ROLLBACK IMMEDIATE;
            GO
            ";
        return strSingleUser;
    }

    public static string setMultiUser(string database){        
        string strMultiUser = $@"
            USE master; 
            GO
            ALTER DATABASE [{database}]
            SET MULTI_USER;";

        return strMultiUser;
    }

    public static string setCmdFileList(string pathBackup){
        string sqlRestFileList = $"RESTORE FILELISTONLY FROM DISK = '{pathBackup}'";
        return sqlRestFileList;
    }
    public static string setRestoreHeader(string database, string pathBackup){
        string strRestoreHeader = @$"
            RESTORE DATABASE {database}
            from disk='{pathBackup}'
            with file=1,recovery,stats = 5, replace,";

        return strRestoreHeader; 
    }
}
