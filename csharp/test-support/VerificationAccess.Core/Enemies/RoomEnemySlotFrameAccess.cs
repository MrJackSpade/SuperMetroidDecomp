using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification's native view of the frame an enemy shows: the bank-local spritemap or extended-frame word, read through
/// the presentation boundary and written back by finding the installed composition at that word in the enemy's bank.
/// </summary>
internal static class RoomEnemySlotFrameAccess
{
    /// <summary>Each installed composition's identity, by bank and word.</summary>
    private static readonly Lazy<Dictionary<(byte Bank, ushort Word), EnemyFrameId>> Identities = new(Bind);

    /// <summary>Binds every installed composition's bank and word to the identity its name gives.</summary>
    private static Dictionary<(byte Bank, ushort Word), EnemyFrameId> Bind()
    {
        var identities = new Dictionary<(byte Bank, ushort Word), EnemyFrameId>();
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
            identities.TryAdd((frame.Bank, frame.Pointer), Identity(frame.Name));
        foreach (EnemySpritemapDefinition frame in EnemyExtendedFrameDefinitions.Frames)
            identities.TryAdd((frame.Bank, frame.Pointer), Identity(frame.Name));
        return identities;
    }

    /// <summary>The identity named after installed composition <paramref name="name"/>.</summary>
    private static EnemyFrameId Identity(string name) =>
        Enum.Parse<EnemyFrameId>(string.Concat(name.Split('_').Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..])));

    /// <summary>Whether an installed composition shows native word <paramref name="word"/> in bank <paramref name="bank"/>.</summary>
    internal static bool Shows(byte bank, ushort word) => Identities.Value.ContainsKey((bank, word));

    /// <summary>The frame shown by native word <paramref name="word"/> in bank <paramref name="bank"/>; zero shows none.</summary>
    internal static EnemyFrameId? FrameAt(byte bank, ushort word) => word switch
    {
        0 => null,
        CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap => EnemyFrameId.Empty,
        CommonEnemyEmptyExtendedFrameDefinitions.Frame => EnemyFrameId.EmptyExtended,
        _ => Identities.Value.TryGetValue((bank, word), out EnemyFrameId frame)
            ? frame
            : throw new ArgumentOutOfRangeException(nameof(word), word, $"No installed enemy frame at ${bank:X2}:{word:X4}."),
    };

    /// <summary>Native views of a slot.</summary>
    extension(RoomEnemySlot self)
    {
        /// <summary>The bank-local word of the frame the slot shows, zero before it shows any.</summary>
        internal ushort SpritemapPointer
        {
            get => EnemyFrames.Spritemap(self.Frame);
            set
            {
                try
                {
                    self.Frame = FrameAt(self.Definition.Bank, value);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    throw new InvalidOperationException(
                        $"Enemy slot {self.SlotIndex} ({self.EnemyDefinitionPointer}, properties {self.Properties}) cannot show ${self.Definition.Bank:X2}:{value:X4}.",
                        exception);
                }
            }
        }
    }
}
