using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.World.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Development-tool members of <see cref="EnemyFrameCartridgeLookup"/>: the cartridge word an enemy frame is stored at,
/// and the frame stored at a word, for tools and oracles that compare against cartridge data; never linked by player
/// hosts.
/// </summary>
internal static class EnemyFrameCartridgeLookupTooling
{
    /// <summary>$804D: the empty spritemap, which shows the empty frame of an enemy colliding by its radius, at the same word in every enemy bank.</summary>
    private const ushort EmptySpritemapWord = 0x804d;

    /// <summary>
    /// $804F: the empty extended frame, whose one point hitbox at the origin shows the empty frame of an enemy colliding by
    /// its frames' hitboxes, at the same word in every enemy bank.
    /// </summary>
    internal const ushort EmptyFrameHitboxWord = 0x804f;

    /// <summary>Each installed frame by where the cartridge stores it; the first composition in schema order wins a shared record.</summary>
    private static readonly Lazy<Dictionary<SnesAddress, EnemyFrameId>> FramesByLocation = new(Bind);

    /// <summary>Binds every installed composition's location, ordinary compositions first.</summary>
    private static Dictionary<SnesAddress, EnemyFrameId> Bind()
    {
        var frames = new Dictionary<SnesAddress, EnemyFrameId>();
        foreach (EnemyFrameName frame in EnemySpritemapDefinitions.Frames)
            frames.TryAdd(EnemyFrameCartridgeLookup.Of(frame.Frame), frame.Frame);
        foreach (EnemyFrameName frame in EnemyExtendedFrameDefinitions.Frames)
            frames.TryAdd(EnemyFrameCartridgeLookup.Of(frame.Frame), frame.Frame);
        return frames;
    }

    /// <summary>The bank-local word <paramref name="frame"/> is stored at for an enemy colliding by its radius.</summary>
    internal static ushort Word(EnemyFrameId frame) =>
        frame == EnemyFrameId.Empty ? EmptySpritemapWord : EnemyFrameCartridgeLookup.Of(frame).Offset;

    /// <summary>The bank-local word of the frame an enemy colliding by its radius shows, zero before it shows any.</summary>
    internal static ushort Word(EnemyFrameId? frame) => frame is { } shown ? Word(shown) : (ushort)0;

    /// <summary>The enemy bank holding <paramref name="frame"/>; the shared empty frames belong to no single bank.</summary>
    internal static byte Bank(EnemyFrameId frame) => EnemyFrameCartridgeLookup.Of(frame).Bank;

    /// <summary>
    /// The frame bank <paramref name="bank"/> stores at <paramref name="word"/>, or null when it stores none there. A
    /// frame the bank's own compositions name at a shared word is that frame; a shared word no composition names is the
    /// empty frame.
    /// </summary>
    internal static EnemyFrameId? TryFrameAt(byte bank, ushort word)
    {
        if (FramesByLocation.Value.TryGetValue(new SnesAddress(bank, word), out EnemyFrameId frame))
            return frame;
        return word is EmptySpritemapWord or EmptyFrameHitboxWord ? EnemyFrameId.Empty : null;
    }
}
