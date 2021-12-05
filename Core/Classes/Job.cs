using System;
using System.Collections.Generic;
using System.IO;

using Cron.Classes.DBF;

namespace Cron.Classes
{
    public class Job
    {
        /// <summary>
        /// Режим работы
        /// </summary>
        public string Mode { get; }

        private Firebird Fb = new Firebird();
        private FtpCron ftp = new FtpCron();
        private Properties.Settings St = Properties.Settings.Default;

        public Job(string Mode)
        {
            this.Mode = Mode;
            if (!Modes.CheckMode(this.Mode))
            {
                Log.Write($"Режим работы (параметр) указан не верно! Получено: \"{this.Mode}\"");
                return;
            }
        }

        /// <summary>
        /// Выполнить
        /// </summary>
        public void Execute()
        {
            #region Подготовка к работе
            // Формируем лог
            DateTime startJob = DateTime.Now;
            Log.Write(String.Format("> Вызов {0} - {1}", Mode, startJob));

            try
            {
                ftp.Connect();
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось подключиться к FTP: {0}", ex.Message));
                throw new Exception(ex.Message);
            }
            #endregion

            dJob action = GetExecuteMethod();
            bool isSuccess = action();

            if (isSuccess)
            {
                DateTime endJob = DateTime.Now;
                Log.Write(String.Format("Выполнено {0}", endJob));            
                Log.Write(String.Format("Итоговое время: {0}", Additional.Helper.TimeFormat(endJob.Subtract(startJob))));
            }
            Log.Write(String.Format("---------------------------------------------"));
            try { ftp.Disconnect(); } catch { }
        }

        private delegate bool dJob();
        private dJob GetExecuteMethod()
        {
            switch (Mode)
            {
                // Забираем с FTP файлы прихода и загружаем в систему
                case Modes.GetPrih:
                    return GetPrih;

                // Формируем DBF файлы ответов на приходы и отправляем на FTP
                case Modes.PrihAnswer:
                    return SendPrihAnswer;

                // Забираем с FTP файлы заказов и загружаем в систему
                case Modes.GetOrder:
                    return GetOrder;

                // Формируем DBF файлы ответов на заказы и отправляем на FTP
                case Modes.OrderAnswer:
                    return SendOrderAnswer;

                // Формируем DBF файлы ответов на оптовые отгрузки и отправляем на FTP
                case Modes.OrderAnswerOpt:
                    return SendOrderAnswerOpt;

                // Формируем файлы возвратов
                case Modes.Vzvr:
                    return SendVzvr;

                // Формируем DBF файл дефектары Котельников
                case Modes.Def:
                    return SendDef;

                // Формируем DBF файл Adres_delivery.dbf и отправляем на FTP
                case Modes.AdresDelivery:
                    return SendAdresDelivery;

                // Забираем с FTP файлы резерва и загружаем в систему
                case Modes.Rez:
                    return GetRezerv;

                // Забираем с FTP файлы снятия резерва и загружаем в систему
                case Modes.SnRez:
                    return GetRezerv;

                // Формируем DBF файлы прайс-листов остатков и отправляем на FTP
                case Modes.Ostatki:
                    return SendOstatki;

                // Формируем MD файл прихода и отправляем на FTP
                case Modes.HandPrihMd:
                    return SendHandPrihMD;

                // Формируем MD файл прихода и отправляем на FTP
                case Modes.HandOrderMd:
                    return SendHandOrderMD;

                default:
                    throw new NotImplementedException($"Режим работы \"{Mode}\" не реализован!");
            }
        }


        //=============================================================================================================
        //=============================================================================================================


