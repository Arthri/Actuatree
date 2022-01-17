using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TerrariaApi.Server;
using TShockAPI;

namespace ActuatreePlugin
{
    partial class Actuatree
    {
        private Assembly[] OpenSourceAssemblies = new Assembly[]
        {
            typeof(AssemblyDefinition).Assembly,
            typeof(Detour).Assembly,
            typeof(ILCursor).Assembly,
        };

        private void Initialize_Credits()
        {
            LogInfo("Actuatree uses open-source components:");
            GetOpenSourceCredits()
                .ForEach(s => LogInfo(s));

            void LogInfo(string message)
            {
                ServerApi.LogWriter.PluginWriteLine(this, message, TraceLevel.Info);
            }



            Commands.ChatCommands.Add(new Command(HandleCredits, "actuatree:credits"));
        }

        private IEnumerable<string> GetOpenSourceCredits()
        {
            return OpenSourceAssemblies
                .Select(assembly =>
                {
                    var assemblyName = assembly.GetName();
                    var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assemblyName.Name;
                    var version = assemblyName.Version;
                    var authors = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "NO_AUTHORS_IN_MANIFEST";
                    var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "NO_COPYRIGHT_NOTICE";
                    return $"{title}(v{version}) by {authors}. {copyright}";
                });
        }

        private void HandleCredits(CommandArgs args)
        {
            args.Player.SendInfoMessage("Actuatree uses open-source components:");
            GetOpenSourceCredits()
                .ForEach(s => args.Player.SendInfoMessage(s));
        }
    }
}
