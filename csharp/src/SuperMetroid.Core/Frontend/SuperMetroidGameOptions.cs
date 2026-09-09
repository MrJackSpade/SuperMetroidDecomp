using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

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
    /// <summary>Prefer hardware rendering; report startup failure before selecting the software reference backend.</summary>
    public RendererSelection Renderer { get; init; } = RendererSelection.Auto;
    /// <summary>
    /// Skips the story sequence between accepting the options screen and arriving at Ceres.
    /// </summary>
    /// <remarks>
    /// The title sequence, file-select screen, and game-options screen still run normally.
    /// The skip enters the same new-game setup state used when the cinematic finishes; it
    /// does not synthesize controller input or construct a special debug room.
    /// </remarks>
    public bool SkipOpeningCinematic { get; init; }

    /// <summary>Prevents emulated gameplay damage from reducing Samus below one energy.</summary>
    /// <remarks>
    /// This is a host-side testing convenience, not cartridge state. Damage routines still
    /// execute, so energy loss, hit reactions, and collision behavior remain available for
    /// debugging; the runtime changes a lethal zero-energy result to one. Shinesparks
    /// also bypass their low-energy cutoff and continue draining down to one, so this
    /// testing mode never requires an energy refill to use them. Collisions still stop them.
    /// </remarks>
    public bool Invincibility { get; init; }

    /// <summary>Keeps each unlocked consumable ammo type at one or more units.</summary>
    /// <remarks>
    /// Missiles, super missiles, and power bombs still consume normally. At the runtime
    /// frame boundary, an unlocked type which reached zero is raised to one. A type whose
    /// maximum remains zero is still locked and is never granted by this host option.
    /// </remarks>
    public bool InfiniteAmmo { get; init; }

    /// <summary>Lets Ceres and Zebes escape countdowns run normally, but holds them at 00:01.00 instead of expiring.</summary>
    public bool PreventEscapeTimeout { get; init; }

    /// <summary>Optional total minutes used only for ending time/reward selection; null preserves actual playtime and SRAM is never changed.</summary>
    public ushort? EndingTimeOverrideMinutes { get; init; }

    /// <summary>Temporary map visibility used by the HUD and pause-map presentations.</summary>
    /// <remarks>
    /// This host convenience is evaluated only while drawing. It never changes exploration
    /// bits or area-map acquisition flags, so returning to <see cref="MapRevealMode.None"/>
    /// immediately restores the cartridge's saved visibility.
    /// </remarks>
    public MapRevealMode MapReveal { get; init; }

    /// <summary>Whether the desktop host creates the SPC/DSP mixer and Windows device.</summary>
    public bool AudioEnabled { get; init; } = true;

    /// <summary>Host PCM gain after cartridge mixing, from zero (mute) to 100.</summary>
    public int MasterVolumePercent { get; init; } = 100;

    /// <summary>Whether recoverable desktop-host errors are filed as GitHub issues.</summary>
    /// <remarks>
    /// This development convenience is deliberately disabled by default. When enabled, the
    /// desktop host prints the complete failure locally, queues a deduplicated issue through
    /// the authenticated GitHub CLI, and attempts the next emulated frame. It is not part of
    /// cartridge state and is therefore not serialized into deterministic input recordings.
    /// </remarks>
    public bool ReportErrorsToGitHub { get; init; }

    /// <summary>GitHub <c>owner/repository</c> receiving automatic private error reports.</summary>
    public string GitHubErrorRepository { get; init; } = "MrJackSpade/SuperMetroidDecomp";
}

