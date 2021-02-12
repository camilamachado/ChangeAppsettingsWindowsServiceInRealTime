using ChangeAppsettingsInRealTime.CommandLines;
using ChangeAppsettingsInRealTime.Services;
using CommandLine;
using Serilog;
using System;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

namespace ChangeAppsettingsInRealTime
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var appsettingsService = new AppsettingsService();

            // Configurando log
            Log.Logger = new LoggerConfiguration()
                             .MinimumLevel.Debug()
                             .WriteTo.File($"{AppContext.BaseDirectory}\\log.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                             .CreateLogger();

            // Inicializando CommandLine
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       for (var i = 0; i < o.Repeat; i++)
                       {
                           Console.WriteLine($"Hello {o.Person}!");
                       }

                       if (o.Save)
                       {
                           appsettingsService.Write("Config:Repeat", o.Repeat);
                           appsettingsService.Write("Config:Person", o.Person);
                           appsettingsService.Write("Config:Save", o.Save);

                           Console.WriteLine($"Dados salvos com sucesso!");
                       }
                   });


            return (int)HostFactory.Run(configurator =>
            {
                //Aqui foi necessário a sobrecarga dos comandos
                //pois sem essa configuração o Topshelf não reconhece os comandos
                //e apresenta erro no console, não deve ser a melhor forma, mas como o objetivo do estudo
                //é apenas gravar informações no appsettings, essa parte é apenas usada para teste
                configurator.AddCommandLineDefinition("r", c => { });
                configurator.AddCommandLineDefinition("p", c => { });
                configurator.AddCommandLineDefinition("s", c => { });
                configurator.AddCommandLineDefinition("repeat", c => { });
                configurator.AddCommandLineDefinition("person", c => { });
                configurator.AddCommandLineDefinition("save", c => { });
                configurator.ApplyCommandLine();

                configurator.Service<MainTask>(host =>
                {
                    host.ConstructUsing(instance => new MainTask());
                    host.WhenStarted(instance => instance.Start());
                    host.WhenStopped(instance => instance.Stop());
                });

                configurator.SetServiceName("OlaMundo");
                configurator.SetDisplayName("Olá Mundo");
                configurator.SetDescription("---------ChangeAppsettingsInRealTime----------");

                configurator.UseEnvironmentBuilder(cfg => new DotNetCoreEnvironmentBuilder(cfg));
                configurator.RunAsLocalSystem();

                configurator.SetStartTimeout(TimeSpan.FromMinutes(1));
                configurator.SetStopTimeout(TimeSpan.FromMinutes(1));

                configurator.EnableServiceRecovery(recovery =>
                {
                    recovery.RestartService(1);
                    recovery.RestartService(3);
                });
            });
        }
    }
}
