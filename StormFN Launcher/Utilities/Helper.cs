using System;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StormFN Launcher
{
    class Helper
    {
        const string BaseArguments = "-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -skippatchcheck";

        public async Task Launch(string arguments)
        {
            var tempDirPath = Path.Combine(Path.GetTempPath(), $"FortniteClient-Win64-Shipping_EAC.exe");

            if (!File.Exists("Platanium.dll"))
                return;

            arguments += $" {BaseArguments} -epiclocale=en -noeac -fromfl=be -fltoken=5dh74c635862g575778132fb -frombe";

            var fortniteDirectory = GetEpicInstallLocations().FirstOrDefault(i => i.AppName == "Fortnite")?.InstallLocation;

            try
            {
                var existingAntiCheatProcess = Process.GetProcessesByName("FortniteClient-Win64-Shipping_EAC")?.FirstOrDefault();

                if (existingAntiCheatProcess.MainModule.FileName == tempDirPath && existingAntiCheatProcess != null)
                {
                    existingAntiCheatProcess.Kill();
                    Thread.Sleep(200);
                }
            }
            catch { }

            if (!File.Exists(tempDirPath))
                await new WebClient().DownloadFileTaskAsync("https://cdn.discordapp.com/attachments/788954437510496266/801534129652629514/FortniteClient-Win64-Shipping_EAC.exe", tempDirPath);

            var clientProcess = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(fortniteDirectory, "FortniteGame\\Binaries\\Win64\\FortniteClient-Win64-Shipping.exe"), arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            var antiCheatProcess = new Process
            {
                StartInfo = new ProcessStartInfo(tempDirPath, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            clientProcess.Start();
            antiCheatProcess.Start();

            Injector.Inject(clientProcess.Id, "Platanium.dll");
            Listener.Listen(Environment.GetCommandLineArgs());
            clientProcess.WaitForExit();

            antiCheatProcess.Kill();
            Thread.Sleep(200);

            File.Delete(tempDirPath);
        }

        public static List<Installation> GetEpicInstallLocations()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic\\UnrealEngineLauncher\\LauncherInstalled.dat");

            if (!Directory.Exists(Path.GetDirectoryName(path)) || !File.Exists(path))
                return null;

            return JsonConvert.DeserializeObject<EpicInstallLocations>(File.ReadAllText(path)).InstallationList;
        }

        public class EpicInstallLocations
        {
            [JsonProperty("InstallationList")]
            public List<Installation> InstallationList { get; set; }
        }

        public class Installation
        {
            [JsonProperty("InstallLocation")]
            public string InstallLocation { get; set; }

            [JsonProperty("AppName")]
            public string AppName { get; set; }
        }
    }
}