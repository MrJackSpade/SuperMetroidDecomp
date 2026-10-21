using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Rooms;

/// <summary>How a generated boss BG2 catalog is queried and re-extracted (#633).</summary>
internal readonly record struct Bg2CatalogAccess
{
    /// <summary>Is frame.</summary>
    public required Func<EnemyFrameId, bool> IsFrame { get; init; }

    /// <summary>Extract.</summary>
    public required Func<byte[]> Extract { get; init; }
}
