using SuperMetroid.Core.Hardware;
﻿using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge values and helpers of <see cref="IntroCinematicState"/> that only verification reads.</summary>
internal static class IntroCinematicStateConstants
{
    /// <summary>Full SNES viewport width used by the cinematic's software planes.</summary>
    internal const int ScreenWidth = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>Visible SNES viewport height used by the cinematic's software planes.</summary>
    internal const int ScreenHeight = SnesPpuLayout.ScreenHeightPixels;
}
