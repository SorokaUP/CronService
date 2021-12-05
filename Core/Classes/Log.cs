using System;
using System.IO;

public static class Log
{
    /// <summary>
    /// Запись в LOG файл и вывод в консоль
    /// </summary>
    /// <param name="s">Сообщение</param>
    public static void Write(string s, bool onlyLog = false)
    {
        string path = AppDomain.CurrentDomain.BaseDirectory + "Logs";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string filepath = AppDomain.CurrentDomain.BaseDirectory + "Logs\\ServiceLog_" + DateTime.Now.Date.ToString("yyyy.MM.dd").Replace('/', '_') + ".txt";
        if (!File.Exists(filepath))
        {
            // Create a file to write to.   
            using (StreamWriter sw = File.CreateText(filepath))
            {
                sw.WriteLine(s);
            }
        }
        else
        {
            using (StreamWriter sw = File.AppendText(filepath))
            {
                sw.WriteLine(s);
            }
        }
        if (!onlyLog)
            Console.WriteLine(s);
    }
}