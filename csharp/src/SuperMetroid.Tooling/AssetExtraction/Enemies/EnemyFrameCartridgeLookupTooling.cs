using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Development-tool members of <see cref="EnemyFrameCartridgeLookup"/>: the cartridge word an enemy frame is stored at,
/// and the frame stored at a word, for tools and oracles that compare against cartridge data; never linked by player
/// hosts.
/// </summary>
internal static class EnemyFrameCartridgeLookupTooling
{
    /// <summary>$804D and $804F: the shared empty spritemap and extended frame, repeated at the same word in every enemy bank.</summary>
    private static readonly Dictionary<EnemyFrameId, ushort> SharedWords = new()
    {
        [EnemyFrameId.Empty] = 0x804d,
        [EnemyFrameId.EmptyExtended] = 0x804f,
    };

    /// <summary>Each installed frame by where the cartridge stores it; the first composition in schema order wins a shared record.</summary>
    private static readonly Lazy<Dictionary<SnesAddress, EnemyFrameId>> FramesByLocation = new(Bind);

    /// <summary>Binds every installed composition's location, ordinary compositions first.</summary>
    private static Dictionary<SnesAddress, EnemyFrameId> Bind()
    {
        var frames = new Dictionary<SnesAddress, EnemyFrameId>();
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
            frames.TryAdd(EnemyFrameCartridgeLookup.Of(frame.Frame), frame.Frame);
        foreach (EnemySpritemapDefinition frame in EnemyExtendedFrameDefinitions.Frames)
            frames.TryAdd(EnemyFrameCartridgeLookup.Of(frame.Frame), frame.Frame);
        return frames;
    }

    /// <summary>The bank-local word <paramref name="frame"/> is stored at.</summary>
    internal static ushort Word(EnemyFrameId frame) =>
        SharedWords.TryGetValue(frame, out ushort word) ? word : EnemyFrameCartridgeLookup.Of(frame).Offset;

    /// <summary>The bank-local word of the frame an enemy shows, zero before it shows any.</summary>
    internal static ushort Word(EnemyFrameId? frame) => frame is { } shown ? Word(shown) : (ushort)0;

    /// <summary>The enemy bank holding <paramref name="frame"/>; the shared empty frames belong to no single bank.</summary>
    internal static byte Bank(EnemyFrameId frame) => EnemyFrameCartridgeLookup.Of(frame).Bank;

    /// <summary>The frame bank <paramref name="bank"/> stores at <paramref name="word"/>, or null when it stores none there.</summary>
    internal static EnemyFrameId? TryFrameAt(byte bank, ushort word)
    {
        foreach ((EnemyFrameId shared, ushort sharedWord) in SharedWords)
            if (sharedWord == word)
                return shared;
        return FramesByLocation.Value.TryGetValue(new SnesAddress(bank, word), out EnemyFrameId frame) ? frame : null;
    }
}
