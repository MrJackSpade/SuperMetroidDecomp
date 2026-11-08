namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>How the room shows Phantoon's body, which lies on the room's background (#1545).</summary>
/// <remarks>The values are arbitrary.</remarks>
public enum PhantoonBodyBlend
{
    /// <summary>The body shows as the room shows its background.</summary>
    Room,

    /// <summary>The body is hidden: the room shows no background.</summary>
    Hidden,

    /// <summary>The body is translucent: the background lies behind the room and brightens what it shows through.</summary>
    Translucent,
}
