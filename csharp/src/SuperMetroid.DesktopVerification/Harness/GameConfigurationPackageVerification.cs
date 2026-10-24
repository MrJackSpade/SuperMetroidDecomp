using SuperMetroid.Core;
using System.Reflection;
using SuperMetroid.Diagnostics.Configuration;
namespace SuperMetroid.DesktopVerification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Exercises the published host's real INI loader without opening a window or touching player data.</summary>
internal static class GameConfigurationPackageVerification
{
    /// <summary>Checks published defaults, first-use files, AppData preservation, and local override precedence.</summary>
    [ReflectionAudit("package verification: the published game assembly is loaded from disk, so its loader is reached by name")]
    public static void Run(string gameAssemblyPath)
    {
        string publish = (Path.GetDirectoryName(Path.GetFullPath(gameAssemblyPath)) ?? throw new InvalidOperationException("Verification requires Path.GetDirectoryName(Path.GetFullPath(gameAssemblyPath))."));
        string template = File.ReadAllText(Path.Combine((publish), "SuperMetroid.defaults.ini"));
        if (template.Replace("\r\n", "\n") != HostOptionsIni.DefaultFileContents.Replace("\r\n", "\n") ||
            HostOptionsIni.Parse(template) != new HostOptions() ||
            HostOptionsIni.Parse("") != new HostOptions())
            throw new InvalidDataException("Published, embedded, parsed and programmatic INI defaults differ.");

        Assembly game = Assembly.LoadFrom(Path.GetFullPath(gameAssemblyPath));
        Type config = (game.GetType("SuperMetroid.Game.GameConfigurationFile", throwOnError: true) ?? throw new InvalidOperationException("Verification requires game.GetType(\"SuperMetroid.Game.GameConfigurationFile\", throwOnError: true)."));
        MethodInfo loadInstalled = (config.GetMethod("LoadInstalled") ?? throw new InvalidOperationException("Verification requires (config ?? throw new InvalidOperationException(\"Verification requires config.\")).GetMet...."));
        // The temporary directory is test-owned. No ROM is needed: this is the same
        // loader called after installation, not a substitute INI parser.
        using var rootScratch = new TestScratchDirectory("config-515");
        string root = rootScratch.Root;
        Directory.CreateDirectory(root);
        try
        {
            HostOptions Load(string data, string defaults, bool local = false)
            {
                Directory.CreateDirectory(data);
                object result = ((loadInstalled ?? throw new InvalidOperationException("Verification requires loadInstalled.")).Invoke(null, [data, defaults]) ?? throw new InvalidOperationException("Verification requires (loadInstalled ?? throw new InvalidOperationException(\"Verification requires loadInstal...."));
                if ((string)((config.GetProperty("Path") ?? throw new InvalidOperationException("Verification requires config.GetProperty(\"Path\").")).GetValue(result) ?? throw new InvalidOperationException("Verification requires (config.GetProperty(\"Path\") ?? throw new InvalidOperationException(\"Verification requir....")) != Path.Combine(local ? defaults : data, "SuperMetroid.ini"))
                    throw new InvalidDataException("Published host selected the wrong player INI path.");
                return (HostOptions)((config.GetProperty("Options") ?? throw new InvalidOperationException("Verification requires config.GetProperty(\"Options\").")).GetValue(result) ?? throw new InvalidOperationException("Verification requires (config.GetProperty(\"Options\") ?? throw new InvalidOperationException(\"Verification req...."));
            }
            string data = Path.Combine(root, "player");
            if (Load(data, publish) != new HostOptions())
                throw new InvalidDataException("First launch did not load the shipped defaults.");
            string edited = template.Replace("MasterVolumePercent=100", "MasterVolumePercent=17")
                .Replace("SkipOpeningCinematic=false", "SkipOpeningCinematic=true");
            string active = Path.Combine(data, "SuperMetroid.ini");
            File.WriteAllText(active, edited);
            if (Load(data, publish) is not { MasterVolumePercent: 17, Game.SkipOpeningCinematic: true } ||
                File.ReadAllText(active) != edited)
                throw new InvalidDataException("Relaunch ignored or overwrote edited player settings.");
            object installedResult = (loadInstalled.Invoke(null, [data, publish]) ?? throw new InvalidOperationException("Verification requires (loadInstalled ?? throw new InvalidOperationException(\"Verification requires loadInstal...."));
            if ((string)((config.GetProperty("Path") ?? throw new InvalidOperationException("Verification requires config.GetProperty(\"Path\").")).GetValue(installedResult) ?? throw new InvalidOperationException("Verification requires (config.GetProperty(\"Path\") ?? throw new InvalidOperationException(\"Verification requir....")) != active ||
                (HostOptions)((config.GetProperty("Options") ?? throw new InvalidOperationException("Verification requires config.GetProperty(\"Options\").")).GetValue(installedResult) ?? throw new InvalidOperationException("Verification requires (config.GetProperty(\"Options\") ?? throw new InvalidOperationException(\"Verification req....")) !=
                HostOptionsIni.Parse(edited))
                throw new InvalidDataException(
                    "Normal installed-content startup did not load player settings without a ROM path.");
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
            if (Load(Path.Combine(root, "embedded-player"), Path.Combine(root, "no-template")) != new HostOptions())
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
