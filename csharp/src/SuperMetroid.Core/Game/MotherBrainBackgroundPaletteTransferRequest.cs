namespace SuperMetroid.Core.Game;

/// <summary>One phase-three room-light palette pair selected by `$AD:F24B`.</summary>
public readonly record struct MotherBrainBackgroundPaletteTransferRequest(
    ushort PaletteIndex,
    uint SourceAddress,
    ushort FirstDestinationColorIndex,
    ushort SecondDestinationColorIndex,
    ushort ColorsPerDestination);
