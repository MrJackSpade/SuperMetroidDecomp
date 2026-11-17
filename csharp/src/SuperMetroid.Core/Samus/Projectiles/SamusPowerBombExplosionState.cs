using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Samus's Power Bomb explosion: the one-at-a-time Power Bomb lock, the room point the blast is centred on, and the blast's
/// timeline (<see cref="Blast"/>), with the oval of added colour each frame shows (<see cref="ShownOval"/>).
/// </summary>
public sealed class SamusPowerBombExplosionState
{
    /// <summary>Starts an explosion whose blast glows in <paramref name="glowColors"/> (#2049).</summary>
    /// <param name="glowColors">The installed glow colours.</param>
    public SamusPowerBombExplosionState(InstalledPowerBombGlowColors glowColors) => Blast = new PowerBombBlastTimeline(glowColors);

    /// <summary>The blast's stages, radii and glow (#2000).</summary>
    public PowerBombBlastTimeline Blast { get; }

    /// <summary>Room-pixel centre copied from the projectile when its fuse reaches zero.</summary>
    public Position16<RoomPixels> Position { get; private set; }

    /// <summary>
    /// The oval the current frame shows (#1927): centred on the blast, as wide as the blast's shown radius, and the glow's
    /// colour (<see cref="PowerBombBlastTimeline.ShownRadius"/>).
    /// </summary>
    public PowerBombBlastOval ShownOval => PowerBombBlastOval.Of(Position, Blast.ShownRadius, Blast.GlowColor);

    /// <summary>True from placement until cleanup releases the one-at-a-time Power Bomb lock.</summary>
    public bool IsArmed { get; private set; }

    /// <summary>
    /// True from the explosion's spawn until its animation ends: X-ray, the pause check, Shaktool's room, Mother Brain
    /// and the sound queues test it.
    /// </summary>
    public bool IsActive => Blast.IsRunning;

    /// <summary>
    /// Implements the morph ball's early power-bomb lock before a Power Bomb takes its place.
    /// </summary>
    public void Arm()
    {
        IsArmed = true;
    }

    /// <summary>
    /// Implements the explosion's spawn after the fuse has copied the projectile's world position.
    /// </summary>
    public void Spawn(Position16<RoomPixels> position)
    {
        Position = position;
        Blast.Start();
    }

    /// <summary>
    /// Implements Crystal Flash's explosion request and both setup calls after
    /// Crystal Flash completes its ten-step rise.
    /// </summary>
    public void BeginCrystalFlash(Position16<RoomPixels> position)
    {
        // Crystal Flash releases the one-at-a-time Power Bomb lock before starting its two
        // explosion effects. Their animation still shares the Power Bomb's status,
        // position, radius, speed, glow colour, and window.
        IsArmed = false;
        Position = position;
        Blast.BeginCrystalFlash();
    }

    /// <summary>
    /// Runs one update of the explosion's active stage step.
    /// </summary>
    /// <returns>True only on the frame that cleanup completes.</returns>
    public bool StepFrame() => Blast.StepFrame();

    /// <summary>
    /// Releases the armed flag after cleanup when Crystal Flash did not take ownership.
    /// </summary>
    public void ReleaseFlag() => IsArmed = false;

    /// <summary>Clears every explosion field during room/runtime reset.</summary>
    public void Reset()
    {
        IsArmed = false;
        Position = Position16<RoomPixels>.Zero;
        Blast.Reset();
    }
}
