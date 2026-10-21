using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification's native view of the frame an enemy shows: the bank-local spritemap or extended-frame word the cartridge
/// stores it at, and the frame stored at a word in the enemy's bank.
/// </summary>
internal static class RoomEnemySlotFrameAccess
{
    /// <summary>Whether an installed composition shows native word <paramref name="word"/> in bank <paramref name="bank"/>.</summary>
    internal static bool Shows(byte bank, ushort word) =>
        word != 0 && EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, word) is not (null or EnemyFrameId.Empty or EnemyFrameId.EmptyExtended);

    /// <summary>The frame shown by native word <paramref name="word"/> in bank <paramref name="bank"/>; zero shows none.</summary>
    internal static EnemyFrameId? FrameAt(byte bank, ushort word) => word == 0
        ? null
        : EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, word)
            ?? throw new ArgumentOutOfRangeException(nameof(word), word, $"No installed enemy frame at ${bank:X2}:{word:X4}.");

    /// <summary>The frame shown by native word <paramref name="word"/> in bank <paramref name="bank"/>, or null when none is.</summary>
    internal static EnemyFrameId? TryFrameAt(byte bank, ushort word) => word == 0 ? null : EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, word);

    /// <summary>Native views of a slot.</summary>
    extension(RoomEnemySlot self)
    {
        /// <summary>The bank-local word of the frame the slot shows, zero before it shows any.</summary>
        internal ushort SpritemapPointer
        {
            get => EnemyFrameCartridgeLookupTooling.Word(self.Frame);
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
