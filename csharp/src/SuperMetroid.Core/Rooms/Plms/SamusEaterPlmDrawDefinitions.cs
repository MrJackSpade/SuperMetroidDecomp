namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Eight floor/ceiling plant block layouts. Their
/// block use consecutive tile pairs, horizontal mouth/foliage mirroring, and
/// a ceiling vertical flip. Three signed runs draw the right mouth, left mouth and
/// foliage. Idle alone restores the special-air parent and horizontal extension.
/// Unused records after each idle are excluded. No stored draw cache remains;
/// replaceable assets continue to own the visible portion.
/// </summary>
internal static class SamusEaterPlmDrawDefinitions
{
    /// <summary>The visual block of the foliage's first tile at phase zero; each phase steps two blocks.</summary>
    internal const int FoliageFirstBlock = 384;
    /// <summary>The visual block of the mouth's first tile at phase zero; each phase steps two blocks.</summary>
    internal const int MouthFirstBlock = 416;
}
