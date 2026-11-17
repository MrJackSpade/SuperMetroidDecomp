using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Verification's native view of a trail side: the bank-$90 address of the beat its animation plays next, or zero for a
/// side with no animation, as native trail RAM holds it (#2348).
/// </summary>
internal static class SamusProjectileTrailSideNative
{
    /// <summary>The pointer native trail RAM's cleared zero names: no animation.</summary>
    private const ushort Cleared = 0;

    /// <summary>
    /// The playback whose next beat is stored at bank-$90 address <paramref name="address"/>: the animation of the program
    /// storing it, at that beat, or at its end word.
    /// </summary>
    internal static AnimationPlayback<ProjectileTrailSegment> PlaybackAt(ushort address)
    {
        foreach (ProjectileTrailSegment program in Enum.GetValues<ProjectileTrailSegment>())
        {
            for (int beat = 0; beat <= ProjectileTrailSparkleAnimations.Segments.Beats(program).Count; beat++)
            {
                if (ProjectileTrailSources.Address(program, beat) == address)
                    return new(ProjectileTrailSparkleAnimations.StartingAt[program],
                        beat == 0 ? AnimationPlace.StartOfAnimation : new AnimationPlace(0, beat));
            }
        }
        throw new ArgumentOutOfRangeException(nameof(address), address, "No trail beat is stored there.");
    }

    /// <summary>The bank-$90 address of the beat <paramref name="playback"/> plays next, or of its program's end word.</summary>
    internal static ushort Address(AnimationPlayback<ProjectileTrailSegment> playback) =>
        ProjectileTrailSources.Address(playback.PlayingSegment, Math.Max(playback.Place.Beat, 0));

    /// <summary>A side's native pointer.</summary>
    extension(SamusProjectileTrailSide side)
    {
        /// <summary>The bank-$90 address of the beat the side plays next, as native trail RAM holds it.</summary>
        internal int InstructionPointer
        {
            get => side.Playback is { } playback ? Address(playback) : Cleared;
            set => side.Playback = value == Cleared ? null : PlaybackAt(checked((ushort)value));
        }
    }
}
