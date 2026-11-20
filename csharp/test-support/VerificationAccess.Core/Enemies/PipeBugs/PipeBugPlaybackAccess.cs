using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Verification access to the animation of whichever Pipe Bug family occupies a place (#1845): the Brinstar, Norfair and
/// yellow bugs each keep it in their own state, and native pointer oracles read and resume it whatever the family.
/// </summary>
internal static class PipeBugPlaybackAccess
{
    /// <summary>Exposes the Pipe Bug animation of a place.</summary>
    extension(RoomEnemyPlace place)
    {
        /// <summary>The Pipe Bug's animation and where it stands in it, or null when the place holds no Pipe Bug.</summary>
        /// <exception cref="InvalidOperationException">Set on a place that holds no Pipe Bug, or to null.</exception>
        internal AnimationPlayback<PipeBugSegment>? PipeBugPlayback
        {
            get => SuperMetroid.Core.Enemies.EnemyPartners.Listed<BrinstarPipeBugEntity>(place).State?.Playback
                ?? SuperMetroid.Core.Enemies.EnemyPartners.Listed<NorfairPipeBugEntity>(place).State?.Playback
                ?? SuperMetroid.Core.Enemies.EnemyPartners.Listed<YellowPipeBugEntity>(place).State?.Playback;
            set
            {
                AnimationPlayback<PipeBugSegment> playback = value
                    ?? throw new InvalidOperationException($"A Pipe Bug in slot {place.Ordinal} cannot play no animation.");
                if (SuperMetroid.Core.Enemies.EnemyPartners.Listed<BrinstarPipeBugEntity>(place).State is { } brinstar)
                    brinstar.Playback = playback;
                else if (SuperMetroid.Core.Enemies.EnemyPartners.Listed<NorfairPipeBugEntity>(place).State is { } norfair)
                    norfair.Playback = playback;
                else if (SuperMetroid.Core.Enemies.EnemyPartners.Listed<YellowPipeBugEntity>(place).State is { } yellow)
                    yellow.Playback = playback;
                else
                    throw new InvalidOperationException($"Slot {place.Ordinal} holds no Pipe Bug.");
            }
        }
    }
}
