namespace SuperMetroid.Core.Assets;

/// <summary>Dead Torizo's editable 4bpp source sheet and its cartridge identity.</summary>
internal static class DeadTorizoArtworkDefinitions
{
    /// <summary>$A9, the bank containing Dead Torizo's private corpse OAM map.</summary>
    internal const byte SpritemapBank = 0xa9;

    /// <summary>$A9:D761, the fixed corpse map drawn by the $A9:D39A hook.</summary>
    internal const ushort HookSpritemap = 0xd761;

    /// <summary>Returns the compiled OAM map used for Dead Torizo's initial corpse pose.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        [new(SpritemapBank, HookSpritemap, "dead_torizo_corpse_a9_d761")];

    /// <summary>$A9:D6DE, the stationary corpse-list's sole visual selector.</summary>
    internal const ushort StationaryOperand = 0xd6de;

    /// <summary>$A9:D6E2, the selected 25-part stationary corpse OAM composition.</summary>
    internal const ushort StationarySpritemap = 0xd6e2;

    // Appended separately from Frames() so version-53 visual overrides retain
    // the exact 1,008-frame prefix they originally authored.
    /// <summary>Returns the stationary corpse map as a separate visual frame definition.</summary>
    internal static EnemySpritemapDefinition[] StationaryFrames() =>
        [new(SpritemapBank, StationarySpritemap, "dead_torizo_stationary_a9_d6e2")];

    /// <summary>Resolves the stationary corpse's selector operand to its compiled OAM map.</summary>
    /// <param name="operandAddress">The bank-local operand address read from the stationary corpse list.</param>
    /// <returns>The stationary corpse spritemap address when the operand matches the compiled selector.</returns>
    /// <exception cref="InvalidDataException">The operand does not identify the compiled Dead Torizo selector.</exception>
    internal static ushort StationaryFrameAt(ushort operandAddress) =>
        operandAddress == StationaryOperand
            ? StationarySpritemap
            : throw new InvalidDataException(
                $"Dead Torizo visual selector $A9:{operandAddress:X4} is not compiled.");

    /// <summary>$B7:A800, the 192-tile source sheet used both for the initial corpse and falling sand.</summary>
    internal const int SourceAddress = 0xb7a800;

    /// <summary>$1800 bytes, the full tile sheet named enemy-ed3f-tiles.png in the installation.</summary>
    internal const int ByteCount = 0x1800;
}
