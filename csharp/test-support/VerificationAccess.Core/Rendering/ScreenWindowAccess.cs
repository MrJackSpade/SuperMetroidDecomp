namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to whether a screen window covers nothing, which the backend never asks.</summary>
internal static class ScreenWindowAccess
{
    /// <summary>Window queries only verification runs.</summary>
    extension(ScreenWindow self)
    {
        /// <summary>Whether the window covers no pixel because it has no shapes or no lines.</summary>
        public bool IsClosed => self.Shapes.Length == 0 || self.FirstLine == self.EndLine;
    }
}
