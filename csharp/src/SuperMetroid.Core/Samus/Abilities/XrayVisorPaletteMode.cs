namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Which visor-colour sequence the X-ray palette handler runs, and whether teardown has asked it to restore the
/// normal suit palette.
/// </summary>
public enum XrayVisorPaletteMode
{
    /// <summary>The beam is widening: the visor steps through the three widening colours and holds the last.</summary>
    Widening,

    /// <summary>The beam has reached full width: the visor cycles the three full-beam colours.</summary>
    FullBeam,

    /// <summary>X-ray has torn down: the next palette call restores the normal suit palette and clears this mode.</summary>
    RestoreSuitPalette,
}
