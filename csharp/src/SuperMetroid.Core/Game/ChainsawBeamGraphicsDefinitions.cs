using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Bounded adjacent-table and instruction data consumed by Chainsaw beam selection.</summary>
internal static class ChainsawBeamGraphicsDefinitions
{
    /// <summary>$000D: Plasma/Spazer/Wave indexes two words beyond the twelve-entry tile table at $90:C3B1.</summary>
    internal const int Selection = 0x0d;
    /// <summary>$9A:C421: $90:C3CB (the second palette-pointer word) reinterpreted by $90:AC8D as the tile pointer.</summary>
    internal const int TileSource = 0x9ac421;
    /// <summary>$90:7FFF: $90:C3E3 (Power palette color one) reinterpreted by $90:ACCD as the color source.</summary>
    internal const ushort PalettePointer = 0x7fff;
    /// <summary>$90:8000..801E, the Samus_Animate instruction prefix read after the first open-bus byte.</summary>
    internal const int InstructionAddress = 0x908000;
    /// <summary>Exact executable bytes of Samus_Animate, not an authored palette or substitute color list.</summary>
    internal static ReadOnlySpan<byte> InstructionBytes =>
    [
        0x08, 0xc2, 0x30, 0x22, 0x58, 0xec, 0x90, 0xad,
        0x6e, 0x19, 0x29, 0x0f, 0x00, 0xaa, 0xfc, 0x67,
        0x80, 0xad, 0x1c, 0x0a, 0xc9, 0x4d, 0x00, 0xf0,
        0x19, 0xc9, 0x4e, 0x00, 0xf0, 0x14, 0xad,
    ];

    internal static void LoadPalette(ISnesAddressSpace bus, SnesCgram cgram)
    {
        for (ushort color = 0; color < Assets.BeamPaletteDefinitions.ColorCount; color++)
            // The glitched pointer reads arbitrary words; the CGRAM port keeps fifteen bits.
            cgram.SetColor(SamusProjectileRomData.Palettes.BeamDestinationIndex + color,
                Bgr555.FromCgramPortWord(SnesIndirectLongDataRead.ReadWord(bus, 0x90, PalettePointer, (ushort)(color * 2),
                    InstructionBytes, InstructionAddress)));
    }
}