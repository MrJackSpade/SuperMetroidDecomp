using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Verification access to the animation of whichever Pipe Bug family occupies a place (#1845): the Brinstar, Norfair and
/// yellow bugs each keep it in their own state, and native pointer oracles read and resume it whatever the family.
/// </summary>
internal static class PipeBugPlaybackAccess
{
    /// <summary>Exposes the Pipe Bug animation of a place.</summary>
    extension(RoomEnemySlot slot)
    {
        /// <summary>The Pipe Bug's animation and where it stands in it, or null when the place holds no Pipe Bug.</summary>
        /// <exception cref="InvalidOperationException">Set on a place that holds no Pipe Bug, or to null.</exception>
        internal AnimationPlayback<PipeBugSegment>? PipeBugPlayback
        {
            get => slot.SpeciesState<BrinstarPipeBugState>()?.Playback
                ?? slot.SpeciesState<NorfairPipeBugState>()?.Playback
                ?? slot.SpeciesState<YellowPipeBugState>()?.Playback;
            set
            {
                AnimationPlayback<PipeBugSegment> playback = value
                    ?? throw new InvalidOperationException($"A Pipe Bug in slot {slot.SlotIndex} cannot play no animation.");
                if (slot.SpeciesState<BrinstarPipeBugState>() is { } brinstar)
                    brinstar.Playback = playback;
                else if (slot.SpeciesState<NorfairPipeBugState>() is { } norfair)
                    norfair.Playback = playback;
                else if (slot.SpeciesState<YellowPipeBugState>() is { } yellow)
                    yellow.Playback = playback;
                else
                    throw new InvalidOperationException($"Slot {slot.SlotIndex} holds no Pipe Bug.");
            }
        }
    }
}
