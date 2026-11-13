using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle animations, with the operands its command carries.</summary>
internal abstract record MotherBrainEffectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainEffectCue()
    {
    }

    /// <summary>Installs a movement behaviour.</summary>
    /// <param name="FrameBehavior">The movement behaviour the cue installs.</param>
    internal sealed record SetFrameBehavior(EnemyProjectileFrameBehavior FrameBehavior) : MotherBrainEffectCue;

    /// <summary>Sets the projectile's packed X and Y collision radii.</summary>
    /// <param name="Radius">The collision half-extents <see cref="SetRadius"/> installs.</param>
    internal sealed record SetRadius(Extent16<RoomPixels> Radius) : MotherBrainEffectCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MotherBrainEffectCueKind Kind) : MotherBrainEffectCue;
}
