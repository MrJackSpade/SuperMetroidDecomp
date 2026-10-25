using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One explosion of Phantoon's death: where it bursts, which dust animation, and how long until the next.</summary>
/// <param name="Offset">Where it bursts, relative to Phantoon's body.</param>
/// <param name="Type">The dust animation shown.</param>
/// <param name="Delay">Frames before the next explosion.</param>
public readonly record struct PhantoonDeathExplosionDefinition(Offset16<RoomPixels> Offset, MiscDustAnimation Type, int Delay);
