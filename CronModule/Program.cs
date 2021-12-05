using System;
using Cron.Classes;

namespace CronModule
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string mode = "";
                if (args.Length == 0)
                {
                    Modes.PrintActualModes();
                    mode = Console.ReadLine();
                }
                else
                {
                    mode = args[0];
                }

                new Job(mode).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Modes.PrintActualModes();
            }
            finally
            {
                Console.WriteLine("Для выхода из программы нажмите любую клавишу...");
                Console.ReadKey();
            }
        }
    }
}
