using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="PreInstruction">The movement routine the cue installs.</param>
/// <param name="Radius">The collision half-extents <see cref="MotherBrainEffectCueKind.SetRadius"/> installs.</param>
internal readonly record struct MotherBrainEffectCue(MotherBrainEffectCueKind Kind, EnemyProjectilePreInstruction PreInstruction = default, Extent16<RoomPixels> Radius = default);
