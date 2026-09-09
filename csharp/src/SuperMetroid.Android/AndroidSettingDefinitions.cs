using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Android;

/// <summary>Supported Android controls for the shared host INI, not separate cheat implementations.</summary>
internal sealed record AndroidSettingDefinition(string Label, string Section, string Key,
    string[] Values, Func<SuperMetroidGameOptions, string> Read);

internal static class AndroidSettingDefinitions
{
    internal static readonly AndroidSettingDefinition[] All =
    [
        new("Ending time override (minutes)", "Game", "EndingTimeOverrideMinutes", ["None", "0", "180", "600"], o => o.EndingTimeOverrideMinutes?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "None"),
        new("Escape countdown (minimum 1 second)", "Game", "PreventEscapeTimeout", ["false", "true"], o => o.PreventEscapeTimeout.ToString().ToLowerInvariant()),
        new("Invincibility (minimum 1 energy)", "Game", "Invincibility", ["false", "true"], o => o.Invincibility.ToString().ToLowerInvariant()),
        new("Infinite ammo (unlocked types only)", "Game", "InfiniteAmmo", ["false", "true"], o => o.InfiniteAmmo.ToString().ToLowerInvariant()),
        new("Map reveal (temporary visibility)", "Game", "MapReveal", ["None", "Public", "Secret"], o => o.MapReveal.ToString()),
        new("Skip opening cinematic", "Game", "SkipOpeningCinematic", ["false", "true"], o => o.SkipOpeningCinematic.ToString().ToLowerInvariant()),
        new("Audio enabled", "Audio", "Enabled", ["false", "true"], o => o.AudioEnabled.ToString().ToLowerInvariant()),
        new("Volume percent", "Audio", "MasterVolumePercent", Enumerable.Range(0, 101).Select(i => i.ToString()).ToArray(), o => o.MasterVolumePercent.ToString()),
    ];
}
