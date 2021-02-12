using Serilog;

namespace ChangeAppsettingsInRealTime
{
    public class MainTask
    {
        public void Start()
        {
            Log.Information("Iniciando serviço....");
        }

        public void Stop()
        {
            Log.Information("Serviço foi parado!");
        }
    }
}
