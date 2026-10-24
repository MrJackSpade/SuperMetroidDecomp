namespace SuperMetroid.Android.Diagnostics;

/// <summary>Menu choices that read and edit the same INI settings used by the shared game host.</summary>
internal static class AndroidSettingDefinitions
{
    /// <summary>Defines the editable game and audio options, their allowed menu values, and their current-value readers.</summary>
    internal static readonly AndroidSettingDefinition[] All =
    [
        new("Door-transition autosaves", "Game", "DoorTransitionAutosave", ["true", "false"], o => o.DoorTransitionAutosave.ToString().ToLowerInvariant()),
        new("Ending time override (minutes)", "Game", "EndingTimeOverrideMinutes", ["None", "0", "180", "600"], o => o.Game.EndingTimeOverrideMinutes?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "None"),
        new("Escape countdown (minimum 1 second)", "Game", "PreventEscapeTimeout", ["false", "true"], o => o.Game.PreventEscapeTimeout.ToString().ToLowerInvariant()),
        new("Invincibility (minimum 1 energy)", "Game", "Invincibility", ["false", "true"], o => o.Game.Invincibility.ToString().ToLowerInvariant()),
        new("Infinite ammo (unlocked types only)", "Game", "InfiniteAmmo", ["false", "true"], o => o.Game.InfiniteAmmo.ToString().ToLowerInvariant()),
        new("Map reveal (temporary visibility)", "Game", "MapReveal", ["None", "Public", "Secret"], o => o.Game.MapReveal.ToString()),
        new("Skip opening cinematic", "Game", "SkipOpeningCinematic", ["false", "true"], o => o.Game.SkipOpeningCinematic.ToString().ToLowerInvariant()),
        new("Audio enabled", "Audio", "Enabled", ["false", "true"], o => o.AudioEnabled.ToString().ToLowerInvariant()),
        new("Volume percent", "Audio", "MasterVolumePercent", Enumerable.Range(0, 101).Select(i => i.ToString()).ToArray(), o => o.MasterVolumePercent.ToString()),
    ];
}
