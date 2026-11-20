namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Phantoon's translucent body (#1514): the background his body shows on, lying behind the room, brightens the foreground,
/// the actors taking part in blends and the backdrop wherever it shows, without ever covering the foreground.
/// </summary>
public sealed record PhantoonTranslucenceBlend : GameplayBlend
{
    /// <summary>The one translucent body.</summary>
    public static PhantoonTranslucenceBlend Body { get; } = new();

    /// <summary>Only <see cref="Body"/> exists.</summary>
    private PhantoonTranslucenceBlend()
    {
    }
}
