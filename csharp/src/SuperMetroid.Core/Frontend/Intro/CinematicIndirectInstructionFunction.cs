namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The bank-$8B functions a cinematic indirect instruction can name, as its first word.</summary>
internal enum CinematicIndirectInstructionFunction : ushort
{
    /// <summary><c>IndirectInstructionFunction_DrawToBG2Tilemap</c> at $8B:88FD, which draws the portrait tilemap.</summary>
    DrawToPortraitTilemap = 0x88fd,
}
