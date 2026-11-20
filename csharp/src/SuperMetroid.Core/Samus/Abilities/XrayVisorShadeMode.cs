namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Which sequence of colours X-ray's suit colouring shows on the visor, or that X-ray has torn down and the colouring shows
/// the normal suit colours at its next update.
/// </summary>
public enum XrayVisorShadeMode
{
    /// <summary>The beam is widening: the visor steps through the widening colours and holds the last.</summary>
    Widening,

    /// <summary>The beam has reached full width: the visor cycles the full-beam colours.</summary>
    FullBeam,

    /// <summary>X-ray has torn down: the colouring's next update shows the normal suit shade and returns to widening.</summary>
    TornDown,
}
