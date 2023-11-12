using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupDatabase
{
    internal class Connection
    {
        public static string ConnStr(string server)
        {
            string Str = $"Data Source={server};Initial Catalog=master;Integrated Security=True;";
            return Str;
        }
    }
}
