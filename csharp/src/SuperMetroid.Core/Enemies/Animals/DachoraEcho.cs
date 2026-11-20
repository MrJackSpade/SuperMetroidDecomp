namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One of a Dachora's speed-boost echoes (#2554): its place and the state its body made for it.</summary>
/// <param name="Place">The echo's place, which the room lists after its body.</param>
/// <param name="State">The echo's state, which the body made as it was placed and the echo took as it was placed.</param>
internal readonly record struct DachoraEcho(RoomEnemyPlace Place, DachoraEchoState State);
