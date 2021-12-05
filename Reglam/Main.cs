using System;
using System.Reflection;
using System.Configuration;

namespace Reglam
{
    public static class Main
    {
        /// <summary>
        /// Пауза между вызовами методов
        /// </summary>
        private const int LOOP_PAUSE = 60000;
        #region Параметры
        public enum ReglamStatus { Process, Finised, Aborted }
        /// <summary>
        /// Статус документа
        /// </summary>
        public static ReglamStatus Status { get; private set; }
        public delegate void DelegateOnStatus(ReglamStatus reglamStatus);
        public static event DelegateOnStatus onState;

        /// <summary>
        /// Время запуска регламента
        /// </summary>
        public static DateTime ReglamStarted { get; private set; }
        /// <summary>
        /// Время завершения регламента
        /// </summary>
        public static DateTime ReglamFinished { get; private set; }
        /// <summary>
        /// Признак завершения работы
        /// </summary>
        private static bool isAbort;
        /// <summary>
        /// Признак рабочего времени
        /// </summary>
        private static bool isWorkTime;

        private static TimeSpan PauseDayFrom;
        private static TimeSpan PauseDayFrom_def = TimeSpan.Parse("16:00:00");
        private static TimeSpan PauseDayTo;
        private static TimeSpan PauseDayTo_def = TimeSpan.Parse("23:59:59");
        private static TimeSpan PauseNightFrom;
        private static TimeSpan PauseNightFrom_def = TimeSpan.Parse("00:00:00");
        private static TimeSpan PauseNightTo;
        private static TimeSpan PauseNightTo_def = TimeSpan.Parse("06:00:00");

        private static TimeSpan AllowNightFrom;
        private static TimeSpan AllowNightFrom_def = TimeSpan.Parse("23:00:00");

        /// <summary>
        /// Дневные регламенты
        /// </summary>
        private static TaskJob[] tasksDayTime;
        /// <summary>
        /// Ночные регламенты
        /// </summary>
        private static TaskJob[] tasksNightTime;
        private static string currentReglamName;
        #endregion

        /// <summary>
        /// Инициализация регламента
        /// </summary>
        private static void Init()
        {
            Log.Write($">>> Инициализация регламента Cron {DateTime.Now} (ver.{Assembly.GetExecutingAssembly().GetName().Version.ToString()})");

            try
            {
                var appSettings = ConfigurationManager
                    .OpenExeConfiguration(Assembly.GetExecutingAssembly().Location)
                    .AppSettings;

                PauseDayFrom = TimeSpan.Parse(appSettings.Settings["PauseDayFrom"].Value);
                PauseDayTo = TimeSpan.Parse(appSettings.Settings["PauseDayTo"].Value);
                PauseNightFrom = TimeSpan.Parse(appSettings.Settings["PauseNightFrom"].Value);
                PauseNightTo = TimeSpan.Parse(appSettings.Settings["PauseNightTo"].Value);
                AllowNightFrom = TimeSpan.Parse(appSettings.Settings["AllowNightFrom"].Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex.Message}");
                Console.WriteLine($"Установка значений рабочего времени по умолчанию: {PauseNightTo_def} - {PauseDayFrom_def}");
                PauseDayFrom = PauseDayFrom_def;
                PauseDayTo = PauseDayTo_def;
                PauseNightFrom = PauseNightFrom_def;
                PauseNightTo = PauseNightTo_def;
                AllowNightFrom = AllowNightFrom_def;
            }

            // Дневной регламент
            tasksDayTime = new TaskJob[] {
                new TaskGetPrih(),
                new TaskPrihAnswer(),
                new TaskGetOrder(),
                new TaskOrderAnswer(),
                new TaskVzvr(),
                new TaskOstatki(),
                new TaskAdresDelivery(),
                new TaskRez(),
                new TaskSnRez(),
                new TaskOrderAnswerOpt()
                //new TaskTest()
            };
            Console.WriteLine($"Дневной регламент: {tasksDayTime.Length} задач.");

            // Ночной регламент
            tasksNightTime = new TaskJob[] {
                new TaskOstatki()
                //new TaskTest()
            };
            Console.WriteLine($"Ночной регламент: {tasksNightTime.Length} задач.");

            Console.WriteLine("Инициализация регламента Cron ЗАВЕРШЕНА");
        }

