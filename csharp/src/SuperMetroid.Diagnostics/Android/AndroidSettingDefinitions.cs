using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Android;

/// <summary>Supported Android controls for the shared host INI, not separate cheat implementations.</summary>
/// <param name="Label">User-facing setting label displayed by the Android host.</param>
/// <param name="Section">Shared INI section containing the setting.</param>
/// <param name="Key">Shared INI key updated by the control.</param>
/// <param name="Values">Allowed serialized values offered by the control.</param>
/// <param name="Read">Reads the current serialized value from the shared game options.</param>
internal sealed record AndroidSettingDefinition(string Label, string Section, string Key,
    string[] Values, Func<SuperMetroidGameOptions, string> Read);

/// <summary>Catalogs the Android controls that edit shared host INI settings.</summary>
internal static class AndroidSettingDefinitions
{
    /// <summary>Settings exposed by Android controls, mapped to the shared host INI values.</summary>
    internal static readonly AndroidSettingDefinition[] All =
    [
        new("Door-transition autosaves", "Game", "DoorTransitionAutosave", ["true", "false"], o => o.DoorTransitionAutosave.ToString().ToLowerInvariant()),
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
