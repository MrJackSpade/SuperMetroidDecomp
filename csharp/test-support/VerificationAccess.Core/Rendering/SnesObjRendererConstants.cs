using SuperMetroid.Core.Hardware;
﻿using SuperMetroid.Core.Assets;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="SnesObjRenderer"/> that only verification reads.</summary>
internal static class SnesObjRendererConstants
{
    /// <summary>Renders finalized OAM into caller-owned storage, clearing it to transparent first.</summary>
    public static void Render(
        Span<Rgba32> output,
        ObjSpriteImage sprites,
        SnesVram vram,
        SnesCgram cgram,
        byte obsel,
        Extent16<ScreenPixels>? size = null,
        int? priority = null)
    {
        Ensure.NotNull(sprites.Oam);
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Extent16<ScreenPixels> frame = SnesObjRenderer.RequireFrame(size);
        Ensure.LengthEqual(output, frame.X * frame.Y);
        output.Clear();

        // A lower OAM number wins an OBJ-vs-OBJ overlap on the normal SNES priority
        // rotation setting. Painting records backwards lets the lower index overwrite
        // the higher one without a second priority buffer.
        //
        // Crucially, an OBJ's two priority bits do *not* participate in that OBJ-vs-OBJ
        // choice. They only place the already-selected OBJ pixel relative to the BG
        // planes. The gameplay compositor asks for one priority plane at a time so it can
        // interleave those planes with BG1/BG2. We must therefore visit every OAM entry on
        // every pass: when a lower-numbered sprite of a different priority owns a pixel,
        // it clears that pixel from this plane. Skipping nonmatching entries would let a
        // later OBJ3 (Ceres Ridley) incorrectly paint over an earlier OBJ2 (Samus).
        SnesObjRenderer.Paint(frame, sprites, vram, cgram, obsel, priority, new ObjRasterTargets(output));
    }
}
