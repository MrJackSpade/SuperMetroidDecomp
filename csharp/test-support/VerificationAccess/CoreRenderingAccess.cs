using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using System.Buffers;
using System.Runtime.CompilerServices;

/// <summary>Verification access to <see cref="RenderFrameSnapshotCodec"/> members production does not use.</summary>
internal static class RenderFrameSnapshotCodecAccess
{
    extension(RenderFrameSnapshotCodec)
    {
        internal static RenderFrameSnapshot Deserialize(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length > RenderPacketFormat.MaximumPacketBytes)
                throw new InvalidDataException("Display fixture exceeds the bounded packet size.");
            return RenderFrameSnapshotCodec.Deserialize(bytes.ToArray());
        }
    }
}

/// <summary>Verification access to <see cref="SnesBgTilemapRenderer"/> members production does not use.</summary>
internal static class SnesBgTilemapRendererAccess
{
    extension(SnesBgTilemapRenderer)
    {
        /// <summary>
        /// Renders a pixel-scrolled Mode-1 4-bpp viewport from any BGSC 32/64-by-32/64 map.
        /// Palette index zero is transparent so a lower-priority BG or backdrop remains visible.
        /// </summary>
        internal static Rgba32[] Render4BppViewport(
            SnesVram vram,
            SnesCgram cgram,
            ushort tilemapBaseWord,
            ushort characterBaseWord,
            ushort horizontalScroll,
            ushort verticalScroll,
            int width,
            int height,
            int tilemapWidthInTiles = 64,
            int tilemapHeightInTiles = 32,
            IReadOnlyList<ushort>? horizontalScrollByLine = null,
            bool? priority = null)
        {
            var output = new Rgba32[checked(width * height)];
            SnesBgTilemapRenderer.Composite4BppViewport(
                output,
                vram,
                cgram,
                tilemapBaseWord,
                characterBaseWord,
                horizontalScroll,
                verticalScroll,
                width,
                height,
                tilemapWidthInTiles,
                tilemapHeightInTiles,
                horizontalScrollByLine,
                priority);
            return output;
        }

        /// <summary>
        /// Renders consecutive tilemap rows. This first implementation supports the 2-bpp BG3
        /// format used by the gameplay HUD; its parameters retain the actual VRAM bases so the
        /// debugger can be compared directly with PPU registers.
        /// </summary>
        internal static Rgba32[] Render2Bpp(
            SnesVram vram,
            SnesCgram cgram,
            ushort tilemapBaseWord,
            ushort characterBaseWord,
            int rowCount,
            bool transparentColorZero = false,
            bool? priority = null)
        {
            const int width = 32 * 8;
            int height = rowCount * 8;
            var output = new Rgba32[width * height];
            SnesBgTilemapRenderer.Render2Bpp(
                output,
                vram,
                cgram,
                tilemapBaseWord,
                characterBaseWord,
                rowCount,
                transparentColorZero,
                priority);
            return output;
        }
    }
}

/// <summary>Verification access to <see cref="SnesGameplayFrameRenderer"/> members production does not use.</summary>
internal static class SnesGameplayFrameRendererAccess
{
    /// <summary>
    /// Halved color math with the X-ray fixed color outside the window. Colors are expanded BGR555,
    /// so reducing with <c>&gt;&gt; 3</c>, halving the full sum before saturation and expanding again
    /// preserves the carry.
    /// </summary>
    private static Rgba32 ApplyXrayOutsideHalfColor(Rgba32 source)
    {
        const int FixedComponent = XrayWindowRenderDefinitions.FixedColorComponent;
        return new Rgba32(
            ExpandFiveBit((byte)Math.Min(31, ((source.R >> 3) + FixedComponent) >> 1)),
            ExpandFiveBit((byte)Math.Min(31, ((source.G >> 3) + FixedComponent) >> 1)),
            ExpandFiveBit((byte)Math.Min(31, ((source.B >> 3) + FixedComponent) >> 1)),
            source.A);
    }

