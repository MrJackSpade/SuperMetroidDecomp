namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The runs of Kraid's hurt flash (#2400): the flash a mouth hit plays, and the rest it ends on.</summary>
internal enum KraidHurtFlashSegment
{
    /// <summary>
    /// A mouth hit's flash: his hurt colours and his band's colours by turns, three times each, ending on his band's
    /// colours for good.
    /// </summary>
    Flashing,

    /// <summary>No flash: his band's colours, shown for good until a mouth hit flashes him again.</summary>
    Settled,
}
