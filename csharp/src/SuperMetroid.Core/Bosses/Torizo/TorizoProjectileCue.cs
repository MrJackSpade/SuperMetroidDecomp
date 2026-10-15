using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's projectile programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="PreInstruction">The movement routine a <see cref="TorizoProjectileCueKind.SetPreInstruction"/> cue installs.</param>
/// <param name="Properties">The property bits a property cue sets or clears.</param>
/// <param name="Sound">The sound a <see cref="TorizoProjectileCueKind.PlaySound"/> cue queues.</param>
/// <param name="MaximumQueued">The sound queue limit the command carries.</param>
/// <param name="Target">The program a conditional cue continues at.</param>
/// <param name="Radius">The collision radius a <see cref="TorizoProjectileCueKind.SetRadius"/> cue sets.</param>
/// <param name="Scatter">The random scatter a <see cref="TorizoProjectileCueKind.Scatter"/> cue applies.</param>
internal readonly record struct TorizoProjectileCue(
    TorizoProjectileCueKind Kind,
    EnemyProjectilePreInstruction PreInstruction = default,
    EnemyProjectilePropertyFlags Properties = EnemyProjectilePropertyFlags.None,
    SoundEffectId Sound = default,
    byte MaximumQueued = 0,
    TorizoProjectileProgram Target = default,
    Extent16<RoomPixels> Radius = default,
    EnemyProjectileScatter Scatter = default);
