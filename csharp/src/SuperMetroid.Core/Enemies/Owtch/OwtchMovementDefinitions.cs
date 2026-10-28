namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>Compiled NTSC patrol and underground-wait rules for Owtch.</summary>
internal static class OwtchMovementDefinitions
{

    /// <summary>The patrol distance, 16*(index+1), for patrol selector 0..7.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all eight NTSC J/U v1.0 words and
    /// pinned bank_A2.asm. The high byte of parameter two selects an undoubled index;
    /// native doubles it for word access, then adds/subtracts the distance with word
    /// wrap. Callers retain that wrapping.
    /// </remarks>
    internal static readonly ushort[] TravelDistances = [16, 32, 48, 64, 80, 96, 112, 128];

    /// <summary>The underground wait, 32*(index+1), for NTSC burial selector 0..5.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all six NTSC J/U v1.0 words and
    /// pinned bank_A2.asm, whose !FPS=1 definitions explicitly multiply this sequence.
    /// Parameter one's high byte selects the index, also used when reloading after
    /// sinking. Its shorter six-entry bound is independent of the patrol domain;
    /// this is not the PAL timer contract and no input clamping is permitted.
    /// </remarks>
    internal static readonly ushort[] UndergroundTimers = [32, 64, 96, 128, 160, 192];

    /// <summary>The random bit that sends a surfacing Owtch right rather than left: bit eight.</summary>
    internal static readonly RandomSampleField MoveRightChoice = new(8, 1);
}