    private static byte ExpandFiveBit(byte value) =>
        (byte)(((value & 0x1f) << 3) | ((value & 0x1f) >> 2));

    extension(SnesGameplayFrameRenderer)
    {
        /// <summary>
        /// Inserts a ROM-derived, host-decoded room crop behind the modeled PPU HUD and objects.
        /// This is the terrain-complete debugger view while live library-background composition
        /// remains incomplete; callers can still select the all-live PPU compositor separately.
        /// </summary>
        internal static Rgba32[] RenderHudRoomAndObjs(
            SnesVram vram,
            SnesCgram cgram,
            OamBuffer oam,
            ReadOnlySpan<Rgba32> roomPixels,
            int roomWidth,
            int roomHeight,
            int cameraX,
            int cameraY,
            byte obsel = 0x03)
        {
            if (roomWidth <= 0 || roomHeight <= 0 || roomPixels.Length != checked(roomWidth * roomHeight))
                throw new ArgumentException("Room pixel buffer does not match its dimensions.", nameof(roomPixels));
            if (cameraX < 0 || cameraY < 0 || cameraX + SnesGameplayFrameRenderer.Width > roomWidth || cameraY + SnesGameplayFrameRenderer.Height > roomHeight)
                throw new ArgumentOutOfRangeException(nameof(cameraX), "Camera must contain the complete physical 256x224 viewport.");

            Rgba32[] output = SnesLayerCompositor.CreateBackdrop(cgram, SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height);
            for (int screenY = SnesGameplayFrameRenderer.HudHeight; screenY < SnesGameplayFrameRenderer.Height; screenY++)
            {
                // The HUD does not create a new BG1 coordinate system. IRQ command 4 merely
                // hides BG1 for physical scanlines 0-31, then command 6 exposes it again.
                // Consequently the first visible terrain row at screen Y=32 samples world
                // cameraY+32. This is also what the live PPU renderer expresses by adding
                // HudHeight to BG1VOFS before rendering its 192-row diagnostic surface.
                int sourceY = cameraY + screenY;
                roomPixels.Slice(sourceY * roomWidth + cameraX, SnesGameplayFrameRenderer.Width)
                    .CopyTo(output.AsSpan(screenY * SnesGameplayFrameRenderer.Width, SnesGameplayFrameRenderer.Width));
            }

            PrivateState.InvokeStatic(typeof(SnesGameplayFrameRenderer), "DrawHudAndObjects", (Rgba32[])(output), (SnesVram)(vram), (SnesCgram)(cgram), (OamBuffer)(oam), (byte)(obsel));
            return output;
        }

        /// <summary>
        /// Applies X-ray's moving window and outside-window half-color-math operation.
        /// </summary>
        /// <remarks>
        /// The cartridge writes two window endpoints per scanline through `$88:8896` and the
        /// bank-$91 `$C54B/$BD1A` geometry routines. Replaying the same angular boundaries at
        /// desktop composition time is equivalent to those WH2/WH3 writes: the two edge rays
        /// use the ROM's own 8.8 absolute-tangent table and pixels between them remain bright.
        /// `$88:817B/$81A4` then enable halved color math outside that window. The separate
        /// X-ray BG2 tilemap producer—which replaces hidden block tiles inside the window—is
        /// intentionally not claimed here; this method only owns the already-independent PPU
        /// window/color-math part of the effect.
        /// </remarks>
        internal static void ApplyXrayWindowColorMath(
            Span<Rgba32> frame,
            ISnesAddressSpace bus,
            SamusXrayState xray,
            SamusState samus,
            ushort layer1X,
            ushort layer1Y)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(xray);
            ArgumentNullException.ThrowIfNull(samus);
            if (frame.Length != SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height)
                throw new ArgumentException("X-ray compositor requires one complete 256x224 frame.", nameof(frame));

            // Setup stages have not installed `$88:86EF` yet. States three through five have
            // already replaced the active table with `$00FF` endpoints while the two backed-up
            // BG2 screens are restored, so neither interval has a visible beam to composite.
            if (!xray.IsActive || xray.SetupStage != 0 ||
                xray.BeamPhase is not (XrayBeamPhase.Widening or XrayBeamPhase.Full))
            {
                return;
            }

            bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
            SamusMovementType movementType = samus.ReadMovementType(bus);

            // `$88:88B8-$88F3` anchors the ray three pixels in front of Samus. Standing and
            // turning bodies place it sixteen pixels above the center; only stable movement
            // type five uses the crouching twelve-pixel offset.
            int originX = unchecked((short)(samus.XPosition - layer1X)) + (facingLeft ? -3 : 3);
            int originY = unchecked((short)(samus.YPosition - layer1Y)) -
                (movementType == SamusMovementType.Crouching ? 12 : 16);
            int centerAngle = xray.Angle.TableIndex;
            int angularWidth = xray.AngularWidth & 0x00ff;
            int leftEdgeAngle = (centerAngle - angularWidth) & 0x00ff;
            int rightEdgeAngle = (centerAngle + angularWidth) & 0x00ff;

            // State zero calculates a genuinely zero-width horizontal table before changing
            // state to one. `$91:C901` special-cases exactly right/left so it produces a single
            // horizontal scanline instead of using the finite `$3C00` tangent-table sentinel.
            bool horizontalLine = angularWidth == 0 &&
                (centerAngle == SnesAngle.QuarterTurn.TableIndex ||
                 centerAngle == SnesAngle.ThreeQuarterTurn.TableIndex);
            object leftEdge = ((object)(PrivateState.InvokeStatic(typeof(SnesGameplayFrameRenderer), "ReadXrayDirection", (int)(leftEdgeAngle)))!);
            object rightEdge = ((object)(PrivateState.InvokeStatic(typeof(SnesGameplayFrameRenderer), "ReadXrayDirection", (int)(rightEdgeAngle)))!);

            for (int screenY = SnesGameplayFrameRenderer.HudHeight; screenY < SnesGameplayFrameRenderer.Height; screenY++)
            {
                int fromOriginY = screenY - originY;
                int row = screenY * SnesGameplayFrameRenderer.Width;
                for (int screenX = 0; screenX < SnesGameplayFrameRenderer.Width; screenX++)
                {
                    int fromOriginX = screenX - originX;
                    bool inside;
                    if (horizontalLine)
                    {
                        inside = fromOriginY == 0 &&
                            (centerAngle == SnesAngle.QuarterTurn.TableIndex
                                ? fromOriginX >= 0
                                : fromOriginX <= 0);
                    }
                    else
                    {
                        // Super Metroid angles increase clockwise. A point is inside this
                        // always-narrower-than-180-degree cone when it is clockwise from the
                        // left edge and counter-clockwise from the right edge. Cross products
                        // retain the exact ROM tangent ratios without floating-point atan/trig.
                        long leftCross = (long)PrivateState.Property<int>(leftEdge, "X") * fromOriginY -
                            (long)PrivateState.Property<int>(leftEdge, "Y") * fromOriginX;
                        long rightCross = (long)PrivateState.Property<int>(rightEdge, "X") * fromOriginY -
                            (long)PrivateState.Property<int>(rightEdge, "Y") * fromOriginX;
                        // The assembly stores the high byte after each 8.8 accumulation, so
                        // a fractional boundary is rounded outward to its containing pixel.
                        // A ±$FF cross-product tolerance is precisely that subpixel remainder.
                        inside = leftCross >= -0x00ff && rightCross <= 0x00ff;
                    }

                    if (!inside)
                        frame[row + screenX] = ApplyXrayOutsideHalfColor(frame[row + screenX]);
                }
            }
        }
    }
}

