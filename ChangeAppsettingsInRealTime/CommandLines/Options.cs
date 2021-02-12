using CommandLine;

namespace ChangeAppsettingsInRealTime.CommandLines
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
}
