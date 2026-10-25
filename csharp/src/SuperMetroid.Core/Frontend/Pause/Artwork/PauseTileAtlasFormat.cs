using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Additional pause-menu characters, separate from the shared map atlas and its overrides.</summary>
public static class PauseTileAtlasFormat
{
    /// <summary>Asset filename for pause-menu characters 256..511, loaded with the same PNG geometry as the shared map atlas.</summary>
    public const string FileName = "pause-ui-tiles.png";
    /// <summary>8192 compiled bytes for the second 256-character half of the pause-menu background artwork.</summary>
    public const int ByteCount = MapTileAtlasFormat.ByteCount;
}
