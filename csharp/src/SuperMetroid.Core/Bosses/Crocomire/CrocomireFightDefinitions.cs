namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How Crocomire wakes, steps, spits and reacts to his hits, and how he holds Samus back (#2182).</summary>
internal static class CrocomireFightDefinitions
{
    /// <summary>How close Samus comes horizontally, exclusive, before his idle body wakes, in pixels.</summary>
    internal const int WakeDistance = 224;

    /// <summary>
    /// The body X left of which his step forward turns into his charge near the spike wall, and at or right of which his
    /// backing off from the wall ends.
    /// </summary>
    internal const int SpikeWallChargeX = 768;

    /// <summary>The tongue's horizontal offset from the body, in pixels, set when the tongue initializes.</summary>
    internal const int TongueBodyXOffset = 23;

    /// <summary>How far above the bottom of his body the dust he raises rises, in pixels.</summary>
    internal const int DustAboveBodyBottom = 16;

    /// <summary>How far his invisible wall pushes Samus each update she presses it: back and a little up.</summary>
    internal static readonly Offset16<RoomPixels> WallPush = new(-4, -1);


    /// <summary>How far either side of his body the dust of a step toward the spike wall rises before its spread, in pixels.</summary>
    internal const int SpikeWallDustSide = 32;

    /// <summary>How long a shaking step shakes the screen, in updates.</summary>
    internal const int StepShakeDuration = 5;

    /// <summary>How far past the camera's left, exclusive, his body's left edge lies while a step back counts as on screen.</summary>
    internal const int StepBackScreenReach = 260;

    /// <summary>How far past the camera's left, exclusive, his body's left edge lies while a mouth shot counts as on screen.</summary>
    internal const int MouthShotScreenReach = 256;

    /// <summary>The steps back a charged beam in his mouth queues.</summary>
    internal const int ChargedBeamSteps = 2;

    /// <summary>The steps back a missile in his mouth queues.</summary>
    internal const int MissileSteps = 1;

    /// <summary>The steps back a super missile in his mouth queues.</summary>
    internal const int SuperMissileSteps = 3;

    /// <summary>The steps a Power Bomb's charge queues.</summary>
    internal const int PowerBombSteps = 3;

    /// <summary>How long a shot in his mouth flashes him, in updates.</summary>
    internal const int MouthShotFlash = 14;

    /// <summary>
    /// The frames a mouth shot holds Crocomire's current animation frame (#2182): an uncharged beam holds it this long, and a
    /// shot that starts a damage reaction adds this to its hold, whatever his fight is doing.
    /// </summary>
    internal const int MouthShotReactionHoldFrames = 8;

    /// <summary>How long a Power Bomb flashes him, in updates.</summary>
    internal const int PowerBombFlash = 4;

    /// <summary>How far a claw's touch pushes Samus back, in pixels.</summary>
    internal const int ClawPush = -4;

    /// <summary>Where his spit leaves his body: ahead of it and up.</summary>
    internal static readonly Offset16<RoomPixels> SpitOffset = new(-32, -16);

    /// <summary>The speed his spit leaves at: leftward, and down as slowly as a speed can fall.</summary>
    internal static readonly PixelVelocity<RoomPixels> SpitVelocity = new(SignedEightEight.FromPixels(-2), SignedEightEight.FromPixelFraction(1, 256));

    /// <summary>How many places after his body the room lists his tongue, the one part his body heads.</summary>
    internal const int TonguePlacesAfterBody = 1;
}
