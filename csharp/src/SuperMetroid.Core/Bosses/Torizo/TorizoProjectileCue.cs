using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's projectile programs, with the operands its native command carries.</summary>
internal abstract record TorizoProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TorizoProjectileCue()
    {
    }

    /// <summary>Turns behaviours off (<c>Instruction_EnemyProjectile_Properties_AndY</c>, $86:823C).</summary>
    /// <param name="Properties">The behaviours a property cue turns on or off.</param>
    /// <param name="Depth">The depth relative to Samus the cue moves the projectile to, or null to leave it.</param>
    internal sealed record ClearProperties(EnemyProjectilePropertyFlags Properties, EnemyProjectileDepth? Depth = null) : TorizoProjectileCue;

    /// <summary>Turns behaviours on (<c>Instruction_EnemyProjectile_Properties_OrY</c>, $86:8230).</summary>
    /// <param name="Properties">The behaviours a property cue turns on or off.</param>
    /// <param name="Depth">The depth relative to Samus the cue moves the projectile to, or null to leave it.</param>
    internal sealed record SetProperties(EnemyProjectilePropertyFlags Properties, EnemyProjectileDepth? Depth = null) : TorizoProjectileCue;

    /// <summary>Continues at the target while Golden Torizo's eye-beam explosions are disabled.</summary>
    /// <param name="Target">The program a conditional cue continues at.</param>
    internal sealed record GoToIfEyeBeamExplosionsDisabled(TorizoProjectileProgram Target) : TorizoProjectileCue;

    /// <summary>Continues at the target one time in four (<c>Instruction_EnemyProjectile_GotoY_Probability_1_4</c>, $86:A456).</summary>
    /// <param name="Target">The program a conditional cue continues at.</param>
    internal sealed record GoToSometimes(TorizoProjectileProgram Target) : TorizoProjectileCue;

    /// <summary>Queues a sound (the <c>QueueSoundInY</c> commands).</summary>
    /// <param name="Sound">The sound a <see cref="PlaySound"/> cue queues.</param>
    /// <param name="MaximumQueued">How many sounds may already be queued for the cue's sound still to be queued: one or six.</param>
    internal sealed record PlaySound(SoundEffectId Sound, SoundQueueLimit MaximumQueued) : TorizoProjectileCue;

    /// <summary>Scatters the projectile randomly about its position ($86:81DF).</summary>
    /// <param name="Spread">The random scatter a <see cref="Scatter"/> cue applies.</param>
    internal sealed record Scatter(EnemyProjectileScatter Spread) : TorizoProjectileCue;

    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    /// <param name="PreInstruction">The movement routine a <see cref="SetPreInstruction"/> cue installs.</param>
    internal sealed record SetPreInstruction(EnemyProjectilePreInstruction PreInstruction) : TorizoProjectileCue;

    /// <summary>Sets the projectile's collision radius (<c>Instruction_EnemyProjectile_XYRadiusInY</c>, $86:8298).</summary>
    /// <param name="Radius">The collision radius a <see cref="SetRadius"/> cue sets.</param>
    internal sealed record SetRadius(Extent16<RoomPixels> Radius) : TorizoProjectileCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(TorizoProjectileCueKind Kind) : TorizoProjectileCue;
}
