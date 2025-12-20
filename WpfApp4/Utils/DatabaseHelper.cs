using System;
using System.IO;
using System.Windows;

namespace WpfApp4.Utils
{
    public static class DatabaseHelper
    {
        private static string _databasePath;
        private static string _databaseFolder;

        static DatabaseHelper()
        {
            // Определяем папку для базы данных
            _databaseFolder = GetDatabaseFolder();
            _databasePath = Path.Combine(_databaseFolder, "medicalclinic.db");

            System.Diagnostics.Debug.WriteLine($"✅ DatabaseHelper инициализирован");
            System.Diagnostics.Debug.WriteLine($"Папка для базы данных: {_databaseFolder}");
            System.Diagnostics.Debug.WriteLine($"Полный путь к файлу: {_databasePath}");

            // Создаем папку, если она не существует
            EnsureDatabaseFolderExists();
        }

        private static string GetDatabaseFolder()
        {
            // Создаем специальную папку "Database" в папке приложения
            string appFolder = AppDomain.CurrentDomain.BaseDirectory;
            string databaseFolder = Path.Combine(appFolder, "Database");

            return databaseFolder;
        }

        private static void EnsureDatabaseFolderExists()
        {
            try
            {
                if (!Directory.Exists(_databaseFolder))
                {
                    Directory.CreateDirectory(_databaseFolder);
                    System.Diagnostics.Debug.WriteLine($"✅ Создана папка для базы данных: {_databaseFolder}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания папки для базы данных: {ex.Message}");
                // В случае ошибки используем папку приложения
                _databaseFolder = AppDomain.CurrentDomain.BaseDirectory;
                _databasePath = Path.Combine(_databaseFolder, "medicalclinic.db");
            }
        }

        public static string GetConnectionString()
        {
            string connectionString = $"Data Source={_databasePath}";
            System.Diagnostics.Debug.WriteLine($"✅ Строка подключения: {connectionString}");
            return connectionString;
        }

        public static string GetDatabasePath()
        {
            return _databasePath;
        }

        public static string GetDatabaseFolderPath()
        {
            return _databaseFolder;
        }

        public static bool DatabaseExists()
        {
            bool exists = File.Exists(_databasePath);
            System.Diagnostics.Debug.WriteLine($"База данных существует: {exists}");
            return exists;
        }

        public static long GetDatabaseSize()
        {
            if (DatabaseExists())
            {
                return new FileInfo(_databasePath).Length;
            }
            return 0;
        }

        public static void PrintDatabaseInfo()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== ИНФОРМАЦИЯ О БАЗЕ ДАННЫХ ===");
                System.Diagnostics.Debug.WriteLine($"Папка: {_databaseFolder}");
                System.Diagnostics.Debug.WriteLine($"Файл: {Path.GetFileName(_databasePath)}");
                System.Diagnostics.Debug.WriteLine($"Полный путь: {_databasePath}");
                System.Diagnostics.Debug.WriteLine($"Существует: {DatabaseExists()}");

                if (DatabaseExists())
                {
                    var fileInfo = new FileInfo(_databasePath);
                    System.Diagnostics.Debug.WriteLine($"Размер: {FormatFileSize(fileInfo.Length)}");
                    System.Diagnostics.Debug.WriteLine($"Создан: {fileInfo.CreationTime}");
                    System.Diagnostics.Debug.WriteLine($"Изменен: {fileInfo.LastWriteTime}");

                    // Проверяем доступность файла
                    try
                    {
                        using (var stream = File.Open(_databasePath, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            System.Diagnostics.Debug.WriteLine($"Статус: ✅ Файл доступен для чтения/записи");
                        }
                    }
                    catch (IOException)
                    {
                        System.Diagnostics.Debug.WriteLine($"Статус: ⚠️ Файл заблокирован другим процессом");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Статус: ❌ Файл не найден");
                    System.Diagnostics.Debug.WriteLine($"Создан ли каталог: {Directory.Exists(_databaseFolder)}");
                }

                // Информация о каталоге
                if (Directory.Exists(_databaseFolder))
                {
                    var files = Directory.GetFiles(_databaseFolder);
                    System.Diagnostics.Debug.WriteLine($"Файлов в папке Database: {files.Length}");
                    foreach (var file in files)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {Path.GetFileName(file)}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения информации о файле: {ex.Message}");
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static void OpenDatabaseFolder()
        {
            try
            {
                if (Directory.Exists(_databaseFolder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", _databaseFolder);
                    System.Diagnostics.Debug.WriteLine($"✅ Открыта папка с базой данных: {_databaseFolder}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Папка не существует: {_databaseFolder}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка открытия папки: {ex.Message}");
            }
        }

        public static void BackupDatabase(string backupName = null)
        {
            try
            {
                if (!DatabaseExists())
                {
                    System.Diagnostics.Debug.WriteLine("❌ Нечего резервировать - база данных не существует");
                    return;
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = backupName ?? $"backup_{timestamp}.db";
                string backupPath = Path.Combine(_databaseFolder, backupFileName);

                File.Copy(_databasePath, backupPath, true);
                System.Diagnostics.Debug.WriteLine($"✅ Создана резервная копия: {backupPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания резервной копии: {ex.Message}");
            }
        }
    }
}