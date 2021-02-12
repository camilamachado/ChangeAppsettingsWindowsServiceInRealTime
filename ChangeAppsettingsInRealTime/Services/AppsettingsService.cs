using ndd.SharedKernel.Result;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.IO;

namespace ChangeAppsettingsInRealTime.Services
{
    public class AppsettingsService : IAppsettingsService
    {
        private dynamic _jsonObj;

        public Result<Exception, Unit> Write<T>(string sectionPathKey, T value, string filePath = "")
        {
            if (String.IsNullOrEmpty(filePath))
            {
                filePath = AppContext.BaseDirectory;
            }

            Log.Debug($"Alterando appsettings localizado no path: {filePath}");

            try
            {
                var settingsLocation = Path.Combine(filePath, "appsettings.json");

                string json = File.ReadAllText(settingsLocation);
                _jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                SetValueRecursively(sectionPathKey, _jsonObj, value);

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(_jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(settingsLocation, output);

                Log.Information($"Appsettings alterado com sucesso!");

                return Unit.Successful;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Falha ao gravar alterações no appsettings");
                return ex;
            }
        }

        /// <summary>
        /// Método responsável por entrar dentro dos aninhamentos do arquivo json, encontrar a chave correta e setar o valor
        /// </summary>
        /// <param name="sectionPathKey">Caminho que se encontra a chave que será alterada</param>
        /// <param name="value">Valor que será escrito na chave</param>
        /// https://stackoverflow.com/questions/41653688/asp-net-core-appsettings-json-update-in-code
        private void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
        {
            var remainingSections = sectionPathKey.Split(":", 2);

            var currentSection = remainingSections[0];
            if (remainingSections.Length > 1)
            {
                var nextSection = remainingSections[1];

                jsonObj[currentSection] ??= new JObject();

                SetValueRecursively(nextSection, jsonObj[currentSection], value);
            }
            else
            {
                Log.Debug($"Setando no appsettings na chave: [{currentSection}] o valor: {value}");
                jsonObj[currentSection] = value;
            }
        }
    }
}
