namespace SuperMetroid.Core.Frontend;

/// <summary>One bank-$8B title-card object definition.</summary>
/// <param name="Phase">Runtime title-card phase entered when this instruction list starts.</param>
/// <param name="InstructionAddress">Full 24-bit address of the bank-$8B text-object instruction list.</param>
/// <param name="Origin">Drawing origin in screen pixels.</param>
/// <param name="CharacterOffset">Native OBJ attribute/palette word applied to the composition; the legacy name does not denote a tile-count offset.</param>
public readonly record struct TitleTextSequenceDefinition(
    TitleSequencePhase Phase,
    int InstructionAddress,
    Position16<ScreenPixels> Origin,
    ushort CharacterOffset);
