using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verification access: the game's published frame as pixels.</summary>
internal static class SuperMetroidGamePixelsAccess
{
    /// <summary>The game's published picture.</summary>
    extension(SuperMetroidGame game)
    {
        /// <summary>Renders the last published frame in software into a fresh array; call it between steps.</summary>
        internal Rgba32[] PublishedPixels() => SoftwareFrameSnapshotRenderer.Render(game.GetRetainedDisplay(1, 1));
    }
}
