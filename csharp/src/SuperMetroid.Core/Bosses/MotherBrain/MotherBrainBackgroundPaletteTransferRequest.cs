namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One phase-three room-light palette pair selected by `$AD:F24B`.</summary>
public readonly record struct MotherBrainBackgroundPaletteTransferRequest(
    int PaletteIndex,
    uint SourceAddress,
    int FirstDestinationColorIndex,
    int SecondDestinationColorIndex,
    int ColorsPerDestination);
