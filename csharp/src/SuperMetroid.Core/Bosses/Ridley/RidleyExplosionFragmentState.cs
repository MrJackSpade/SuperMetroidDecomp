using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The state of one fragment of Norfair Ridley's breakup.</summary>
internal sealed class RidleyExplosionFragmentState
{
    /// <summary>Signed 1/256-pixel velocity per update: X, which drag slows toward zero, and Y, which gravity increases.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The updates the fragment lives, counted down past zero.</summary>
    public int Lifetime { get; internal set; }

    /// <summary>The fragment's playing breakup animation and where it stands (#641, #1600), set when it spawns.</summary>
    internal AnimationPlayback<RidleyBreakupFrame> Playback { get; set; } =
        new(RidleyBreakupAnimations.StartingWith(RidleyBreakupFrame.TailSegmentSmall), AnimationPlace.StartOfAnimation);
}
