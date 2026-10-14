using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Core.Frontend;

/// <summary>Strict reader and documented template for the playable executable's INI file.</summary>
public static class SuperMetroidGameOptionsIni
{
    /// <summary>
    /// Documented defaults written when the host has no active <c>SuperMetroid.ini</c>.
    /// </summary>
    /// <remarks>
    /// The same tracked INI is embedded here and shipped beside the Windows executable.
    /// Keeping one source prevents the release template and startup defaults from drifting.
    /// </remarks>
    public static string DefaultFileContents { get; } = ReadDefaultFile();

    /// <summary>Reads the embedded release INI template that supplies startup defaults.</summary>
    private static string ReadDefaultFile()
    {
        using Stream stream = typeof(SuperMetroidGameOptionsIni).Assembly.GetManifestResourceStream(
            "SuperMetroid.defaults.ini")
            ?? throw new InvalidDataException("The documented default INI resource is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>Parses the supported INI surface and rejects misspelled or ambiguous keys.</summary>
    public static SuperMetroidGameOptions Parse(
        string contents,
        string sourceName = "SuperMetroid.ini")
    {
        Ensure.NotNull(contents);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        bool? skipOpeningCinematic = null;
        bool? invincibility = null;
        bool? infiniteAmmo = null;
        bool? grantAllEquipment = null;
        bool? unlockTourian = null;
        bool? resetBossesOnLoad = null;
        bool? doorTransitionAutosave = null;
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

                if (key.Equals(nameof(SuperMetroidGameOptions.GrantAllEquipment), StringComparison.OrdinalIgnoreCase))
                {
                    if (grantAllEquipment.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    grantAllEquipment = ParseBoolean(sourceName, lineNumber, key, value);
                    continue;
                }
                if (key.Equals(nameof(SuperMetroidGameOptions.DoorTransitionAutosave), StringComparison.OrdinalIgnoreCase))
                {
                    if (doorTransitionAutosave.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    doorTransitionAutosave = ParseBoolean(sourceName, lineNumber, key, value);
                    continue;
                }
                if (key.Equals(nameof(SuperMetroidGameOptions.ResetBossesOnLoad), StringComparison.OrdinalIgnoreCase))
                {
                    if (resetBossesOnLoad.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    resetBossesOnLoad = ParseBoolean(sourceName, lineNumber, key, value);
                    continue;
                }
                if (key.Equals(nameof(SuperMetroidGameOptions.UnlockTourian), StringComparison.OrdinalIgnoreCase))
                {
                    if (unlockTourian.HasValue)
                        throw Invalid(sourceName, lineNumber, $"duplicate [Game] option '{key}'");
                    unlockTourian = ParseBoolean(sourceName, lineNumber, key, value);
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
            GrantAllEquipment = grantAllEquipment ?? false,
            UnlockTourian = unlockTourian ?? false,
            ResetBossesOnLoad = resetBossesOnLoad ?? false,
            DoorTransitionAutosave = doorTransitionAutosave ?? true,
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

    /// <summary>Accepts an owner/repository pair made from GitHub-safe ASCII name characters.</summary>
    private static bool IsGitHubRepositoryName(string value)
    {
        string[] components = value.Split('/');
        return components.Length == 2 &&
            components.All(component =>
                component.Length != 0 &&
                component.All(character =>
                    char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.'));
    }

    /// <summary>Parses a strict true/false setting and includes its INI location on failure.</summary>
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

    /// <summary>Formats a configuration error with the source filename and one-based line number.</summary>
    private static InvalidDataException Invalid(string sourceName, int lineNumber, string reason) =>
        new($"Invalid game configuration in '{sourceName}' at line {lineNumber}: {reason}.");

    /// <summary>Updates one validated setting while preserving unrelated lines and comments.</summary>
    public static string WithValue(string contents, string section, string key, string value)
    {
        Ensure.NotNull(contents);
        foreach (string token in new[] { section, key, value })
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            if (token.IndexOfAny(['\r', '\n', '[', ']', '=']) >= 0)
                throw new ArgumentException("An INI edit must contain a single section, key and value.");
        }
        _ = Parse(contents);
        string newline = contents.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = contents.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n').ToList();
        bool inSection = false, replaced = false;
        int sectionLine = -1;
        for (int index = 0; index < lines.Count; index++)
        {
            string line = lines[index].Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                inSection = line[1..^1].Trim().Equals(section, StringComparison.OrdinalIgnoreCase);
                if (inSection) sectionLine = index;
            }
            if (!inSection || line.StartsWith(';') || line.StartsWith('#')) continue;
            int equals = line.IndexOf('=');
            if (equals <= 0 || !line[..equals].Trim().Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
            lines[index] = key + "=" + value;
            replaced = true;
            break;
        }
        if (!replaced)
        {
            if (sectionLine >= 0) lines.Insert(sectionLine + 1, key + "=" + value);
            else { lines.Add("[" + section + "]"); lines.Add(key + "=" + value); }
        }
        string result = string.Join(newline, lines);
        _ = Parse(result);
        return result;
    }
}
