namespace SuperMetroid.Core.Game;

/// <summary>Calculated radial approach for Draygon's six death/burial Evir actors.</summary>
internal static class DraygonBurialEvirDefinitions
{
    /// <summary>$A5:A1DF DraygonDeathSequenceEvirAngles: first actor starts at angle $68, followed by sixteenth-turn steps.</summary>
    private const int FirstAngle = 0x68;
    /// <summary>$A5:A1C7 DraygonDeathSequenceEvirSpawnPositions: circle centered at room X256, Y512.</summary>
    private const int CenterX = 256, CenterY = 512;
    /// <summary>Radius of the six original spawn positions in pixels, four pixels inside the 512-pixel extent.</summary>
    private const int Radius = 508;

    /// <summary>
    /// $A5:A1AF subspeeds are the absolute unit-circle components scaled by $FFFF
    /// and truncated. $A5:A1C7 positions use the same six angles on the radius508
    /// circle, floor to pixels, then wrap signed X into the native word. The angle
    /// at $A5:A1DF selects the movement signs; its unused zero padding is not state.
    /// </summary>
    internal static DraygonBurialEvirDefinition ForEntry(int entry)
    {
        if ((uint)entry >= 6)
            throw new InvalidDataException($"Draygon burial Evir entry {entry} exceeds its six-record table.");
        int angle = FirstAngle - 0x10 * entry;
        double radians = angle * Math.PI / 128;
        double x = Math.Cos(radians), y = Math.Sin(radians);
        return new((ushort)Math.Floor(ushort.MaxValue * Math.Abs(x)),
            (ushort)Math.Floor(ushort.MaxValue * y),
            unchecked((ushort)(int)Math.Floor(CenterX + Radius * x)),
            (ushort)Math.Floor(CenterY - Radius * y), (ushort)angle);
    }
}

/// <summary>One compiled Draygon death/burial Evir position and movement record.</summary>
internal readonly record struct DraygonBurialEvirDefinition(
    ushort XSubspeed,
    ushort YSubspeed,
    ushort InitialX,
    ushort InitialY,
    ushort Angle);
