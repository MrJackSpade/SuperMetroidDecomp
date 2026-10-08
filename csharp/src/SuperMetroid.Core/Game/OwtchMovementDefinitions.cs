namespace SuperMetroid.Core.Game;

/// <summary>Compiled NTSC patrol and underground-wait rules for Owtch.</summary>
internal static class OwtchMovementDefinitions
{

    /// <summary>Returns 16*(index+1) for patrol selector 0..7.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all eight NTSC J/U v1.0 words and
    /// pinned bank_A2.asm. The high byte of parameter two selects an undoubled index;
    /// native doubles it for word access, then adds/subtracts the distance with word
    /// wrap. Only definition selection changes here; callers retain that wrapping.
    /// </remarks>
    internal static ushort TravelDistance(byte index)
    {
        if (index >= 8)
            throw new InvalidDataException(
                $"Owtch travel-distance index {index} is outside the eight authored values.");
        return (ushort)(16 * (index + 1));
    }

    /// <summary>Returns 32*(index+1) for NTSC burial selector 0..5.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all six NTSC J/U v1.0 words and
    /// pinned bank_A2.asm, whose !FPS=1 definitions explicitly multiply this sequence.
    /// Parameter one's high byte selects the index, also used when reloading after
    /// sinking. Keep its shorter six-entry bound independent of the patrol domain;
    /// this is not the PAL timer contract and no input clamping is permitted.
    /// </remarks>
    internal static ushort UndergroundTimer(byte index)
    {
        if (index >= 6)
            throw new InvalidDataException(
                $"Owtch underground-timer index {index} is outside the six authored values.");
        return (ushort)(32 * (index + 1));
    }
}
