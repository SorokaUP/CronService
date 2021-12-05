using System;
using System.Text;
using System.Configuration;
using System.Reflection;
using Cron.Classes;
using Additional;

namespace Reglam
{
    //Локальное хранилище настроек
    //C:\Users\Developer00\AppData\Local\Test\Test.exe_Url_lynop0zlx01nfe0c4czofy4qijf4xyzh\1.0.0.0\user.config

    public abstract class TaskJob
    {
        /// <summary>
        /// Состояние "Факт выполнения" (по умолчанию true)
        /// </summary>
        private bool Finished;
        /// <summary>
        /// Дата и время последнего запуска (по умолчанию начало отсчета времени 1869 год)
        /// </summary>
        private DateTime LastRun;
        /// <summary>
        /// Режим работы
        /// </summary>
        private readonly string Mode;
        /// <summary>
        /// Задача активна
        /// </summary>
        private bool isActive;
        /// <summary>
        /// Частота выполнения: d,h,m
        /// </summary>
        private string TimeType;
        /// <summary>
        /// Частота выполнения ЗНАЧЕНИЕ
        /// </summary>
        private int TimeValue;
        /// <summary>
        /// Наименование параметра задачи
        /// </summary>
        private readonly string TaskName;

        //active=1;period=m;value=10;last=10.11.2021 14:49
        private const string PARAM_ACTIVE = "active";
        private const string PARAM_PERIOD = "period";
        private const string PARAM_VALUE = "value";
        private const string PARAM_LAST_RUN = "last";

        public TaskJob(string Mode, string TaskName)
        {
            this.Finished = true;
            this.TaskName = TaskName;
            this.Mode = Mode;

            // Значения по умолчанию
            TimeValue = 1;
            TimeType = "h";
            isActive = false;
            LastRun = DateTime.MinValue;
        }

        /// <summary>
        /// Выполнить
        /// </summary>
        public void Execute()
        {
            LoadSettings();
            if (isExecute())
            {
                try
                {
                    OnExecute(true);
                    new Job(Mode).Execute();
                }
                catch (Exception e)
                {
                    Log.Write("<!> Неожиданное исключение: " + e.Message + Environment.NewLine + e.StackTrace);
                }
                finally
                {
                    OnExecute(false);
                }
            }
        }

        /// <summary>
        /// Начало / завершение работы
        /// </summary>
        /// <param name="isBegin">Признак НАЧАЛА работы</param>
        private void OnExecute(bool isBegin)
        {
            if (isBegin)
            {
                Finished = false;
            }
            else
            {
                Finished = true;
                LastRun = DateTime.Now;
                SaveSettings();
            }
        }

        /// <summary>
        /// Разрешение на выполнение задания
        /// </summary>
        public virtual bool isExecute()
        {
            return (isActive) && Finished && (LastRun.ItsTime(TimeType, TimeValue));
        }

        /// <summary>
        /// Загрузить настройки
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // Параметры
                var taskConfig = ConfigurationManager
                    .OpenExeConfiguration(Assembly.GetExecutingAssembly().Location)
                    .AppSettings
                    .Settings[TaskName].Value;

                foreach (var param in taskConfig.Split(';'))
                {
                    try
                    {
                        if (param == "")
                            break;

                        string[] paramInfo = param.Split('=');
                        string paramName = paramInfo[0];
                        string paramValue = paramInfo[1];

                        switch (paramName)
                        {
                            case PARAM_ACTIVE:
                                isActive = paramValue == "1";
                                break;

                            case PARAM_PERIOD:
                                TimeType = paramValue;
                                break;

                            case PARAM_VALUE:
                                TimeValue = Int32.Parse(paramValue);
                                break;

                            case PARAM_LAST_RUN:
                                LastRun = DateTime.Parse(paramValue);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Сохранить настройки
        /// </summary>
        private void SaveSettings()
        {
            StringBuilder paramInfo = new StringBuilder();
            paramInfo.Append($"{PARAM_ACTIVE}={(isActive ? '1' : '0')};");
            paramInfo.Append($"{PARAM_PERIOD}={TimeType};");
            paramInfo.Append($"{PARAM_VALUE}={TimeValue};");
            paramInfo.Append($"{PARAM_LAST_RUN}={LastRun.ToString("dd.MM.yyyy HH:mm")};");

            var appConfig = ConfigurationManager
                .OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            appConfig.AppSettings.Settings[TaskName].Value = paramInfo.ToString();
            appConfig.Save();
        }
    }

    //------------------------------------------------------------

    public class TaskGetPrih : TaskJob
    {
        public TaskGetPrih() : base(Modes.GetPrih, "TaskGetPrih") { }
    }
    public class TaskPrihAnswer : TaskJob
    {
        public TaskPrihAnswer() : base(Modes.PrihAnswer, "TaskPrihAnswer") { }
    }
    public class TaskGetOrder : TaskJob
    {
        public TaskGetOrder() : base(Modes.GetOrder, "TaskGetOrder") { }
    }
    public class TaskOrderAnswer : TaskJob
    {
        public TaskOrderAnswer() : base(Modes.OrderAnswer, "TaskOrderAnswer") { }
    }

    //------------------------------------------------------------

    public class TaskVzvr : TaskJob
    {
        public TaskVzvr() : base(Modes.Vzvr, "TaskVzvr") { }
    }
    public class TaskOstatki : TaskJob
    {
        public TaskOstatki() : base(Modes.Ostatki, "TaskOstatki") { }
    }
    public class TaskAdresDelivery : TaskJob
    {
        public TaskAdresDelivery() : base(Modes.AdresDelivery, "TaskAdresDelivery") { }
    }
    public class TaskRez : TaskJob
    {
        public TaskRez() : base(Modes.Rez, "TaskRez") { }
    }
    public class TaskSnRez : TaskJob
    {
        public TaskSnRez() : base(Modes.SnRez, "TaskSnRez") { }
    }
    public class TaskTest : TaskJob
    {
        public TaskTest() : base(Modes.Test, "TaskTest") { }
    }
    public class TaskOrderAnswerOpt : TaskJob
    {
        public TaskOrderAnswerOpt() : base(Modes.OrderAnswerOpt, "TaskOrderAnswerOpt") { }
    }
}