using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Verification access to <see cref="SamusProjectileListLayout"/> reads extraction does not make.</summary>
internal static class SamusProjectileListLayoutAccess
{
    /// <summary>The native instruction pointers checks compare playbacks with.</summary>
    extension(SamusProjectileListLayout self)
    {
        /// <summary>Each place's instruction pointer, by segment: one per beat, then the segment's end.</summary>
        internal IReadOnlyDictionary<SamusProjectileSegment, ushort[]> BeatPositions => self.beatPositions;

        /// <summary>The instruction pointer of a projectile's playback; a cleared projectile's is zero.</summary>
        internal ushort Position(AnimationPlayback<SamusProjectileSegment>? playback) =>
            playback is { } live ? self.Position(SamusProjectilePlayback.BeatOf(live)) : (ushort)0;

        /// <summary>The playback instruction pointer <paramref name="place"/> stands for; zero is a cleared projectile.</summary>
        /// <exception cref="InvalidDataException">The pointer is no list position.</exception>
        internal AnimationPlayback<SamusProjectileSegment>? PlaybackAt(int place) =>
            place == 0 ? null : self.TryPlaceAt(place, out SamusProjectileSegmentBeat beat) ? SamusProjectilePlayback.PlaybackAt(beat)
                : throw new InvalidDataException($"$93:{place:X4} is not a Samus projectile list position.");
    }
}
