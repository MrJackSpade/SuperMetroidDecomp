namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The breakup a piece of Lower Norfair Ridley joins as his death spawns it (#2231): his encounter, whose tail, facing and
/// colours the piece starts from, and where his body stood as it broke up.
/// </summary>
/// <param name="Ridley">His encounter.</param>
/// <param name="Body">Where his body stood as it broke up.</param>
internal sealed record RidleyBreakupSource(NorfairRidleyState Ridley, Position16<RoomPixels> Body);
