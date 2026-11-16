namespace SuperMetroid.Core.Escape;

/// <summary>
/// Where an escape room's random explosions go off (#2287): outdoors every second update and only over scenery, the room
/// shaking on with them; indoors every fourth update anywhere on the screen. Its values are arbitrary.
/// </summary>
public enum EscapeExplosionSite
{
    /// <summary>Under Zebes's sky: an explosion every second update over scenery, the room's quake kept shaking.</summary>
    Outdoors,

    /// <summary>Inside: an explosion every fourth update anywhere on the screen.</summary>
    Indoors,
}
