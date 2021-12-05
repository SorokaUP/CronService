namespace Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.spiCronService = new System.ServiceProcess.ServiceProcessInstaller();
            this.siCronService = new System.ServiceProcess.ServiceInstaller();
            // 
            // spiCronService
            // 
            this.spiCronService.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.spiCronService.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.siCronService});
            this.spiCronService.Password = null;
            this.spiCronService.Username = null;
            // 
            // siCronService
            // 
            this.siCronService.Description = "КРОН. Загрузка приходов. Отправка ответов на приходы. Отправка остатков. Загрузка" +
    " заказов. Отправка ответов на заказы. Отправка Adres_delivery. Отправка возврато" +
    "в. Загрузка резервов и снятие резервов.";
            this.siCronService.DisplayName = "CronService";
            this.siCronService.ServiceName = "CronService";
            this.siCronService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.spiCronService});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller spiCronService;
        private System.ServiceProcess.ServiceInstaller siCronService;
        private System.ServiceProcess.ServiceInstaller siServiceWithoutPrice;
    }
}