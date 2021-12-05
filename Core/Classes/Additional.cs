using System;
using System.IO;

namespace Additional
{
    public static class Mail
    {
        public static void Send(string Theme, string Body, string Files)
        {
            string FilesNameList = "";
            foreach (var item in Files.Split(','))
            {
                FileInfo fi = new FileInfo(item);
                FilesNameList += "\n * " + fi.Name;
            }

            string MailTo = Cron.Properties.Settings.Default.MailTo;
            foreach (string email in MailTo.Split(';'))
            {
                if (string.IsNullOrEmpty(email))
                    continue;

                Log.Write("Отправляем E-mail ответственному лицу: " + email);
                new PmMail.EmailClass().SendFileToEmail(
                    email,
                    Files,
                    Body + FilesNameList,
                    Theme,

                    "profitmed@profitmed.net",
                    -1,
                    false);
            }
        }

        public static string GetMailToManager { get { return Cron.Properties.Settings.Default.MailTo; } }

        /// <summary>
        /// Отправка файлов по почте ответственному лицу
        /// </summary>
        /// <param name="FileList">Список файлов (полный путь)</param>
        /// <param name="Header">Заголовок письма</param>
        public static void SendFilesForMail(System.Collections.Generic.List<string> FileList, string Header)
        {
            if (FileList.Count > 0)
            {
                string Files = "";
                foreach (var File in FileList)
                {
                    FileInfo finfo = new FileInfo(File);
                    Log.Write("> " + finfo.Name, true);
                    Files += ((Files == "") ? "" : ",") + File;
                }
                Mail.Send(Header, "Загружены файлы: ", Files);
            }
        }
    }

    public static class Helper
    {
        public static string GenerateNameMdFile(string ndok, int? idid)
        {
            try
            {
                int? did = 0;
                did = Additional.Helper.TryConvertToInt32(idid, false) - 100000000;
                did = Math.Abs((int)did);
                if (string.IsNullOrEmpty(ndok))
                    ndok = "";
                else
                    ndok = ndok.Substring(ndok.Length > 4 ? ndok.Length - 4 : 0).PadLeft(4, '0');
                return $"md({ndok}){(did).ToString()}";
            }
            catch
            {
                return "_";
            }
        }

        public static int? TryConvertToInt32(object x, bool isReturnNull = true)
        {
            int? res = isReturnNull ? null : (int?)0;
            if (x == System.DBNull.Value)
                return isReturnNull ? null : (int?)0;
            try
            {
                res = System.Convert.ToInt32(x);
            }
            catch
            {
                res = isReturnNull ? null : (int?)0;
            }

            return res;
        }

        public static string TimeFormat(TimeSpan t)
        {
            string res = "";

            res = "";
            if (t.Hours > 0)
                res += ((t.Hours < 10) ? "0" + t.Hours.ToString() : t.Hours.ToString()) + " ч. ";
            if (t.Minutes > 0)
                res += ((t.Minutes < 10) ? "0" + t.Minutes.ToString() : t.Minutes.ToString()) + " мин. ";
            res += ((t.Seconds < 10) ? "0" + t.Seconds.ToString() : t.Seconds.ToString()) + " сек.";

            return res;
        }

        /// <summary>
        /// Возвращает результат между текущим временем и переданным и сравнивает с условием
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="Mode">s = Seconds, m = Minutes, h = Hours</param>
        /// <param name="Value">Значением времени</param>
        /// <returns></returns>
        public static bool ItsTime(this DateTime dt, string Mode, int Value)
        {
            try
            {
                TimeSpan ts = (dt - DateTime.Now);

                switch (Mode.ToLower())
                {
                    case "s":
                        return Math.Abs(ts.Seconds) >= Value;

                    case "m":
                        return Math.Abs(ts.Minutes) >= Value;

                    case "h":
                        return Math.Abs(ts.Hours) >= Value;

                    case "dh":
                        return (DateTime.Now.TimeOfDay.Hours == Value) && (dt.Date != DateTime.Now.Date);

                    default:
                        return false;
                }
            }
            catch { return false; }
        }
    }
}
