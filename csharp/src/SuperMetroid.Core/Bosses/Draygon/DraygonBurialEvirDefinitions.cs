namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Calculated radial approach for Draygon's six death/burial Evir actors.</summary>
internal static class DraygonBurialEvirDefinitions
{
    /// <summary>The number of burial Evir actors.</summary>
    internal const int EntryCount = 6;
    /// <summary>$A5:A1DF DraygonDeathSequenceEvirAngles: first actor starts at angle $68, followed by sixteenth-turn steps.</summary>
    internal const int FirstAngle = 0x68;
    /// <summary>Each following actor's angle is a sixteenth turn below the previous one's.</summary>
    internal const int AngleStep = 0x10;
    /// <summary>$A5:A1C7 DraygonDeathSequenceEvirSpawnPositions: circle centered at room X256, Y512.</summary>
    internal const int CenterX = 256, CenterY = 512;
    /// <summary>Radius of the six original spawn positions in pixels, four pixels inside the 512-pixel extent.</summary>
    internal const int Radius = 508;
}
