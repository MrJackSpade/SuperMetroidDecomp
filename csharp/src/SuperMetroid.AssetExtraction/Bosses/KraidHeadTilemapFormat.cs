using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Supported JSON version and fixed head-frame dimensions matching the $0160-word native copy at $A7:AF3D.</summary>
public static partial class KraidHeadTilemapFormat
{
    /// <summary>Supported Kraid head tilemap schema revision one.</summary>
    public const int Version = 1;
    /// <summary>Thirty-two tile-reference columns in each head-frame row.</summary>
    public const int Width = 32;
    /// <summary>Eleven tile-reference rows in each head frame; with <see cref="Width"/> this yields 352 native words.</summary>
    public const int Height = 11;
    /// <summary>The four head poses whose layouts are extracted as import data (#1638), from closed to fully open, in the cartridge's order.</summary>
    internal static IReadOnlyList<KraidHeadFrame> HeadsByMouthStage { get; } =
        [KraidHeadFrame.Closed, KraidHeadFrame.Opening, KraidHeadFrame.Wider, KraidHeadFrame.Open];

    /// <summary>The written-out key the import's head file and manifest entry name each head pose by (#2464, #2556).</summary>
    private static readonly Dictionary<KraidHeadFrame, string> ImportKeys = new()
    {
        [KraidHeadFrame.Closed] = "closed",
        [KraidHeadFrame.Opening] = "opening",
        [KraidHeadFrame.Wider] = "wider",
        [KraidHeadFrame.Open] = "open",
    };

    /// <summary>The key the import's head file and manifest entry name <paramref name="head"/> by.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="head"/> is no head pose the import extracts.</exception>
    internal static string ImportKey(KraidHeadFrame head) =>
        ImportKeys.TryGetValue(head, out string? key) ? key : throw new ArgumentOutOfRangeException(nameof(head), head, "Kraid's head has no such pose.");
}
