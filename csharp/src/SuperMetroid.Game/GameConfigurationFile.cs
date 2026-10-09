using SuperMetroid.Core.Frontend;
using System.Text;

namespace SuperMetroid.Game;

/// <summary>Loads player settings from the installation data root (or legacy ROM directory).</summary>
/// <param name="Path">Absolute path of the INI file that supplied the loaded settings.</param>
/// <param name="Options">Parsed gameplay and presentation settings from that file.</param>
/// <param name="Source">Description of the selected settings location, such as an executable-directory override.</param>
internal sealed record GameConfigurationFile(
    string Path,
    SuperMetroidGameOptions Options,
    string Source)
{
    /// <summary>Filename of the active player configuration stored in the data or executable directory.</summary>
    public const string FileName = "SuperMetroid.ini";
    /// <summary>Filename of the first-run template used to seed a missing player configuration.</summary>
    public const string DefaultsFileName = "SuperMetroid.defaults.ini";

    /// <summary>Loads normal player settings from the installation, independent of its import ROM.</summary>
    public static GameConfigurationFile LoadInstalled(string dataDirectory, string? defaultsDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        return LoadFromDirectory(dataDirectory, defaultsDirectory, "installation data-directory settings");
    }

    /// <summary>Loads or initializes the player INI, preferring an executable-directory override over the data-root file.</summary>
    /// <param name="dataDirectory">Installation data root used when no local override exists.</param>
    /// <param name="defaultsDirectory">Optional directory for the active override and defaults template.</param>
    /// <param name="source">Origin description recorded with the loaded file unless a local override is selected.</param>
    /// <returns>The resolved settings-file path and its parsed options.</returns>
    private static GameConfigurationFile LoadFromDirectory(string dataDirectory,
        string? defaultsDirectory, string source)
    {
        string executableDirectory = defaultsDirectory ?? AppContext.BaseDirectory;
        string localPath = System.IO.Path.Combine(executableDirectory, FileName);
        bool localOverride = File.Exists(localPath);
        string configurationPath = System.IO.Path.GetFullPath(localOverride ? localPath :
            System.IO.Path.Combine(dataDirectory, FileName));

        if (!File.Exists(configurationPath))
        {
            // The separately named release template cannot overwrite an active INI when
            // a ZIP is extracted over an old application folder. It seeds only first use;
            // existing player settings always win, even if the template later changes.
            string templatePath = System.IO.Path.Combine(defaultsDirectory ?? AppContext.BaseDirectory,
                DefaultsFileName);
            string initialContents = File.Exists(templatePath)
                ? File.ReadAllText(templatePath) : SuperMetroidGameOptionsIni.DefaultFileContents;
            _ = SuperMetroidGameOptionsIni.Parse(initialContents, templatePath);
            // UTF-8 without a byte-order mark remains readable in ordinary text editors and
            // avoids putting an invisible character in front of the first INI comment.
            // CreateNew also prevents a concurrent launch from overwriting a new user file.
            using var stream = new FileStream(configurationPath, FileMode.CreateNew, FileAccess.Write);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(initialContents);
        }

        string contents = File.ReadAllText(configurationPath);
        SuperMetroidGameOptions options =
            SuperMetroidGameOptionsIni.Parse(contents, configurationPath);
        return new GameConfigurationFile(configurationPath, options,
            localOverride ? "executable-directory override" : source);
    }
}
