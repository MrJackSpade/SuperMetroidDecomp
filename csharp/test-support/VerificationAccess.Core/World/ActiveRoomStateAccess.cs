using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.World;

/// <summary>Verification access to <see cref="ActiveRoomState"/> members production does not use.</summary>
internal static class ActiveRoomStateAccess
{
    /// <summary>Exposes <see cref="ActiveRoomState"/> state that only verification reads.</summary>
    extension(ActiveRoomState self)
    {
        /// <summary>Landing Site's scrolling-sky updater after room initialization.</summary>
        public ScrollingSkyState? ScrollingSky => self.View(c => c.ScrollingSky, s => s.ScrollingSky);
    }
}
