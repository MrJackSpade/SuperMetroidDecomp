namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification access to <see cref="BlueDoorCapObject"/> members production does not use.</summary>
internal static class BlueDoorCapObjectAccess
{
    /// <summary>Exposes <see cref="BlueDoorCapObject"/> state that only verification reads.</summary>
    extension(BlueDoorCapObject self)
    {
        /// <summary>Whether a shot opened the cap, so whether it closes behind Samus is the room's shot-cap rule's (#1906).</summary>
        internal bool OpenedByShot => self.shotCapClosing is not null;
    }
}
