using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The cartridge words of frames an enemy colliding by its frames' hitboxes shows, for checks that compare them (#1603).</summary>
internal static class EnemyFrameHitboxWordsAccess
{
    extension(EnemyFrameCartridgeLookupTooling)
    {
        /// <summary>
        /// The bank-local word <paramref name="frame"/> is stored at for an enemy colliding by its frames' hitboxes: the empty
        /// frame is the point-hitbox record.
        /// </summary>
        internal static ushort FrameHitboxWord(EnemyFrameId frame) =>
            frame == EnemyFrameId.PointAtOrigin ? EnemyFrameCartridgeLookupTooling.EmptyFrameHitboxWord : EnemyFrameCartridgeLookupTooling.Word(frame);

        /// <summary>The bank-local word of the frame an enemy shows, by how it collides, zero before it shows any.</summary>
        internal static ushort Word(EnemyFrameId? frame, bool collidesByFrameHitboxes) => frame is not { } shown ? (ushort)0
            : collidesByFrameHitboxes ? EnemyFrameCartridgeLookupTooling.FrameHitboxWord(shown) : EnemyFrameCartridgeLookupTooling.Word(shown);

        /// <summary>
        /// The bank-local word of the frame an enemy whose header lives in <paramref name="bank"/> shows, by how it collides,
        /// zero before it shows any: a frame another bank stores is shown from this bank's record storing it again (#2327).
        /// </summary>
        internal static ushort WordIn(byte bank, EnemyFrameId? frame, bool collidesByFrameHitboxes) => frame is not { } shown ? (ushort)0
            : collidesByFrameHitboxes && shown == EnemyFrameId.PointAtOrigin ? EnemyFrameCartridgeLookupTooling.EmptyFrameHitboxWord
            : EnemyFrameCartridgeLookupTooling.WordIn(bank, shown);
    }
}
