using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification's native view of the frame an enemy shows: the bank-local spritemap or extended-frame word the cartridge
/// stores it at, and the frame stored at a word in the enemy's bank.
/// </summary>
internal static class RoomEnemyPlaceFrameAccess
{
    /// <summary>Whether an installed composition shows native word <paramref name="word"/> in bank <paramref name="bank"/>.</summary>
    internal static bool Shows(byte bank, ushort word) =>
        word != 0 && EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, word) is not (null or EnemyFrameId.Empty);

    /// <summary>The frame shown by native word <paramref name="word"/> in bank <paramref name="bank"/>; zero shows none.</summary>
    internal static EnemyFrameId? FrameAt(byte bank, ushort word) => word == 0
        ? null
        : TryFrameAt(bank, word)
            ?? throw new ArgumentOutOfRangeException(nameof(word), word, $"No installed enemy frame at ${bank:X2}:{word:X4}.");

    /// <summary>
    /// The frame shown by native word <paramref name="word"/> in bank <paramref name="bank"/>, or null when none is. Mother
    /// Brain's blank brain frame, which nothing installs, is her brain's hit area (#2220).
    /// </summary>
    internal static EnemyFrameId? TryFrameAt(byte bank, ushort word) =>
        word == 0 ? null
        : EnemyFrameCartridgeLookup.Of(EnemyFrameId.MotherBrainBrainHitArea) == new SnesAddress(bank, word) ? EnemyFrameId.MotherBrainBrainHitArea
        : EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, word);

    /// <summary>Native views of a slot.</summary>
    extension(RoomEnemyPlace self)
    {
        /// <summary>
        /// The bank-local word of the frame the slot shows, zero before it shows any: the empty frame of a slot colliding by
        /// its frames' hitboxes is the point-hitbox record, and a frame another bank stores is its own bank's record storing it
        /// again (#2327).
        /// </summary>
        internal ushort SpritemapPointer
        {
            // One of Kraid's parts shows a pose, whose record is the first that stores it (#2210).
            // A slot showing no frame, a free one among them, names no header and shows zero.
            get => SuperMetroid.Core.Enemies.EnemyPartners.Listed<KraidEntity>(self).PartState?.Pose is { } pose ? KraidPartFrameToolingDefinitions.Spritemap(pose)
                : self.Frame is null ? (ushort)0
                : EnemyFrameCartridgeLookupTooling.WordIn(self.NativeHeader.Bank, self.Frame, self.CollidesByFrameHitboxes);
            set
            {
                // Zero shows no frame in any bank, so a free slot, which names no header, imports it too.
                if (value == 0)
                {
                    self.Frame = null;
                    return;
                }
                try
                {
                    // One of Kraid's parts shows the pose its record stores: the picture, its step and its depth (#2210).
                    if (SuperMetroid.Core.Enemies.EnemyPartners.Listed<KraidEntity>(self).PartState is { } part
                        && KraidPartFrameToolingDefinitionsConstants.PoseAt(value) is { } pose)
                    {
                        self.Frame = part.Show(pose);
                        return;
                    }
                    self.Frame = FrameAt(self.NativeHeader.Bank, value);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    throw new InvalidOperationException(
                        $"Enemy slot {self.Ordinal} ({self.DefinitionId}, contact {self.Contact}, processing {self.Processing}, presence {self.Presence}) cannot show ${self.NativeHeader.Bank:X2}:{value:X4}.",
                        exception);
                }
            }
        }
    }
}
