using System;
using System.ServiceProcess;
using System.Threading;

namespace Service
{
    partial class Service : ServiceBase
    {
        #region Конструкторы
        public Service()
        {
            InitializeComponent();
        }
        #endregion

        #region Константы
        Thread Worker;
        ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        const int WorkStop = 10000; // 10 сек
        #endregion

        #region Ключевые методы управления службой
        protected override void OnStart(string[] args)
        {
            Log.Write($">>> Запуск службы {DateTime.Now} (ver.{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()})");
            Worker = new Thread(DoWork);
            Worker.Name = "CronService Thread";
            Worker.IsBackground = true;
            Worker.Start();
        }

        protected override void OnStop()
        {
            Log.Write($">>> Остановка службы... {DateTime.Now}");

            ShutdownEvent.Set();
            Reglam.Main.Abort();
            if (!Worker.Join(WorkStop))
                Worker.Abort();

            Log.Write($">>> Служба остановлена {DateTime.Now}");
        }
        #endregion

        /// <summary>
        /// Процесс работы
        /// </summary>
        /// <param name="state"></param>
        private void DoWork(object state)
        {
            Log.Write($">>> Служба запущена {DateTime.Now}");
            Log.Write($"---------------------------------------------");
            Log.Write($">> Ответственное лицо: " + Additional.Mail.GetMailToManager);

            //while (!ShutdownEvent.WaitOne(0))
            {
                Reglam.Main.Execute();
                //Thread.Sleep(LoopPause);
            }

            Log.Write($">>> Прекращение работы {DateTime.Now}");
        }
    }
}
