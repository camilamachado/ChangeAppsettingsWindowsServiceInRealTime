using Serilog;

namespace ChangeAppsettingsInRealTime
{
    public class MainTask
    {
        public void Start()
        {
            Log.Debug("Iniciando serviço....");
        }

        public void Stop()
        {
            Log.Debug("Serviço foi parado!");
        }
    }
}
