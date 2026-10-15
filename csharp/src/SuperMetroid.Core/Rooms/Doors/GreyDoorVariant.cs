namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Which grey cap a program set belongs to (#1296): a facing, or Bomb Torizo's right-facing door.</summary>
/// <param name="Orientation">The cap's facing.</param>
/// <param name="BombTorizo">Whether this is Bomb Torizo's door, whose closing waits for Bombs.</param>
internal readonly record struct GreyDoorVariant(ColoredDoorOrientation Orientation, bool BombTorizo);
