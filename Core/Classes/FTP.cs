using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using FluentFTP;

namespace Cron.Classes
{
    public delegate void dSendFTP(string FullFileName);

    public class FtpCron : FtpClient
    {
        public FtpCron()
        {
            try
            {
                // Читаем и устанавливаем папку для складывания приходов
                var St = Properties.Settings.Default;

                Host = St.ftp_host;
                Port = St.ftp_port;
                DataConnectionType = FtpDataConnectionType.PASV;
                Credentials = new NetworkCredential(St.ftp_usr, St.ftp_psw);
            }
            catch (Exception ex)
            {
                string err = String.Format("(!!!) Не удалось создать подключение к FTP по причине: {0}", ex.Message);
                Log.Write(err);
                throw new Exception(err);
            }
        }

        //-------------------------------------------------------------------------------

        /// <summary>
        /// Отправить файл ПРИХОДА в АРХИВ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToPrihArchiv(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл прихода (возможно был удален).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpPrihArchivDir + finfo.Name);
            DeleteFile(Properties.Settings.Default.FtpPrihDir + finfo.Name);
        }

        /// <summary>
        /// Отправить файл ПРИХОДА в ОТВЕТЫ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToPrihAnswer(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл ответа (возможно не удалось сформировать).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpPrihAnswerDir + finfo.Name);
        }

        /// <summary>
        /// Отправить файл ПРИХОДА в ОТВЕТЫ MD
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToPrihAnswerMd(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл ответа (возможно не удалось сформировать).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpPrihAnswerMdDir + finfo.Name);
        }

        /// <summary>
        /// Отправить файл РЕЗЕРВА в ОТВЕТЫ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToRezAnswer(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл ответа на резерв (возможно не удалось сформировать).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpRezAnswerDir + finfo.Name);            
        }

        /// <summary>
        /// Отправить файл РЕЗЕРВА в АРХИВ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToRezArchiv(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл ответа на резерв (возможно не удалось сформировать).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpRezArchivDir + finfo.Name);
            DeleteFile(Properties.Settings.Default.FtpRezDir + finfo.Name);
        }

        /// <summary>
        /// Отправить файл в ПРАЙСЫ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToPrice(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                string err = "Не найден файл (возможно не удалось сформировать).";
                throw new Exception(err);
            }
            FileInfo finfo = new FileInfo(FullFileName);
            string tmpFileName = finfo.Name + "_tmp";
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpPriceDir + tmpFileName);
            try
            {
                DeleteFile(Properties.Settings.Default.FtpPriceDir + finfo.Name);
            }
            catch { }
            Rename(Properties.Settings.Default.FtpPriceDir + tmpFileName, Properties.Settings.Default.FtpPriceDir + finfo.Name);

            // ------------------------------------------------------------
            // После отработки - пакуем файл в архив

            if (File.Exists(FullFileName))
                try
                {
                    string newFullFileName =
                        finfo.DirectoryName + "\\" +
                        Path.GetFileNameWithoutExtension(FullFileName) +
                        "__" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm") +
                        Path.GetExtension(FullFileName);
                    File.Move(FullFileName, newFullFileName);
                    Log.Write(String.Format("Файл успешно архивирован: {0}", newFullFileName));
                    if (File.Exists(FullFileName))
                        File.Delete(FullFileName);
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("Ошибка при работе с локальным файлом остатков: {0}", ex.Message));
                }
        }

        /// <summary>
        /// Отправить файл в ПРАЙСЫ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToAddress(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                string err = "Не найден файл (возможно не удалось сформировать).";
                throw new Exception(err);
            }
            FileInfo finfo = new FileInfo(FullFileName);
            string tmpFileName = finfo.Name + "_tmp";
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpAdressDir + tmpFileName);
            try
            {
                DeleteFile(Properties.Settings.Default.FtpAdressDir + finfo.Name);
            }
            catch { }
            Rename(Properties.Settings.Default.FtpAdressDir + tmpFileName, Properties.Settings.Default.FtpAdressDir + finfo.Name);

            // ------------------------------------------------------------
            // После отработки - пакуем файл в архив

            if (File.Exists(FullFileName))
                try
                {
                    string newFullFileName =
                        finfo.DirectoryName + "\\" +
                        Path.GetFileNameWithoutExtension(FullFileName) +
                        "__" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm") +
                        Path.GetExtension(FullFileName);
                    File.Move(FullFileName, newFullFileName);
                    Log.Write(String.Format("Файл успешно архивирован: {0}", newFullFileName));
                    if (File.Exists(FullFileName))
                        File.Delete(FullFileName);
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("Ошибка при работе с локальным файлом адресов: {0}", ex.Message));
                }
        }

        /// <summary>
        /// Отправить файл ЗАКАЗА в АРХИВ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToOrderArchiv(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл заказа (возможно был удален).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpOrderArchivDir + finfo.Name);
            DeleteFile(Properties.Settings.Default.FtpOrderDir + finfo.Name);
        }

        /// <summary>
        /// Отправить файл ЗАКАЗА в ОТВЕТЫ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToOrderAnswer(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл заказа (возможно не удалось сформировать).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpOrderAnswerDir + finfo.Name);
        }

        /// <summary>
        /// Отправить файл ОПТОВОГО ЗАКАЗА в ОТВЕТЫ
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToOrderAnswerOpt(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл заказа (возможно не удалось сформировать).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpOrderAnswerOptDir + finfo.Name);
        }

        /// <summary>
        /// Отправить файл ОТВЕТА НА ЗАКАЗ в ОТВЕТЫ MD
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToOrderAnswerMd(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл ответа (возможно не удалось сформировать).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpOrderAnswerMdDir + finfo.Name);
        }

        /// <summary>
        /// Отправить файл ВОЗВРАТА
        /// </summary>
        /// <param name="FullFileName"></param>
        public void ToVzvr(string FullFileName)
        {
            if (!File.Exists(FullFileName))
            {
                Log.Write("Не найден файл возврата (возможно не удалось сформировать).");
                return;
            }
            FileInfo finfo = new FileInfo(FullFileName);
            UploadFile(finfo.FullName, Properties.Settings.Default.FtpVzvrDir + finfo.Name);
        }
    }

    public static class FtpCronExtensions
    {
        /// <summary>
        /// Получаем актуальные файлы с FTP по маске имени файла
        /// </summary>
        /// <param name="DirFtp">Папка на FTP из которой нужно забрать</param>
        /// <param name="DirLocal">Папка на ПК в которую нужно сохранить</param>
        /// <param name="Mask">Маска поиска файлов</param>
        /// <returns>Ссылки на скачанные файлы в указанной локальной директории</returns>
        public static List<string> GetFromFtp(this FtpCron ftp, string DirFtp, string DirLocal, string Mask)
        {
            List<string> res = new List<string>();

            // Проверяем / создаем локальную директорию
            if (!Directory.Exists(DirLocal))
                Directory.CreateDirectory(DirLocal);

            ftp.SetWorkingDirectory(DirFtp);
            FtpListItem[] items = ftp.GetListing(DirFtp);
            foreach (FtpListItem item in items)
            {
                if (item.Type != FtpFileSystemObjectType.File)
                    continue;
                string itemLowered = item.Name.ToLower();
                if (!(itemLowered.Replace(Mask.ToLower(), "").Length != itemLowered.Length))
                    continue;

                // Получаем файл с сервера
                ftp.DownloadFile(DirLocal + "\\" + item.Name, item.FullName, FtpLocalExists.Overwrite, FtpVerify.None);
                res.Add(DirLocal + "\\" + item.Name);
            }

            return res;
        }
    }
}
