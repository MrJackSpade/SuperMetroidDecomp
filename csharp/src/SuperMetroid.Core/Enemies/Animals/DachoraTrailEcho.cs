namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// One speed-boost echo in its body's trail (#2554): the place the echo stands in and the state its entity runs on, which is
/// all the body reads of it. The trail holds them rather than the echo's entity, so a restore of the game's state reaching
/// the trail first rebuilds the echo's place, and through it the echo, instead of the echo's entity ahead of its place.
/// </summary>
/// <param name="Place">The place the echo stands in.</param>
/// <param name="State">The state the echo's entity runs on.</param>
internal sealed partial record DachoraTrailEcho(RoomEnemyPlace Place, DachoraEchoState State);
