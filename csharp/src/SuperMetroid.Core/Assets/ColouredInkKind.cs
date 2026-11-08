namespace SuperMetroid.Core.Assets;

/// <summary>How a pixel that has a colour of its image's shows it (#1544).</summary>
public enum ColouredInkKind
{
    /// <summary>
    /// The pixel shows what lies behind it, and its own colour only where its layer is drawn opaque, as gameplay's HUD band
    /// is: a clear pixel of a HUD or room effect picture.
    /// </summary>
    Clear,

    /// <summary>The pixel shows its colour and takes no part in colour blends.</summary>
    Opaque,

    /// <summary>
    /// The pixel shows its colour and takes part in colour blends where it is drawn as an actor's: a glow or tint that blends
    /// the actors shows over it, and a translucent actor's pixel blends with what lies behind it.
    /// </summary>
    Blending,
}
