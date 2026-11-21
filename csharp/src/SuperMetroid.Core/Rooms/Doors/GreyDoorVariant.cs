namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Which grey cap a sequence set belongs to (#1296): a facing, or Bomb Torizo's right-facing door.</summary>
/// <param name="Orientation">The cap's facing.</param>
/// <param name="Closing">How the cap closes once Samus has passed: at once, or, Bomb Torizo's, held open until she has the Bombs.</param>
internal readonly partial record struct GreyDoorVariant(ColoredDoorOrientation Orientation, GreyDoorClosing Closing);
