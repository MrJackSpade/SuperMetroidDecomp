namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What the owner of a <see cref="DoorContact"/> does on each of its cases (#2553).</summary>
/// <param name="EntersDoor">Reacts to a <see cref="DoorContact.EntersDoor"/>.</param>
/// <param name="StandsAtElevator">Reacts to a <see cref="DoorContact.StandsAtElevator"/>.</param>
/// <param name="BrushesElevator">Reacts to a <see cref="DoorContact.BrushesElevator"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record DoorContactReactions<TContext>(Action<DoorContact.EntersDoor, TContext> EntersDoor, Action<DoorContact.StandsAtElevator, TContext> StandsAtElevator, Action<DoorContact.BrushesElevator, TContext> BrushesElevator);
