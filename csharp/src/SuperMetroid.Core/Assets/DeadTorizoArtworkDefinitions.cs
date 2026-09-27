namespace SuperMetroid.Core.Assets;

/// <summary>Dead Torizo's editable 4bpp source sheet and its cartridge identity.</summary>
internal static class DeadTorizoArtworkDefinitions
{
    /// <summary>$A9, the bank containing Dead Torizo's private corpse OAM map.</summary>
    internal const byte SpritemapBank = 0xa9;

    /// <summary>$A9:D761, the fixed corpse map drawn by the $A9:D39A hook.</summary>
    internal const ushort HookSpritemap = 0xd761;

    internal static EnemySpritemapDefinition[] Frames() =>
        [new(SpritemapBank, HookSpritemap, "dead_torizo_corpse_a9_d761")];

    /// <summary>$B7:A800, the 192-tile source sheet used both for the initial corpse and falling sand.</summary>
    internal const int SourceAddress = 0xb7a800;

    /// <summary>$1800 bytes, the full tile sheet named enemy-ed3f-tiles.png in the installation.</summary>
    internal const int ByteCount = 0x1800;
}
