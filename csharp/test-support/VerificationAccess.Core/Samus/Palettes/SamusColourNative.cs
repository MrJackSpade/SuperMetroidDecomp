namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Verification's native view of where Samus's colours lie in CGRAM, for checks that compare a whole native colour
/// image with gameplay's.
/// </summary>
internal static class SamusColourNative
{
    /// <summary>The CGRAM colour of Samus's first suit colour, OBJ palette four's first.</summary>
    public const int SuitFirstCgram = 0xc0;

    /// <summary>The CGRAM colour of Samus's first beam colour, OBJ palette six's first.</summary>
    public const int BeamFirstCgram = 0xe0;
}
