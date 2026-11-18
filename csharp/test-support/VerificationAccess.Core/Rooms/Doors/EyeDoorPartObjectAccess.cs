namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification access to <see cref="EyeDoorPartObject"/> members production does not use.</summary>
internal static class EyeDoorPartObjectAccess
{
    /// <summary>Exposes <see cref="EyeDoorPartObject"/> state that only verification reads.</summary>
    extension(EyeDoorPartObject self)
    {
        /// <summary>Whether the part closes behind Samus, leaving the room on its next update.</summary>
        internal bool ClosesBehindSamus => self.Phase == EyeDoorPartPhase.ClosingBehindSamus;
    }
}
