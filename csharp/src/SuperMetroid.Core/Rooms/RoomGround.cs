namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The ground Samus's running footsteps and landings go by (#2416): a room's (<see cref="RoomDefinition.Ground"/>), or a
/// scene's own ground where no room has loaded.
/// </summary>
/// <param name="Wetness">When the ground is wet, so that her footsteps and landings splash.</param>
/// <param name="DryLandingsRaiseDust">Whether her landings raise dust while the ground is dry; otherwise they raise nothing.</param>
public sealed partial record RoomGround(RoomGroundWetness Wetness, bool DryLandingsRaiseDust);
