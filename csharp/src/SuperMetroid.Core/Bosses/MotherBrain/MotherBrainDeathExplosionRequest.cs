using SuperMetroid.Core.Enemies.Projectiles;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One projectile in a simultaneous death-explosion batch.</summary>
/// <param name="Offset">Where the explosion bursts relative to her body.</param>
/// <param name="Animation">Which explosion it shows.</param>
/// <param name="Sound">The sound it requests, at the priority it requests it.</param>
public readonly partial record struct MotherBrainDeathExplosionRequest(
    Offset16<RoomPixels> Offset,
    DustExplosionAnimation Animation,
    SoundRequest Sound);
