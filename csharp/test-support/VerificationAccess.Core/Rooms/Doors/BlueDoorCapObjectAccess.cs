namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification views of a <see cref="BlueDoorCapObject"/>'s origin, which its own updates read.</summary>
internal static class BlueDoorCapObjectAccess
{
    /// <summary>Reads what the cap stands for, from where it came from.</summary>
    extension(BlueDoorCapObject self)
    {
        /// <summary>Whether a shot opened the cap, so whether it closes behind Samus is the room's shot-cap rule's (#1906).</summary>
        internal bool OpenedByShot => self.ShotCapClosing is not null;

        /// <summary>The door a cap another door turned into stands in; none for a cap a shot opened or one closing behind Samus.</summary>
        internal PlacedDoor? Door => self.Origin is BlueDoorCapObject.CapOrigin.TakenOverCap taken ? taken.Door : null;

        /// <summary>The room's shot-cap rule of a cap a shot opened; none for a cap no shot opened.</summary>
        internal Compatibility.ShotOpenedCapClosing<Progression.LockedDoor>? ShotCapClosing =>
            self.Origin is BlueDoorCapObject.CapOrigin.ShotOpenedCap shot ? shot.Rule : null;
    }
}
