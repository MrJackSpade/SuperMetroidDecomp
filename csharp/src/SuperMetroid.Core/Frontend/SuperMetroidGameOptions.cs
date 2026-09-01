namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Host-selected behavior for a complete <see cref="SuperMetroidGame"/> session.
/// </summary>
/// <remarks>
/// These are deliberately not SNES registers or translated WRAM fields. They describe the
/// few conveniences supplied by the C# host around the cartridge-authentic state machine.
/// Keeping them in one named type makes every departure from normal retail startup visible
/// at the point where a game session is constructed.
/// </remarks>
public sealed record SuperMetroidGameOptions
{
    /// <summary>
    /// Skips the story sequence between accepting the options screen and arriving at Ceres.
    /// </summary>
    /// <remarks>
    /// The title sequence, file-select screen, and game-options screen still run normally.
    /// The skip enters the same new-game setup state used when the cinematic finishes; it
    /// does not synthesize controller input or construct a special debug room.
    /// </remarks>
    public bool SkipOpeningCinematic { get; init; }

    /// <summary>Whether the desktop host creates the SPC/DSP mixer and Windows device.</summary>
    public bool AudioEnabled { get; init; } = true;

    /// <summary>Host PCM gain after cartridge mixing, from zero (mute) to 100.</summary>
    public int MasterVolumePercent { get; init; } = 100;
}

/// <summary>Strict reader and documented template for the playable executable's INI file.</summary>
public static class SuperMetroidGameOptionsIni
{
    /// <summary>
    /// Contents written when no <c>SuperMetroid.ini</c> exists beside the private ROM.
    /// </summary>
    /// <remarks>
    /// Normal retail behavior remains the default. A checked-in copy of this same template
    /// also makes the option discoverable before the playable executable has been run once.
    /// </remarks>
    public const string DefaultFileContents =
        "; Super Metroid C# playable-host settings\r\n" +
        "; This file belongs beside your private .smc/.sfc cartridge image.\r\n" +
        "\r\n" +
        "[Game]\r\n" +
        "; true  = keep title/file select/options, then go directly to the Ceres elevator\r\n" +
        "; false = play the narration, flashbacks, and Ceres approach before the elevator\r\n" +
        "SkipOpeningCinematic=false\r\n" +
        "\r\n" +
        "[Audio]\r\n" +
        "; Enables the cartridge SPC sequencer, BRR samples, DSP mixing, and playback\r\n" +
        "Enabled=true\r\n" +
        "; Final host gain after SNES mixing; integer from 0 through 100\r\n" +
        "MasterVolumePercent=100\r\n";

    /// <summary>Parses the supported INI surface and rejects misspelled or ambiguous keys.</summary>
    public static SuperMetroidGameOptions Parse(
        string contents,
        string sourceName = "SuperMetroid.ini")
    {
        ArgumentNullException.ThrowIfNull(contents);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        bool? skipOpeningCinematic = null;
        bool? audioEnabled = null;
        int? masterVolumePercent = null;
        string currentSection = string.Empty;
        string[] lines = contents.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            int lineNumber = lineIndex + 1;
            string line = lines[lineIndex].Trim();

            // INI comments are intentionally whole-line only. Treating a semicolon inside
            // a value as an implicit comment would make future string options surprising.
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line[1..^1].Trim();
                if (!currentSection.Equals("Game", StringComparison.OrdinalIgnoreCase) &&
                    !currentSection.Equals("Audio", StringComparison.OrdinalIgnoreCase))
                    throw Invalid(sourceName, lineNumber, $"unknown section [{currentSection}]");
                continue;
            }

            int equals = line.IndexOf('=');
            if (equals <= 0)
                throw Invalid(sourceName, lineNumber, "expected a key=value assignment");
            if (currentSection.Length == 0)
                throw Invalid(sourceName, lineNumber, "option appears before a section");

            string key = line[..equals].Trim();
            string value = line[(equals + 1)..].Trim();
            if (currentSection.Equals("Game", StringComparison.OrdinalIgnoreCase))
            {
                if (!key.Equals(nameof(SuperMetroidGameOptions.SkipOpeningCinematic),
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw Invalid(sourceName, lineNumber, $"unknown [Game] option '{key}'");
                }

                if (skipOpeningCinematic.HasValue)
                    throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                if (!bool.TryParse(value, out bool parsed))
                {
                    throw Invalid(
                        sourceName,
                        lineNumber,
                        $"{key} must be either true or false, not '{value}'");
                }
                skipOpeningCinematic = parsed;
                continue;
            }

            if (key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
            {
                if (audioEnabled.HasValue)
                    throw Invalid(sourceName, lineNumber, $"duplicate [Audio] option '{key}'");
                if (!bool.TryParse(value, out bool parsed))
                    throw Invalid(sourceName, lineNumber, $"{key} must be either true or false, not '{value}'");
                audioEnabled = parsed;
            }
            else if (key.Equals(nameof(SuperMetroidGameOptions.MasterVolumePercent),
                         StringComparison.OrdinalIgnoreCase))
            {
                if (masterVolumePercent.HasValue)
                    throw Invalid(sourceName, lineNumber, $"duplicate [Audio] option '{key}'");
                if (!int.TryParse(value, out int parsed) || parsed is < 0 or > 100)
                    throw Invalid(sourceName, lineNumber, $"{key} must be an integer from 0 through 100, not '{value}'");
                masterVolumePercent = parsed;
            }
            else
            {
                throw Invalid(sourceName, lineNumber, $"unknown [Audio] option '{key}'");
            }
        }

        return new SuperMetroidGameOptions
        {
            // A missing key is deliberately equivalent to the retail path. This makes old
            // or empty local configuration files safe when a new host option is introduced.
            SkipOpeningCinematic = skipOpeningCinematic ?? false,
            AudioEnabled = audioEnabled ?? true,
            MasterVolumePercent = masterVolumePercent ?? 100,
        };
    }

    private static InvalidDataException Invalid(string sourceName, int lineNumber, string reason) =>
        new($"Invalid game configuration in '{sourceName}' at line {lineNumber}: {reason}.");
}