        /// <summary>
        /// Забираем с FTP файлы прихода и загружаем в систему
        /// </summary>
        private bool GetPrih()
        {
            try
            {
                Log.Write("*** [GetPrih] Забираем приходы ***");
                List<string> PrihFiles = ftp.GetFromFtp(St.FtpPrihDir, St.LocalPrihDir, St.MaskPrih);
                foreach (string item in PrihFiles)
                {
                    FileInfo finfo = new FileInfo(item);

                    try
                    {
                        Prih pr = new Prih();
                        pr.ReceivedFile = finfo.FullName;
                        pr.ReadDbf();

                        // Обрабатываем
                        Log.Write("> " + finfo.Name);
                        Fb.Load_Process(pr.ReceivedData, finfo.Name, Mode);

                        // Перемещаем выполненный файл в архив
                        ftp.ToPrihArchiv(finfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(String.Format("Не удалось обработать файл {0} по причине: {1}", finfo.Name, ex.Message));
                    }
                }

                if (PrihFiles.Count > 0)
                {
                    // Вызываем пост-обработку после загрузки всех файлов прихода
                    Log.Write("Пост-обработка приходов");
                    Fb.Load_PostProcess(Mode);
                    // Отправляем файлы по почте ответственному лицу
                    Additional.Mail.SendFilesForMail(PrihFiles, "КРОН. Приходы");
                    Log.Write(String.Format("Файлы прихода: {0}", PrihFiles.Count));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Ошибки: ", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Забираем с FTP файлы заказов и загружаем в систему
        /// </summary>
        private bool GetOrder()
        {
            try
            {
                Log.Write("*** [GetOrder] Забираем заказы ***");
                List<string> OrderFiles = ftp.GetFromFtp(St.FtpOrderDir, St.LocalOrderDir, St.MaskOrder);
                foreach (string item in OrderFiles)
                {
                    FileInfo finfo = new FileInfo(item);

                    try
                    {
                        Order ord = new Order();
                        ord.ReceivedFile = finfo.FullName;
                        ord.ReadDbf();

                        // Обрабатываем
                        Log.Write("> " + finfo.Name);
                        Fb.Load_Process(ord.ReceivedData, finfo.Name, Mode);

                        // Перемещаем выполненный файл в архив
                        ftp.ToOrderArchiv(finfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(String.Format("Не удалось обработать файл {0} по причине: {1}", finfo.Name, ex.Message));
                    }
                }

                // Вызываем пост-обработку, даже если заказов небыло. необходимо для тех заказов, которые 
                // имели какую-то ошибку и были оперативно поправлены.
                Log.Write("Пост-обработка заказов");
                Fb.Load_PostProcess(Mode);

                if (OrderFiles.Count == 0)
                    return false;

                // Отправляем файлы по почте ответственному лицу
                Additional.Mail.SendFilesForMail(OrderFiles, "КРОН. Заказы");
                Log.Write(String.Format("Файлы заказов: {0}", OrderFiles.Count));

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Ошибки: ", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Забираем с FTP файлы (снятия)/резерва и загружаем в систему
        /// </summary>
        private bool GetRezerv()
        {
            try
            {
                Log.Write("*** [GetRezerv (" + Mode + ")] Забираем резервы ***");
                List<string> RezFiles = ftp.GetFromFtp(St.FtpRezDir, St.LocalRezDir, (Mode == Modes.Rez) ? St.MaskRez : St.MaskSnRez);
                foreach (string item in RezFiles)
                {
                    Rez rez = null;
                    try
                    {
                        FileInfo finfo = new FileInfo(item);
                        if (Mode == Modes.Rez && finfo.Name.ToLower().Contains(St.MaskSnRez))
                            continue;

                        rez = new Rez
                        {
                            FileName = item,
                            isSn = (Mode == Modes.SnRez),
                            AnswerDir = St.FtpRezAnswerDir,
                            LocalDir = St.LocalRezDir,
                            LocalAnswerDir = St.LocalRezAnswerDir
                        };

                        // Обрабатываем
                        Log.Write("> " + finfo.Name);
                        rez.Execute();

                        // Перемещаем выполненный файл в архив
                        ftp.ToRezArchiv(finfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        rez.ErrorMessage = ex.Message;
                    }                    
                }

                if (RezFiles.Count == 0)
                    return false;

                Log.Write(String.Format("Файлы резервов/снятия: {0}", RezFiles.Count));
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось загрузить и обработать файл резерва по причине: {0}", ex.Message));
                return true;
            }
        }

        /// <summary>
        /// Формируем DBF файлы возвратов и отправляем на FTP
        /// </summary>
        private bool SendVzvr()
        {
            try
            {
                Log.Write("*** [SendVzvr] Формируем возвраты ***");

                List<string> res = Fb.CreateAnswerFiles<Vzvr, VzvrMdlp>(6054, true, false);
                return SendToFtp(res, "Возвраты", ftp.ToVzvr);
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось отправить файлы возвратов по причине: {0}", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Формируем DBF файлы дефектуры и отправляем на FTP
        /// </summary>
        private bool SendDef()
        {
            try
            {
                Log.Write("*** [SendDef] Формируем дефектуру ***");

                List<string> DefFiles = Fb.CreateDefFiles();
                if (DefFiles.Count > 0)
                {
                    Log.Write("Формируем 702");
                    // TODO: Формирование 702
                    Log.Write("Отправляем дефектуру по FTP");
                    string Files = "";
                    foreach (var DefFile in DefFiles)
                    {
                        FileInfo finfo = new FileInfo(DefFile);
                        Log.Write("> " + finfo.Name);
                        Files += ((Files == "") ? "" : ",") + DefFile;
                        // Отправляем на FTP
                        ftp.ToVzvr(DefFile);
                    }

                    // Отправляем файлы по почте ответственному лицу
                    Additional.Mail.Send("КРОН. Дефектура", "Сформирована дефектура: ", Files);
                    Log.Write(String.Format("Файлы дефектуры: {0}", DefFiles.Count));
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось отправить файлы дефектуры по причине: {0}", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Формируем DBF файлы ответов на заказы и отправляем на FTP
        /// </summary>
        private bool SendOrderAnswer()
        {
            try
            {
                Log.Write($"*** [SendOrderAnswer] Формируем ответы на заказы (6051) ***");

                List<string> res = Fb.CreateAnswerFiles<Order, OrderMdlp>(6051, true, true);
                return SendToFtp(res, "Ответы на заказы", ftp.ToOrderAnswer, ftp.ToOrderAnswerMd);
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось отправить файлы ответов на заказы по причине: {0}", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Формируем DBF файлы ответов на оптовые отгрузки и отправляем на FTP
        /// </summary>
        private bool SendOrderAnswerOpt()
        {
            try
            {
                Log.Write($"*** [SendOrderAnswerOpt] Формируем ответы на оптовые отгрузки (6951) ***");

                List<string> res = Fb.CreateAnswerFiles<Order, OrderMdlp>(6951, false, true);
                return SendToFtp(res, "Ответы на оптовые заказы", ftp.ToOrderAnswerOpt);
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось отправить файлы ответов на оптовые заказы по причине: {0}", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Формируем DBF файлы ответов на приходы и отправляем на FTP
        /// </summary>
        private bool SendPrihAnswer()
        {
            try
            {
                Log.Write("*** [SendPrihAnswer] Формируем ответы на приходы (6001) ***");

                List<string> res = Fb.CreateAnswerFiles<Prih, PrihMdlp>(6001, true, true);
                return SendToFtp(res, "Ответы на Приходы", ftp.ToPrihAnswer, ftp.ToPrihAnswerMd);
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось отправить файлы ответов по причине: {0}", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Формируем DBF файлы прайс-листов остатков и отправляем на FTP
        /// </summary>
        private bool SendOstatki()
        {
            try
            {
                Log.Write("*** [SendOstatki] Формируем остатки ***");
                double size = 0;
                string ostatki = "";
                string ostatki_new = "";

                //------------------------------------------------
                try
                {
                    Log.Write("> ostatki.dbf");
                    ostatki = Fb.CreateOstatki(false);
                    Log.Write(ostatki);
                    FileInfo fiOstatki = new FileInfo(ostatki);
                    size = fiOstatki.Length / 1024;
                    Log.Write(String.Format("Размер файла: {0} кб. ({1} мб.)", size, Math.Round(size / 1024, 2)));
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("Ошибка формирования ostatki.dbf: {0}", ex.Message));
                }
                //------------------------------------------------
                try
                {
                    Log.Write("> ostatki_new.dbf");
                    ostatki_new = Fb.CreateOstatki(true);
                    Log.Write(ostatki_new);
                    FileInfo fiOstatki_new = new FileInfo(ostatki_new);
                    size = fiOstatki_new.Length / 1024;
                    Log.Write(String.Format("Размер файла: {0} кб. ({1} мб.)", size, Math.Round(size / 1024, 2)));
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("Ошибка формирования ostatki_new.dbf: {0}", ex.Message));
                }
                //------------------------------------------------

                Log.Write("Отправка остатков на FTP");
                try
                {
                    Log.Write("> ostatki.dbf");
                    ftp.ToPrice(ostatki);
                    Log.Write("> УСПЕШНО");
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("> Ошибка отправки ostatki.dbf на FTP: {0}", ex.Message));
                }
                try
                {
                    Log.Write("> ostatki_new.dbf");
                    ftp.ToPrice(ostatki_new);
                    Log.Write("> УСПЕШНО");
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("> Ошибка отправки ostatki_new.dbf на FTP: {0}", ex.Message));
                }

                // Отправляем файлы по почте ответственному лицу
                //Mail.Send(
                //    "КРОН. Остатки.",
                //    "Сформированы файлы остатков на " + DateTime.Now.ToString("dd.MM.yyyy - HH:mm"),
                //    ostatki_new + "," + ostatki);

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось сформировать и отправить файлы остатков по причине: {0}", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Формируем DBF файл Adres_delivery.dbf и отправляем на FTP
        /// </summary>
        private bool SendAdresDelivery()
        {
            try
            {
                Log.Write("*** [SendAdresDelivery] Формируем адреса доставки ***");
                double size = 0;
                string FileAdresDelivery = "";

                //------------------------------------------------
                try
                {
                    Log.Write("> Adres_delivery.dbf");
                    FileAdresDelivery = Fb.CreateAdresDelivery();
                    Log.Write(FileAdresDelivery);
                    FileInfo fiAdresDelivery = new FileInfo(FileAdresDelivery);
                    size = fiAdresDelivery.Length / 1024;
                    Log.Write(String.Format("Размер файла: {0} кб. ({1} мб.)", size, Math.Round(size / 1024, 2)));
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("Ошибка формирования Adres_delivery.dbf: {0}", ex.Message));
                }
                //------------------------------------------------

                Log.Write("Отправка Adres_delivery.dbf на FTP");
                try
                {
                    Log.Write("> Adres_delivery.dbf");
                    ftp.ToAddress(FileAdresDelivery);
                    Log.Write("> УСПЕШНО");
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("> Ошибка отправки Adres_delivery.dbf на FTP: {0}", ex.Message));
                }

                // Отправляем файлы по почте ответственному лицу
                //Mail.Send(
                //    "КРОН. AdresDelivery.",
                //    "Сформирован файл Adres_delivery.dbf на " + DateTime.Now.ToString("dd.MM.yyyy - HH:mm"),
                //    FileAdresDelivery);

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось сформировать и отправить файл Adres_delivery.dbf по причине: {0}", ex.Message));
                return true;
            }
        }
        

        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------
        // ДЛЯ РУЧНОЙ ВЫГРУЗКИ

        /// <summary>
        /// Формируем MD файл прихода и отправляем на FTP
        /// </summary>
        private bool SendHandPrihMD()
        {
            try
            {
                Console.WriteLine("Введите did для формирования MD файла (можно через запятую): ");
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    throw new Exception("Не передано данных");
                line = line.Replace(';',',');
                foreach (string sdid in line.Split(','))
                {
                    try
                    {
                        int did = int.Parse(sdid.Trim());

                        Log.Write($"*** [SendHandPrihMD] Формируем ответы на приходы MD ***");
                        Log.Write($"did = {did}");

                        List<string> res = Fb.CreateAnswerFilesByDID<Prih, PrihMdlp>(did, 6001, false, true);
                        SendToFtp(res, "Ответы на Приходы МДЛП", ftp.ToPrihAnswerMd);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(String.Format("Не удалось сформировать и отправить файл MD по причине: {0}", ex.Message));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось сформировать и отправить файл MD по причине: {0}", ex.Message));
                return true;
            }
        }
        /// <summary>
        /// Формируем MD файл заказа и отправляем на FTP
        /// </summary>
        private bool SendHandOrderMD()
        {
            try
            {
                Console.WriteLine("Введите did для формирования MD файла (можно через запятую): ");
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    throw new Exception("Не передано данных");
                line = line.Replace(';', ',');
                foreach (string sdid in line.Split(','))
                {
                    try
                    {
                        int did = int.Parse(sdid.Trim());

                        Log.Write("*** [SendHandOrderMD] Формируем ответы на заказы MD ***");
                        Log.Write($"did = {did}");

                        List<string> res = Fb.CreateAnswerFilesByDID<Order, OrderMdlp>(did, 6051, false, true);
                        SendToFtp(res, "Ответы на Заказы МДЛП", ftp.ToOrderAnswerMd);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(String.Format("Не удалось сформировать и отправить файл MD по причине: {0}", ex.Message));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось сформировать и отправить файл MD по причине: {0}", ex.Message));
                return true;
            }
        }


        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ

        /// <summary>
        /// Отправить файлы по FTP
        /// </summary>
        /// <param name="AnswerFiles">Список полных путей к сфоримированным файлам</param>
        /// <param name="caption">Заголовок письма E-Mail</param>
        /// <param name="SendToFolder">Метод отправки в папку на FTP</param>
        /// <param name="SendToMd">(не обязательно) Метод отправки в папку на FTP маркированных данных (по имени MD в файле)</param>
        private bool SendToFtp(List<string> AnswerFiles, string caption, dSendFTP SendToFolder, dSendFTP SendToMd = null)
        {
            if (AnswerFiles.Count > 0)
            {
                Log.Write("Отправляем файлы по FTP");
                string Files = "";
                foreach (var AnswerFile in AnswerFiles)
                {
                    FileInfo finfo = new FileInfo(AnswerFile);
                    Log.Write("> " + finfo.Name);
                    Files += ((Files == "") ? "" : ",") + AnswerFile;
                    // Отправляем на FTP
                    if (finfo.Name.Contains("md") && SendToMd != null)
                    {
                        SendToMd(AnswerFile);
                    }
                    else
                    {
                        SendToFolder(AnswerFile);
                    }
                }

                // Отправляем файлы по почте ответственному лицу
                Additional.Mail.Send($"КРОН. {caption}", $"Сформированы {caption}: ", Files);
                Log.Write(String.Format("Сформировано файлов: {0}", AnswerFiles.Count));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}