using SuperMetroid.Core.Hardware;
﻿using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ObjSpriteImage"/> that only verification reads.</summary>
internal static class ObjSpriteImageConstants
{
    /// <summary>An OAM image with no whole-frame draws, for screens that still build only OAM.</summary>
    public static ObjSpriteImage OamOnly(OamBuffer oam) => new(Ensure.NotNull(oam), []);
}