/// <summary>Verification access to <see cref="SnesMode7Renderer"/> members production does not use.</summary>
internal static class SnesMode7RendererAccess
{
    extension(SnesMode7Renderer)
    {
        /// <summary>
        /// Renders one Mode 7 viewport using the signed 8.8 matrix/register convention.
        /// </summary>
        /// <remarks>
        /// The identity values A=D=$0100 and B=C=0 produce one source pixel per screen pixel.
        /// Outside-map policy defaults to M7SEL=$80: bit 7 disables wrapping and bit 6 leaves
        /// overflow transparent. Character-zero fill is available for the distinct $C0 mode.
        /// Wrapping selects M7SEL bit seven clear and takes precedence over character-zero fill.
        /// Transparent color zero is returned with alpha zero so OBJ/backdrop composition can
        /// retain the normal renderer contract.
        /// </remarks>
        internal static Rgba32[] RenderViewport(
            SnesVram vram,
            SnesCgram cgram,
            short matrixA,
            short matrixB,
            short matrixC,
            short matrixD,
            short centerX,
            short centerY,
            short horizontalOffset,
            short verticalOffset,
            int width = SnesPpuLayout.ScreenWidthPixels,
            int height = SnesPpuLayout.ScreenHeightPixels,
            bool fillOutsideWithCharacterZero = false, bool wrapOutsideMap = false)
        {
            ArgumentNullException.ThrowIfNull(vram);
            ArgumentNullException.ThrowIfNull(cgram);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            var output = new Rgba32[checked(width * height)];
            SnesMode7Renderer.CompositeViewport(
                output,
                vram,
                cgram,
                matrixA,
                matrixB,
                matrixC,
                matrixD,
                centerX,
                centerY,
                horizontalOffset,
                verticalOffset,
                width,
                height,
                fillOutsideWithCharacterZero, wrapOutsideMap);
            return output;
        }
    }
}

/// <summary>Verification access to <see cref="SnesObjRenderer"/> members production does not use.</summary>
internal static class SnesObjRendererAccess
{
    extension(SnesObjRenderer)
    {
        /// <summary>
        /// Renders finalized OAM over a transparent canvas. Color index zero remains
        /// transparent, matching the OBJ-specific transparency rule in the SNES PPU.
        /// </summary>
        internal static Rgba32[] Render(
            OamBuffer oam,
            SnesVram vram,
            SnesCgram cgram,
            byte obsel,
            int width = SnesPpuLayout.ScreenWidthPixels,
            int height = SnesPpuLayout.ScreenHeightPixels,
            int? priority = null)
        {
            ArgumentNullException.ThrowIfNull(oam);
            ArgumentNullException.ThrowIfNull(vram);
            ArgumentNullException.ThrowIfNull(cgram);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
            var output = new Rgba32[checked(width * height)];
            SnesObjRenderer.Render(output, oam, vram, cgram, obsel, width, height, priority);
            return output;
        }

        /// <summary>
        /// Resolves the winning OBJ and its BG-relative priority for every output pixel in one
        /// OAM walk.
        /// </summary>
        /// <remarks>
        /// Ordinary gameplay needs to interleave four OBJ priority groups with BG1 and BG2.
        /// Calling <see cref="Render"/> four times is correct but needlessly decodes every
        /// sprite tile four times and allocates four RGBA canvases. This form performs the
        /// expensive character decode once, stores only the winning color plus its priority,
        /// and lets the PPU compositor place that winner at the appropriate point in its BG
        /// ladder. Empty pixels retain <see cref="SnesObjRenderer.TransparentPriority"/>.
        /// </remarks>
        internal static ResolvedObjFrame RenderResolved(
            OamBuffer oam,
            SnesVram vram,
            SnesCgram cgram,
            byte obsel,
            int width = SnesPpuLayout.ScreenWidthPixels,
            int height = SnesPpuLayout.ScreenHeightPixels)
        {
            ArgumentNullException.ThrowIfNull(oam);
            ArgumentNullException.ThrowIfNull(vram);
            ArgumentNullException.ThrowIfNull(cgram);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            var pixels = new Rgba32[checked(width * height)];
            var priorities = new byte[pixels.Length];
            SnesObjRenderer.RenderResolved(
                oam,
                vram,
                cgram,
                obsel,
                pixels,
                priorities,
                width,
                height);

            return new ResolvedObjFrame(pixels, priorities);
        }
    }
}
