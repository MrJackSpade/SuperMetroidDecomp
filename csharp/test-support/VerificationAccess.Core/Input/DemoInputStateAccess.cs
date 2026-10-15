using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Input;

/// <summary>Verification access to <see cref="DemoInputState"/> members production does not use.</summary>
internal static class DemoInputStateAccess
{
    /// <summary>Exposes <see cref="DemoInputState"/> state that only verification reads.</summary>
    extension(DemoInputState self)
    {
        /// <summary>Whether the object still runs a program.</summary>
        public bool IsActive => self.Cursor is not null;
    }
}
