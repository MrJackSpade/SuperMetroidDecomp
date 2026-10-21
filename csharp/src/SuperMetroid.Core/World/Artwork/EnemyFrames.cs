using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>
/// The presentation boundary of enemy frames: the installed spritemap or extended frame an <see cref="EnemyFrameId"/>
/// shows, bound to the installed schemas by composition name.
/// </summary>
internal sealed class EnemyFrames : IEnemyFrameWords
{
    /// <summary>The binding of the installed enemy-bank frame schema.</summary>
    internal static EnemyFrames Installed { get; } = new();

    /// <summary>Each catalogued frame's installed spritemap or extended-frame word.</summary>
    private static readonly Lazy<Dictionary<EnemyFrameId, ushort>> Words = new(Bind);

    /// <summary>Each installed composition's enemy bank; the shared empty frames have a copy in every supported bank.</summary>
    private static readonly Lazy<Dictionary<EnemyFrameId, byte>> Banks = new(BindBanks);

    /// <summary>Binds the shared empty frames, then every installed composition to the identity its name gives.</summary>
    private static Dictionary<EnemyFrameId, ushort> Bind()
    {
        var words = new Dictionary<EnemyFrameId, ushort>
        {
            [EnemyFrameId.Empty] = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap,
            [EnemyFrameId.EmptyExtended] = CommonEnemyEmptyExtendedFrameDefinitions.Frame,
        };
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
            words.Add(Identity(frame.Name), frame.Pointer);
        foreach (EnemySpritemapDefinition frame in EnemyExtendedFrameDefinitions.Frames)
            words.Add(Identity(frame.Name), frame.Pointer);
        return words;
    }

    /// <summary>Binds every installed composition's identity to its enemy bank.</summary>
    private static Dictionary<EnemyFrameId, byte> BindBanks()
    {
        var banks = new Dictionary<EnemyFrameId, byte>();
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
            banks.Add(Identity(frame.Name), frame.Bank);
        foreach (EnemySpritemapDefinition frame in EnemyExtendedFrameDefinitions.Frames)
            banks.Add(Identity(frame.Name), frame.Bank);
        return banks;
    }

    /// <summary>The enemy bank holding installed composition <paramref name="frame"/>.</summary>
    public byte Bank(EnemyFrameId frame) => Banks.Value.TryGetValue(frame, out byte bank)
        ? bank
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "A shared empty frame belongs to no single bank.");

    /// <summary>The identity named after installed composition <paramref name="name"/>.</summary>
    private static EnemyFrameId Identity(string name) =>
        Enum.Parse<EnemyFrameId>(string.Concat(name.Split('_').Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..])));

    /// <summary>The bank-local word of the frame an enemy shows, zero before it shows any.</summary>
    public ushort Spritemap(EnemyFrameId? frame) => frame is { } shown ? Spritemap(shown) : (ushort)0;

    /// <summary>The bank-local word of the spritemap or extended frame <paramref name="frame"/> shows, in the enemy's bank.</summary>
    public ushort Spritemap(EnemyFrameId frame) => Words.Value.TryGetValue(frame, out ushort word)
        ? word
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "No installed composition shows this enemy frame.");
}
