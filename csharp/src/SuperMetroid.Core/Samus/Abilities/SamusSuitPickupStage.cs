namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The seven stages of the suit pickup's light-beam transformation, run in this order once per update.</summary>
public enum SamusSuitPickupStage
{
    /// <summary>The narrow beam grows inward from the top and bottom screen edges until it meets at the centre.</summary>
    LightBeamAppears,

    /// <summary>The beam's left and right edges move apart uniformly.</summary>
    UniformWidening,

    /// <summary>The beam widens along the authored curve while the fixed colour brightens to white.</summary>
    CurvedWidening,

    /// <summary>The suit is equipped and revealed: suited pose, history and palette.</summary>
    RevealSuit,

    /// <summary>The full-screen beam is wiped from the top and bottom while the colour returns to the suit's tint.</summary>
    LightBeamShrinks,

    /// <summary>Only the centre scanline remains and narrows from both sides.</summary>
    LightBeamDissipates,

    /// <summary>The window and fixed colour are cleared and Samus's input is released.</summary>
    Finish,
}