        /// <summary>
        /// Выполнить регламент
        /// </summary>
        public static void Execute()
        {
            Init();
            ExecuteState(true);
            bool isFirstNotWorkTimeJob = true;
            bool isFirstNotWorkTimeMessage = true;
            bool isFirstRun = true;
            while (true)
            {
                if (CheckAbort())
                    break;

                if (IsWorkTime())
                {
                    // РАБОЧЕЕ ВРЕМЯ
                    if (isFirstRun || !isFirstNotWorkTimeJob)
                    {
                        PrintWorkTimeInfo(true);
                        isFirstNotWorkTimeJob = true;
                        isFirstNotWorkTimeMessage = true;
                    }

                    ExecuteReglam(tasksDayTime);
                }
                else
                {
                    // НЕ РАБОЧЕЕ ВРЕМЯ
                    if (isFirstRun || isFirstNotWorkTimeMessage)
                    {
                        PrintWorkTimeInfo(false);
                        isFirstNotWorkTimeMessage = false;
                    }

                    // Работа в вечернее время выполняется строго 1 раз
                    if (isFirstNotWorkTimeJob && DateTime.Now.TimeOfDay >= AllowNightFrom_def)
                    {
                        ExecuteReglam(tasksNightTime);
                        isFirstNotWorkTimeJob = false;
                    }
                }

                if (CheckAbort())
                    break;

                isFirstRun = false;
                Console.WriteLine("Пауза...");
                System.Threading.Thread.Sleep(LOOP_PAUSE);
            }
            ExecuteState(false);
        }

        /// <summary>
        /// Проверка прерывания цикла
        /// </summary>
        private static bool CheckAbort()
        {
            if (isAbort)
            {
                Console.WriteLine("Останавливаю выполнение регламента...");
                ReglamStatus.Aborted.SetStatus();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Выполнить конкретный регламент
        /// </summary>
        /// <param name="tasks">Дневной / Ночной</param>
        private static void ExecuteReglam(TaskJob[] tasks)
        {
            currentReglamName = $"{(isWorkTime ? "Дневной" : "Ночной")} регламент";

            if (isAbort)
            {
                Console.WriteLine($"{currentReglamName} ПРЕРВАН!!!");
                return;
            }

            Console.WriteLine($"Выполняю {currentReglamName}...");
            foreach (var task in tasks)
            {
                task.Execute();

                if (isAbort)
                {
                    Console.WriteLine($"{currentReglamName} ПРЕРВАН!!!");
                    return;
                }
            }
            Console.WriteLine($"{currentReglamName} выполнен!");
        }

        /// <summary>
        /// Пре/пост-процессы
        /// </summary>
        /// <param name="isStart">Признак запуска / остановки регламента</param>
        private static void ExecuteState(bool isStart)
        {
            if (isStart)
            {
                ReglamStarted = DateTime.Now;
                isAbort = false;
                ReglamStatus.Process.SetStatus();
                Console.WriteLine("*** НАЧАТО ВЫПОЛНЕНИЕ РЕГЛАМЕНТА ***");
            }
            else
            {
                ReglamFinished = DateTime.Now;
                tasksDayTime = null;
                tasksNightTime = null;
                GC.Collect();
                ReglamStatus.Finised.SetStatus();
                Console.WriteLine("*** ЗАВЕРШЕНО ВЫПОЛНЕНИЕ РЕГЛАМЕНТА ***");
            }
        }

        /// <summary>
        /// Прервать выполнение регламента
        /// </summary>
        public static void Abort()
        {
            isAbort = true;
            Console.WriteLine("!!! Получена команда завершения работы регламента...");
        }

        /// <summary>
        /// Вычисление рабочего периода
        /// </summary>
        private static bool IsWorkTime()
        {
            TimeSpan timeNow = DateTime.Now.TimeOfDay;
            bool isInPauseDay = timeNow.IsBetween(PauseDayFrom, PauseDayTo);
            bool isInPauseNight = timeNow.IsBetween(PauseNightFrom, PauseNightTo);

            isWorkTime = !(isInPauseDay || isInPauseNight);
            return isWorkTime;
        }

        /// <summary>
        /// Время в диапозоне
        /// </summary>
        private static bool IsBetween(this TimeSpan timeNow, TimeSpan dtFrom, TimeSpan dtTo)
        {
            return timeNow >= dtFrom && timeNow <= dtTo;
        }

        private static void SetStatus(this ReglamStatus reglamStatus)
        {
            onState?.Invoke(reglamStatus);
        }

        /// <summary>
        /// Вывод информации о рабочем периоде
        /// </summary>
        /// <param name="isAllowWork">Дневная работа</param>
        private static void PrintWorkTimeInfo(bool isAllowWork)
        {
            if (isAllowWork)
            {
                Log.Write($"---------------------------------------------");
                Log.Write($">> Работа в промежуток [{PauseNightTo} - {PauseDayFrom}]");
                Log.Write($"---------------------------------------------");
            }
            else
            {
                Log.Write($"---------------------------------------------");
                Log.Write($">> Работа приостановлена в промежуток [{PauseDayFrom} - {PauseNightTo}]");
                Log.Write($">> Некоторые процессы могут быть выполнены и в вечернее время");
                Log.Write($"---------------------------------------------");
            }
        }
    }
}
