using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace FinanceMovilApp.Helpers
{
    public class Constants
    {
        public const string DatabaseFilename = "FinanceMovil.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode / abrir la base de datos en modo lectura/escritura
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist / crear la base de datos si no existe
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access / habilitar el acceso a la base de datos en modo multi-hilo
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    }
}
