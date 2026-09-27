namespace SuperMetroid.Core.Assets;

/// <summary>Dead Torizo's editable 4bpp source sheet and its cartridge identity.</summary>
internal static class DeadTorizoArtworkDefinitions
{
    /// <summary>$B7:A800, the 192-tile source sheet used both for the initial corpse and falling sand.</summary>
    internal const int SourceAddress = 0xb7a800;

    /// <summary>$1800 bytes, the full tile sheet named enemy-ed3f-tiles.png in the installation.</summary>
    internal const int ByteCount = 0x1800;
}
