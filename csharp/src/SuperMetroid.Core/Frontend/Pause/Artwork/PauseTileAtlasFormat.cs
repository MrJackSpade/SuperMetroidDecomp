using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Additional pause-menu characters, separate from the shared map atlas and its overrides.</summary>
public static class PauseTileAtlasFormat
{
    /// <summary>Asset filename for pause-menu characters 256..511, loaded with the same PNG geometry as the shared map atlas.</summary>
    public const string FileName = "pause-ui-tiles.png";
    /// <summary>$B6:A000, characters 256..511 in GameState_13's background-character transfer.</summary>
    public const int SourceAddress = MapTileAtlasRomData.PauseInterfaceCharacterData;
    /// <summary>Byte $2000 immediately follows the shared map characters in pause VRAM.</summary>
    public const int DestinationByte = MapTileAtlasFormat.ByteCount;
    /// <summary>8192 compiled bytes for the second 256-character half of the pause-menu background artwork.</summary>
    public const int ByteCount = MapTileAtlasFormat.ByteCount;
}
