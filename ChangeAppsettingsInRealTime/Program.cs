using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

namespace ChangeAppsettingsInRealTime
{
    public class Program
    {
        // Command Lines disponíveis
        [Option(Description = "Sujeito que deseja dizer olá")]
        public string Subject { get; } = "world";

        [Option(ShortName = "n", Description = "Quantas vezes deseja dizer olá")]
        public int Count { get; } = 1;

        // Esse método é chamado quando o serviço é inicializado e cada vez que um Command Line válido é executado
        private void OnExecute()
        {
            for (var i = 0; i < Count; i++)
            {
                Log.Debug($"Hello {Subject}!");
                Console.WriteLine($"Hello {Subject}!");
            }

            AddOrUpdateAppSetting("Config:Subject", "Dev");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var subjectAppsettings = config["Config:Subject"];

            for (var i = 0; i < Count; i++)
            {
                Log.Debug($"Hello {subjectAppsettings}!");
                Console.WriteLine($"Hello {subjectAppsettings}!");
            }
        }

        public static int Main(string[] args)
        {
            // Configurando log
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"{AppContext.BaseDirectory}\\log.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // Inicializando CommandLine
            CommandLineApplication.Execute<Program>(args);

            // Configurando serviço windows usando o Topshelf
            return (int)HostFactory.Run(configurator =>
            {
                int count = 1;
                // Aqui foi necessário a sobrecarga do comando n
                // pois sem essa configuração o Topshelf não reconhece o comando
                // e apresenta erro no console
                configurator.AddCommandLineDefinition("n", c =>
                {
                    count = Convert.ToInt32(c);
                });
                configurator.ApplyCommandLine();

                configurator.Service<MainTask>(host =>
                {
                    host.ConstructUsing(instance => new MainTask());
                    host.WhenStarted(instance => instance.Start());
                    host.WhenStopped(instance => instance.Stop());
                });

                configurator.SetServiceName("1Teste");
                configurator.SetDisplayName("1Teste");
                configurator.SetDescription("----");

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

        // Método responsável por alterar as configurações no appsettings
        public static void AddOrUpdateAppSetting<T>(string key, T value)
        {
            try
            {

                var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                var sectionPath = key.Split(":")[0];
                if (!string.IsNullOrEmpty(sectionPath))
                {
                    var keyPath = key.Split(":")[1];
                    jsonObj[sectionPath][keyPath] = value;
                }
                else
                {
                    jsonObj[sectionPath] = value;
                }
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, output);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing app settings {ex}");
            }
        }
    }
}
