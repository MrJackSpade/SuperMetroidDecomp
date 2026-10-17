namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Where the pose's drawing mode places Samus's independent arm-cannon cover relative to her body.</summary>
public enum SamusArmCannonDrawOrder
{
    /// <summary>A zero mode: the cover is not drawn, even while a HUD-driven cover frame remains open.</summary>
    Hidden,
    /// <summary>Every nonzero mode other than the after-body mode: the cover is drawn before the body.</summary>
    BeforeBody,
    /// <summary>The forward-facing mode: the cover is drawn after the body.</summary>
    AfterBody,
}
