using System.Reflection;
using SuperMetroid.Core.Frontend;

/// <summary>Exercises the published host's real INI loader without opening a window or touching player data.</summary>
internal static class GameConfigurationPackageVerification
{
    public static void Run(string gameAssemblyPath)
    {
        string publish = Path.GetDirectoryName(Path.GetFullPath(gameAssemblyPath))!;
        string template = File.ReadAllText(Path.Combine(publish, "SuperMetroid.defaults.ini"));
        if (template.Replace("\r\n", "\n") != SuperMetroidGameOptionsIni.DefaultFileContents.Replace("\r\n", "\n") ||
            SuperMetroidGameOptionsIni.Parse(template) != new SuperMetroidGameOptions() ||
            SuperMetroidGameOptionsIni.Parse("") != new SuperMetroidGameOptions())
            throw new InvalidDataException("Published, embedded, parsed and programmatic INI defaults differ.");

        Assembly game = Assembly.LoadFrom(Path.GetFullPath(gameAssemblyPath));
        Type config = game.GetType("SuperMetroid.Game.GameConfigurationFile", throwOnError: true)!;
        MethodInfo load = config.GetMethod("LoadOrCreate")!;
        // The temporary directory is test-owned. No ROM is needed: this is the same
        // loader called after installation, not a substitute INI parser.
        string root = Path.Combine(Path.GetTempPath(), "supermetroid-config-515-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            SuperMetroidGameOptions Load(string data, string defaults, bool local = false)
            {
                Directory.CreateDirectory(data);
                object result = load.Invoke(null, [Path.Combine(root, "fixture.smc"), data, defaults])!;
                if ((string)config.GetProperty("Path")!.GetValue(result)! != Path.Combine(local ? defaults : data, "SuperMetroid.ini"))
                    throw new InvalidDataException("Published host selected the wrong player INI path.");
                return (SuperMetroidGameOptions)config.GetProperty("Options")!.GetValue(result)!;
            }
            string data = Path.Combine(root, "player");
            if (Load(data, publish) != new SuperMetroidGameOptions())
                throw new InvalidDataException("First launch did not load the shipped defaults.");
            string edited = template.Replace("MasterVolumePercent=100", "MasterVolumePercent=17")
                .Replace("SkipOpeningCinematic=false", "SkipOpeningCinematic=true");
            string active = Path.Combine(data, "SuperMetroid.ini");
            File.WriteAllText(active, edited);
            if (Load(data, publish) is not { MasterVolumePercent: 17, SkipOpeningCinematic: true } ||
                File.ReadAllText(active) != edited)
                throw new InvalidDataException("Relaunch ignored or overwrote edited player settings.");
            string customized = Path.Combine(root, "customized-release");
            Directory.CreateDirectory(customized);
            File.WriteAllText(Path.Combine(customized, "SuperMetroid.defaults.ini"), edited);
            if (Load(Path.Combine(root, "fresh-player"), customized).MasterVolumePercent != 17)
                throw new InvalidDataException("Edited first-use release template was ignored.");
            File.WriteAllText(Path.Combine(customized, "SuperMetroid.defaults.ini"), "[Invalid]\nBad=true");
            if (Load(data, customized).MasterVolumePercent != 17 || File.ReadAllText(active) != edited)
                throw new InvalidDataException("Updated template affected existing player settings.");
            string invalidData = Path.Combine(root, "invalid-first-use");
            bool rejected = false;
            try { Load(invalidData, customized); }
            catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { rejected = true; }
            if (!rejected || File.Exists(Path.Combine(invalidData, "SuperMetroid.ini")))
                throw new InvalidDataException("Invalid template created a broken active INI.");
            if (Load(Path.Combine(root, "embedded-player"), Path.Combine(root, "no-template")) != new SuperMetroidGameOptions())
                throw new InvalidDataException("Embedded default fallback differs from the release defaults.");
            string localPath = Path.Combine(customized, "SuperMetroid.ini");
            File.WriteAllText(localPath, "[Diagnostics]\nReportErrorsToGitHub=true\n[Audio]\nMasterVolumePercent=23\n");
            if (Load(data, customized, local: true) is not { ReportErrorsToGitHub: true, MasterVolumePercent: 23 } ||
                File.ReadAllText(active) != edited)
                throw new InvalidDataException("Executable-directory override did not win or changed AppData settings.");
            File.WriteAllText(localPath, "[Invalid]\nBad=true");
            rejected = false;
            try { Load(data, customized, local: true); }
            catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { rejected = true; }
            if (!rejected) throw new InvalidDataException("Invalid local override silently fell back to AppData.");
            File.Delete(localPath);
            if (Load(data, customized).MasterVolumePercent != 17)
                throw new InvalidDataException("Removing the local override did not restore AppData selection.");
            Console.WriteLine("PASS executable INI precedence, reporting=true, untouched AppData, invalid override rejection and removal fallback.");
            Console.WriteLine("PASS published INI: shipped defaults, first-use edits, active edits, update preservation, invalid-template rejection and embedded fallback.");
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
