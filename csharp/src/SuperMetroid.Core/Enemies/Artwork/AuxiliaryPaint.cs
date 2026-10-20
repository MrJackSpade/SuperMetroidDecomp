namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Which calculated paint progression an auxiliary enemy palette family follows.</summary>
internal enum AuxiliaryPaint
{
    /// <summary>The face block's glow cycle.</summary>
    FaceGlow,

    /// <summary>A dead sidehopper drained by the Shitroid, ending at its corpse colours.</summary>
    SidehopperDrain,

    /// <summary>Golden Torizo's front-body health gradient.</summary>
    TorizoFront,

    /// <summary>Golden Torizo's rear-body health gradient.</summary>
    TorizoRear,
}
