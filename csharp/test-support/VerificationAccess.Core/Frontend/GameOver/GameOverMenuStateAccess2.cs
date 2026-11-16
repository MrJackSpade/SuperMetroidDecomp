using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Verification access to <see cref="GameOverMenuState"/> members production does not use.</summary>
internal static class GameOverMenuStateAccess2
{
    /// <summary>Exposes <see cref="GameOverMenuState"/> state that only verification reads.</summary>
    extension(GameOverMenuState self)
    {
        /// <summary>The colours the Baby shows (#1512): those of its current look.</summary>
        internal GameOverBabyColouring BabyColours => self.BabyLook.Colours;
    }
}
