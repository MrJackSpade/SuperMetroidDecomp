namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Eight native floor/ceiling plant block layouts at $84:9E0D..9F24. Their
/// block use consecutive tile pairs, horizontal mouth/foliage mirroring, and
/// a ceiling vertical flip. Three signed runs draw the right mouth, left mouth and
/// foliage. Idle alone restores the special-air parent and horizontal extension.
/// Native unused records after each idle are excluded. No stored draw cache remains;
/// replaceable assets continue to own the visible portion.
/// </summary>
internal static class SamusEaterPlmDrawDefinitions
{
    /// <summary>The visual block of the foliage's first tile at phase zero; each phase steps two blocks.</summary>
    internal const int FoliageFirstBlock = 0x180;
    /// <summary>The visual block of the mouth's first tile at phase zero; each phase steps two blocks.</summary>
    internal const int MouthFirstBlock = 0x1a0;
}
