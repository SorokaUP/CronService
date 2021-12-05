using System;
using System.Reflection;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Collections.Generic;
using System.IO;

using DotNetDBF;
using Extensions;
using FirebirdSql.Data.FirebirdClient;

namespace Cron.Classes.DBF
{
    public interface IDbfStructItem
    {
        DBFField[] GetDBFFields();
        void Mapping(FbDataReader dr_did);
    }
    public interface IDbfStruct
    {
        bool AddItem(FbDataReader dr);
        bool CreateDbf();
        string GetAnswerFile();
    }

    /// <summary>
    /// Структура DBF
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DbfStruct<T> : IDbfStruct
        where T : IDbfStructItem, new()
    {
        /// <summary>
        /// Полный путь к файлу скаченного с FTP
        /// </summary>
        public string ReceivedFile { get; set; }
        /// <summary>
        /// Данные из скаченного файла с FTP
        /// </summary>
        public DataTable ReceivedData { get; set; }

        /// <summary>
        /// Полный путь к файлу ответа
        /// </summary>
        private string AnswerFile { get; set; }
        /// <summary>
        /// Строки файла ответа
        /// </summary>
        public List<T> AnswerItems { get; set; }

        public DbfStruct()
        {
            AnswerItems = new List<T>();
        }

        /// <summary>
        /// Добавить строку данных
        /// </summary>
        /// <param name="dr"></param>
        public bool AddItem(FbDataReader dr)
        {
            var item = new T();
            item.Mapping(dr);
            AnswerItems.Add(item);

            return true;
        }

        /// <summary>
        /// Возвращает полный путь к сформированному DBF файлу
        /// </summary>
        /// <returns></returns>
        public string GetAnswerFile()
        {
            return AnswerFile;
        }

        /// <summary>
        /// Наименование файла ответа без расширения .DBF
        /// </summary>
        /// <returns></returns>
        protected abstract string GetAnswerFileName();
        /// <summary>
        /// Полный путь к файлу ответа на ПК
        /// </summary>
        /// <returns></returns>
        protected abstract string GetAnswerLocalDir();

        /// <summary>
        /// Создать DBF
        /// </summary>
        public bool CreateDbf()
        {
            Type TFields = typeof(T);
            // Данные заполнены
            if (AnswerItems.Count == 0)
                return false;
            // Данные хранятся в ПОЛЯХ класса
            if (TFields.GetFields().Length == 0)
                return false;

            string dir = GetAnswerLocalDir().FixFilePath();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            AnswerFile = dir + $"\\{GetAnswerFileName()}.dbf";
            AnswerFile = AnswerFile.FixFilePath();

            if (File.Exists(AnswerFile))
                try { File.Delete(AnswerFile); } catch { }

            using (Stream fos = File.Open(AnswerFile, FileMode.Create, FileAccess.ReadWrite))
            using (var dbf = new DBFWriter())
            {
                dbf.CharEncoding = Encoding.GetEncoding(866);
                dbf.Signature = 3;         // DBFSigniture.DBase3;
                dbf.LanguageDriver = 0x26; // кодировка 866
                                           // Колонки СТРОГО КАПСОМ, требование КРОНА

                dbf.Fields = AnswerItems[0].GetDBFFields();
                foreach (IDbfStructItem item in AnswerItems)
                {
                    object[] data = new object[dbf.Fields.Length];
                    for (int i = 0; i < dbf.Fields.Length; i++)
                    {
                        data[i] = TFields
                            .GetField(dbf.Fields[i].Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                            .GetValue(item);
                    }
                    dbf.AddRecord(data);
                }
                dbf.Write(fos);
            }
            return true;
        }
        /// <summary>
        /// Считать данные из DBF
        /// </summary>
        public bool ReadDbf()
        {
            try
            {
                ReceivedData = ReadDataTable(ReceivedFile);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("Ошибка чтения файла " + ReceivedFile + " :: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Чтение данных
        /// </summary>
        /// <param name="LocalFile">Полный путь к файлу</param>
        /// <returns>Таблица данных</returns>
        protected static DataTable ReadDataTable(string LocalFile)
        {
            DataTable data = new DataTable();

            //--------------------------------------------------------------------------------------
            // Открываем и читаем файл

            // Файлы, длина имени которых более 8 символов (без расширения), необходимо переименовывать!
            FileInfo fi = new FileInfo(LocalFile.FixFilePath());
            OleDbConnection con = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Extended Properties=dBase III;Data Source=" + fi.DirectoryName + "\\");
            con.Open();

            if (con.State == ConnectionState.Open)
            {
                // Выполняем переименование файла
                string work = "work.dbf";
                string FileNameRename = fi.DirectoryName + "\\" + work;
                bool needWork = (fi.Name.Replace(fi.Extension, "").Length > 8);

                OleDbCommand query = new OleDbCommand("select * from [" + (needWork ? work : fi.Name) + "]", con);
                OleDbDataAdapter DA = new OleDbDataAdapter(query);

                // Выполняем чтение
                try
                {
                    if (needWork)
                    {
                        if (File.Exists(FileNameRename))
                            File.Delete(FileNameRename);
                        File.Move(LocalFile, FileNameRename);
                    }

                    DA.Fill(data);
                }
                catch (Exception ex)
                {
                    Log.Write(ex.Message);
                }

                // Возвращаем файлу прежнее имя
                if (needWork)
                    File.Move(FileNameRename, LocalFile);

                con.Close();
            }

            return data;
        }
    }
}
