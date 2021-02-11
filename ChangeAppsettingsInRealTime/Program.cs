using CommandLine;
using Serilog;
using System;
using System.IO;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

namespace ChangeAppsettingsInRealTime
{
    public class Program
    {
        public class Options
        {
            [Option('r', "repeat", Required = false, HelpText = "Quantas vezes deseja dizer olá")]
            public int Repeat { get; set; } = 1;

            [Option('p', "person", Required = false, HelpText = "Pra quem deseja dizer olá")]
            public string Person { get; set; } = "world";

            [Option('s', "save", Required = false, HelpText = "Deseja salvar alterações no appsettings?")]
            public bool Save { get; set; } = false;
        }

        public static int Main(string[] args)
        {
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
                           AddOrUpdateAppSetting("Config:Repeat", Convert.ToString(o.Repeat));
                           AddOrUpdateAppSetting("Config:Person", o.Person);
                           AddOrUpdateAppSetting("Config:Save", o.Save);

                           Console.WriteLine($"Dados salvos com sucesso!");
                       }
                   });

            // Configurando serviço windows usando o Topshelf
            return (int)HostFactory.Run(configurator =>
            {
                // Aqui foi necessário a sobrecarga dos comandos
                // pois sem essa configuração o Topshelf não reconhece o comando
                // e apresenta erro no console
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

        // Método responsável por alterar as configurações no appsettings
        public static void AddOrUpdateAppSetting<T>(string key, T value)
        {
            try
            {
                Log.Debug($"Salvando no appsettings na chave {key} o valor {key}");

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
                Log.Error(ex, "Falha ao gravar alterações no appsettings");
            }
        }
    }
}