/// <summary>Strict reader and documented template for the playable executable's INI file.</summary>
public static partial class SuperMetroidGameOptionsIni
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
        "; true allows damage but prevents Samus from dropping below 1 energy\r\n" +
        "; also lets shinesparks continue at low energy, draining down to 1\r\n" +
        "; false preserves normal cartridge damage and death behavior\r\n" +
        "Invincibility=false\r\n" +
        "; true allows normal consumption but keeps unlocked ammo types at 1 or more\r\n" +
        "; false preserves normal cartridge ammunition behavior\r\n" +
        "InfiniteAmmo=false\r\n" +
        "; true = Ceres and post-Mother-Brain timers count down normally, then stop at 00:01.00\r\n" +
        "PreventEscapeTimeout=false\r\n" +
        "; None = actual playtime; 0..5999 = ending-only total minutes (0 selects fastest ending)\r\n" +
        "EndingTimeOverrideMinutes=None\r\n" +
        "; None = normal save/map-station visibility\r\n" +
        "; Public = temporarily reveal everything an ordinary map station exposes\r\n" +
        "; Secret = temporarily reveal every valid map cell, including hidden cells\r\n" +
        "MapReveal=None\r\n" +
        "\r\n" +
        "[Audio]\r\n" +
        "; Enables the cartridge SPC sequencer, BRR samples, DSP mixing, and playback\r\n" +
        "Enabled=true\r\n" +
        "; Final host gain after SNES mixing; integer from 0 through 100\r\n" +
        "MasterVolumePercent=100\r\n" +
        "\r\n" +
        "[Video]\r\n" +
        "; Software | Direct3D11 | Auto; Auto prefers hardware and is the default\r\n" +
        "; Auto logs hardware startup failure and falls back to Software\r\n" +
        "Renderer=Auto\r\n" +
        "\r\n" +
        "[Diagnostics]\r\n" +
        "; true files recoverable runtime errors through the authenticated GitHub CLI\r\n" +
        "; repeated errors share a stable fingerprint and never create duplicate issues\r\n" +
        "ReportErrorsToGitHub=false\r\n" +
        "; Private repository in owner/name form; no ROM contents are attached\r\n" +
        "GitHubErrorRepository=MrJackSpade/SuperMetroidDecomp\r\n";

    /// <summary>Parses the supported INI surface and rejects misspelled or ambiguous keys.</summary>
    public static SuperMetroidGameOptions Parse(
        string contents,
        string sourceName = "SuperMetroid.ini")
    {
        ArgumentNullException.ThrowIfNull(contents);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        bool? skipOpeningCinematic = null;
        bool? invincibility = null;
        bool? infiniteAmmo = null;
        bool? preventEscapeTimeout = null;
        ushort? endingTimeOverrideMinutes = null;
        bool endingTimeOverrideSeen = false;
        MapRevealMode? mapReveal = null;
        bool? audioEnabled = null;
        RendererSelection? renderer = null;
        int? masterVolumePercent = null;
        bool? reportErrorsToGitHub = null;
        string? githubErrorRepository = null;
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
                    !currentSection.Equals("Audio", StringComparison.OrdinalIgnoreCase) &&
                    !currentSection.Equals("Video", StringComparison.OrdinalIgnoreCase) &&
                    !currentSection.Equals("Diagnostics", StringComparison.OrdinalIgnoreCase))
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
            if (currentSection.Equals("Video", StringComparison.OrdinalIgnoreCase))
            {
                if (!key.Equals(nameof(SuperMetroidGameOptions.Renderer), StringComparison.OrdinalIgnoreCase))
                    throw Invalid(sourceName, lineNumber, $"unknown [Video] option '{key}'");
                if (renderer.HasValue) throw Invalid(sourceName, lineNumber, "duplicate [Video] Renderer option");
                renderer = value.ToUpperInvariant() switch
                {
                    "SOFTWARE" => RendererSelection.Software,
                    "DIRECT3D11" => RendererSelection.Direct3D11,
                    "AUTO" => RendererSelection.Auto,
                    _ => throw Invalid(sourceName, lineNumber, "Renderer must be Software, Direct3D11 or Auto")
                };
                continue;
            }
            if (currentSection.Equals("Game", StringComparison.OrdinalIgnoreCase))
            {
                if (key.Equals(nameof(SuperMetroidGameOptions.SkipOpeningCinematic),
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (skipOpeningCinematic.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    skipOpeningCinematic = ParseBoolean(sourceName, lineNumber, key, value);
                    continue;
                }

                if (key.Equals(nameof(SuperMetroidGameOptions.Invincibility),
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (invincibility.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    invincibility = ParseBoolean(sourceName, lineNumber, key, value);
                    continue;
                }

                if (key.Equals(nameof(SuperMetroidGameOptions.InfiniteAmmo),
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (infiniteAmmo.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    infiniteAmmo = ParseBoolean(sourceName, lineNumber, key, value);
                    continue;
                }

                if (key.Equals(nameof(SuperMetroidGameOptions.PreventEscapeTimeout), StringComparison.OrdinalIgnoreCase))
                {
                    if (preventEscapeTimeout.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    preventEscapeTimeout = ParseBoolean(sourceName, lineNumber, key, value);
                    continue;
                }

                if (key.Equals(nameof(SuperMetroidGameOptions.EndingTimeOverrideMinutes), StringComparison.OrdinalIgnoreCase))
                {
                    if (endingTimeOverrideSeen)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    endingTimeOverrideSeen = true;
                    if (!value.Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!ushort.TryParse(value, out ushort minutes) || minutes > 5999)
                            throw Invalid(sourceName, lineNumber, $"{key} must be None or total minutes from 0 through 5999");
                        endingTimeOverrideMinutes = minutes;
                    }
                    continue;
                }

                if (key.Equals(nameof(SuperMetroidGameOptions.MapReveal),
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (mapReveal.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    if (!Enum.TryParse(value, ignoreCase: true, out MapRevealMode parsed) ||
                        !Enum.IsDefined(parsed))
                    {
                        throw Invalid(
                            sourceName,
                            lineNumber,
                            $"{key} must be None, Public, or Secret, not '{value}'");
                    }
                    mapReveal = parsed;
                    continue;
                }

                throw Invalid(sourceName, lineNumber, $"unknown [Game] option '{key}'");
            }

            if (currentSection.Equals("Audio", StringComparison.OrdinalIgnoreCase) &&
                key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
            {
                if (audioEnabled.HasValue)
                    throw Invalid(sourceName, lineNumber, $"duplicate [Audio] option '{key}'");
                if (!bool.TryParse(value, out bool parsed))
                    throw Invalid(sourceName, lineNumber, $"{key} must be either true or false, not '{value}'");
                audioEnabled = parsed;
            }
            else if (currentSection.Equals("Audio", StringComparison.OrdinalIgnoreCase) &&
                     key.Equals(nameof(SuperMetroidGameOptions.MasterVolumePercent),
                         StringComparison.OrdinalIgnoreCase))
            {
                if (masterVolumePercent.HasValue)
                    throw Invalid(sourceName, lineNumber, $"duplicate [Audio] option '{key}'");
                if (!int.TryParse(value, out int parsed) || parsed is < 0 or > 100)
                    throw Invalid(sourceName, lineNumber, $"{key} must be an integer from 0 through 100, not '{value}'");
                masterVolumePercent = parsed;
            }
            else if (currentSection.Equals("Audio", StringComparison.OrdinalIgnoreCase))
            {
                throw Invalid(sourceName, lineNumber, $"unknown [Audio] option '{key}'");
            }
            else if (key.Equals(nameof(SuperMetroidGameOptions.ReportErrorsToGitHub),
                         StringComparison.OrdinalIgnoreCase))
            {
                if (reportErrorsToGitHub.HasValue)
                    throw Invalid(sourceName, lineNumber, $"duplicate [Diagnostics] option '{key}'");
                reportErrorsToGitHub = ParseBoolean(sourceName, lineNumber, key, value);
            }
            else if (key.Equals(nameof(SuperMetroidGameOptions.GitHubErrorRepository),
                         StringComparison.OrdinalIgnoreCase))
            {
                if (githubErrorRepository is not null)
                    throw Invalid(sourceName, lineNumber, $"duplicate [Diagnostics] option '{key}'");
                if (!IsGitHubRepositoryName(value))
                {
                    throw Invalid(
                        sourceName,
                        lineNumber,
                        $"{key} must use owner/repository form, not '{value}'");
                }
                githubErrorRepository = value;
            }
            else
            {
                throw Invalid(sourceName, lineNumber, $"unknown [Diagnostics] option '{key}'");
            }
        }

        return new SuperMetroidGameOptions
        {
            // A missing key is deliberately equivalent to the retail path. This makes old
            // or empty local configuration files safe when a new host option is introduced.
            SkipOpeningCinematic = skipOpeningCinematic ?? false,
            Invincibility = invincibility ?? false,
            InfiniteAmmo = infiniteAmmo ?? false,
            PreventEscapeTimeout = preventEscapeTimeout ?? false,
            EndingTimeOverrideMinutes = endingTimeOverrideMinutes,
            MapReveal = mapReveal ?? MapRevealMode.None,
            AudioEnabled = audioEnabled ?? true,
            Renderer = renderer ?? RendererSelection.Auto,
            MasterVolumePercent = masterVolumePercent ?? 100,
            ReportErrorsToGitHub = reportErrorsToGitHub ?? false,
            GitHubErrorRepository = githubErrorRepository ?? "MrJackSpade/SuperMetroidDecomp",
        };
    }

    private static bool IsGitHubRepositoryName(string value)
    {
        string[] components = value.Split('/');
        return components.Length == 2 &&
            components.All(component =>
                component.Length != 0 &&
                component.All(character =>
                    char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.'));
    }

    private static bool ParseBoolean(
        string sourceName,
        int lineNumber,
        string key,
        string value)
    {
        if (bool.TryParse(value, out bool parsed))
            return parsed;
        throw Invalid(
            sourceName,
            lineNumber,
            $"{key} must be either true or false, not '{value}'");
    }

    private static InvalidDataException Invalid(string sourceName, int lineNumber, string reason) =>
        new($"Invalid game configuration in '{sourceName}' at line {lineNumber}: {reason}.");
}
