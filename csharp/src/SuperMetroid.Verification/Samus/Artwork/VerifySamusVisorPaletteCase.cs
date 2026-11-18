using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>The updates the shown visor shade still holds and the shade the room's visor cycle shows (#2387).</summary>
/// <param name="Timer">The updates the shown shade still holds.</param>
/// <param name="Shown">The shade the cycle shows, or null before its first.</param>
internal readonly record struct VerifySamusVisorPaletteCase(int Timer, PictureShade? Shown)
{
    /// <summary>The case <paramref name="state"/> stands at.</summary>
    internal static VerifySamusVisorPaletteCase Of(SamusVisorShadeState state) => new(state.Cycle.UpdatesLeft, state.Cycle.Shown);
}
