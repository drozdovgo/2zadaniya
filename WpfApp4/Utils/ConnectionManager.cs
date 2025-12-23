using System;
using System.IO;

namespace WpfApp4.Utils
{
    public static class ConnectionManager
    {
        public static string GetConnectionString()
        {
            return $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "medicalclinic.db")}";
        }
    }
}