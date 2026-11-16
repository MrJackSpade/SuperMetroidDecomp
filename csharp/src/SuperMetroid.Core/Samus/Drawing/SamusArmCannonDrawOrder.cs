namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Where the pose's drawing mode places Samus's independent arm-cannon cover relative to her body.</summary>
public enum SamusArmCannonDrawOrder
{
    /// <summary>A pose that draws no cover: the cover is not drawn, even while a HUD-driven cover frame remains open.</summary>
    Hidden,
    /// <summary>Every other pose that draws the cover, except the forward-facing one: the cover is drawn before the body.</summary>
    BeforeBody,
    /// <summary>The forward-facing pose: the cover is drawn after the body.</summary>
    AfterBody,
}
