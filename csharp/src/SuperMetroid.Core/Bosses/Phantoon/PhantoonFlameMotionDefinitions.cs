namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>How Phantoon's enraged and spiral flames move and sound (the Japanese and North American values; the European release's differ).</summary>
internal static class PhantoonFlameMotionDefinitions
{
    /// <summary>The turn an enraged flame makes round the body each update, clockwise or counterclockwise: two steps.</summary>
    internal static readonly TrigonometryAngle RageTurnPerUpdate = new(2);

    /// <summary>
    /// The gravity on a falling or bouncing casual flame and a falling rain flame, in 256ths of a pixel an update, each
    /// update. A flame lands when its terrain probe finds a block, at the latest as it leaves the room, at most fifty
    /// screens tall (<see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/>), and a bounce restarts it at three
    /// pixels an update upward, so its speed stays below 64 pixels an update.
    /// </summary>
    internal static readonly SignedEightEight FlameGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);
    /// <summary>
    /// The updates from an intro flame's orbit starting to the first on which its circle may close; it circles at its full
    /// radius on the 180 before.
    /// </summary>
    internal const int IntroCircleHoldUpdates = 181;
    /// <summary>The turn an intro flame makes clockwise round the body each update: one step.</summary>
    internal static readonly TrigonometryAngle StartingTurnPerUpdate = new(1);
    /// <summary>The radius, in pixels, an intro flame starts circling at; it closes by one every other update.</summary>
    internal const int IntroCircleRadius = 48;
    /// <summary>The pixels an enraged flame's orbit widens by each update.</summary>
    internal const int RageRadiusStep = 4;
    /// <summary>The pixels a spiral flame's orbit widens by each update.</summary>
    internal const int SpiralRadiusStep = 2;
    /// <summary>The turn a spiral flame makes clockwise round the body each update: two steps.</summary>
    internal static readonly TrigonometryAngle SpiralTurnPerUpdate = new(2);
    /// <summary>Sound requested when rain starts falling and when it hits terrain.</summary>
    internal static SoundEffectId RainFallSound => RoomSound.PhantoonFlame;
    /// <summary>The rain sound's request priority.</summary>
    internal static readonly SoundRequestPriority RainSoundPriority = SoundRequestPriority.Normal;
}
