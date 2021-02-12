using ndd.SharedKernel.Result;
using System;

namespace ChangeAppsettingsInRealTime.Services
{
    public interface IAppsettingsService
    {
        /// <summary>
        /// Escreve no appsettings.json
        /// </summary>
        /// <param name="key">Chave que deve escrita. A cada camada de aninhamento é necessário separar por : (dois pontos). EX: Config:Repeat </param>
        /// <param name="value">Valor que será escrito na chave</param>
        /// <param name="filePath">Caminho do arquivo</param>
        Result<Exception, Unit> Write<T>(string key, T value, string filePath = "");
    }
}
