using System.Globalization;
using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The names and numbers the Godot game's data is written in.</summary>
internal static partial class EngineNames
{
    /// <summary>
    /// The name the Godot game asks for one of the old game's sounds by: which of the three sound libraries it is in,
    /// and its name there, in lower case with underscores. A request that only stops a sound has no name.
    /// </summary>
    internal static string? Sound(SoundEffectId sound) => sound switch
    {
        SoundEffectId.OfSamus samus => "samus_" + EngineSheets.FileName(samus.Sound.ToString()),
        SoundEffectId.OfEnemies enemies => "enemy_" + EngineSheets.FileName(enemies.Sound.ToString()),
        SoundEffectId.OfRoom room => "room_" + EngineSheets.FileName(room.Sound.ToString()),
        _ => null,
    };

    /// <summary>The cue that asks for one of the old game's sounds, or none for a request that only stops one.</summary>
    internal static string? SoundCue(SoundEffectId sound) => Sound(sound) is { } name ? "sound:" + name : null;

    /// <summary>The cue that makes an enemy go on behaving however far from the view it is.</summary>
    internal const string AlwaysActive = "always_active";

    /// <summary>The cue that ends <see cref="AlwaysActive"/>.</summary>
    internal const string NotAlwaysActive = "not_always_active";

    /// <summary>A side, as the Godot game's cues name it.</summary>
    internal static string Side(HorizontalDirection direction) => direction == HorizontalDirection.Left ? "left" : "right";

    /// <summary>A distance or speed as a plain number of pixels, as the Godot game's data writes one.</summary>
    internal static JsonNode Number(SignedSixteenSixteen value) => JsonValue.Create(value.Pixels);

    /// <summary>A distance or speed as a plain number of pixels.</summary>
    internal static JsonNode Number(SignedEightEight value) => JsonValue.Create(value.Pixels);

    /// <summary>A distance or speed as it is written inside a cue.</summary>
    internal static string InCue(SignedSixteenSixteen value) => value.Pixels.ToString(CultureInfo.InvariantCulture);
}
