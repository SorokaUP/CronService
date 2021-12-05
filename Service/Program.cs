using System.ServiceProcess;

namespace Service
{
    /*
        cd C:\Windows\Microsoft.NET\Framework\v4.0.30319
        Syntax
        InstallUtil.exe + Your copied path + \your service name + .exe
        Our Path
        InstallUtil.exe C:\Users\Faisal-Pathan\source\repos\MyFirstService\MyFirstService\bin\Debug\MyFirstService.exe
    */

    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
