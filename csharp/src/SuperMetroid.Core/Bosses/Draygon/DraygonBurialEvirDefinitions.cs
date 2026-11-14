namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Calculated radial approach for Draygon's six death/burial Evir actors.</summary>
internal static class DraygonBurialEvirDefinitions
{
    /// <summary>The number of burial Evir actors.</summary>
    internal const int EntryCount = 6;
    /// <summary>The leftmost burial Evir's angle, the largest of the six.</summary>
    internal const int FirstAngle = 104;
    /// <summary>Neighbouring Evirs' angles are a sixteenth turn apart.</summary>
    internal const int AngleStep = 16;
    /// <summary>The burial Evirs spawn on a circle centered at room X256, Y512.</summary>
    internal const int CenterX = 256, CenterY = 512;
    /// <summary>Radius of the six original spawn positions in pixels, four pixels inside the 512-pixel extent.</summary>
    internal const int Radius = 508;
    /// <summary>
    /// The scale the authored subspeeds apply to the unit circle's components: the largest fraction of a pixel, one
    /// 65536th short of a whole pixel, so a full component stays a fraction.
    /// </summary>
    internal const int SubspeedScale = FractionUnitsPerPixel - 1;

    /// <summary>The fractions of a pixel a subspeed counts in: 65536 to the pixel.</summary>
    private const int FractionUnitsPerPixel = 1 << 16;
}
