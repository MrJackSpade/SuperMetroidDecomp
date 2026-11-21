using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's projectile animations: what it does, with the values it acts on.</summary>
internal abstract partial record TorizoProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TorizoProjectileCue()
    {
    }

    /// <summary>Requests a sound at the priority the cue states.</summary>
    /// <param name="Sound">The sound a <see cref="PlaySound"/> cue queues.</param>
    /// <param name="Priority">How strongly the sound competes for a place in its library's request queue.</param>
    internal sealed partial record PlaySound(SoundEffectId Sound, SoundRequestPriority Priority) : TorizoProjectileCue;

    /// <summary>Scatters the projectile randomly about its position.</summary>
    /// <param name="Spread">The random scatter a <see cref="Scatter"/> cue applies.</param>
    internal sealed partial record Scatter(EnemyProjectileScatter Spread) : TorizoProjectileCue;

    /// <summary>Sets the projectile's collision radius.</summary>
    /// <param name="Radius">The collision radius a <see cref="SetRadius"/> cue sets.</param>
    internal sealed partial record SetRadius(Extent16<RoomPixels> Radius) : TorizoProjectileCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(TorizoProjectileCueKind Kind) : TorizoProjectileCue;
}
