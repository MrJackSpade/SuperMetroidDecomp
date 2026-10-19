using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Rendering;

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

    /// <summary>Replicates a five-bit color component into eight-bit display space after X-ray half-color math.</summary>
    private static byte ExpandFiveBit(byte value) =>
        (byte)(((value & 0x1f) << 3) | ((value & 0x1f) >> 2));

    /// <summary>Exposes debugger room composition and the independent X-ray window/color-math projection.</summary>
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
            SpriteFrameBuffer sprites,
            ReadOnlySpan<Rgba32> roomPixels,
            Extent16<RoomPixels> roomSize,
            Position16<RoomPixels> camera)
        {
            int roomWidth = roomSize.X, roomHeight = roomSize.Y;
            if (roomWidth == 0 || roomHeight == 0 || roomPixels.Length != roomWidth * roomHeight)
                throw new ArgumentException("Room pixel buffer does not match its dimensions.", nameof(roomPixels));
            if (camera.X + SnesGameplayFrameRenderer.Width > roomWidth || camera.Y + SnesGameplayFrameRenderer.Height > roomHeight)
                throw new ArgumentOutOfRangeException(nameof(camera), "Camera must contain the complete physical 256x224 viewport.");

            Rgba32[] output = SnesLayerCompositor.CreateBackdrop(cgram, SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height);
            for (int screenY = SnesGameplayFrameRenderer.HudHeight; screenY < SnesGameplayFrameRenderer.Height; screenY++)
            {
                // The HUD does not create a new BG1 coordinate system. IRQ command 4 merely
                // hides BG1 for physical scanlines 0-31, then command 6 exposes it again.
                // Consequently the first visible terrain row at screen Y=32 samples world
                // cameraY+32. This is also what the live PPU renderer expresses by adding
                // HudHeight to BG1VOFS before rendering its 192-row diagnostic surface.
                int sourceY = camera.Y + screenY;
                roomPixels.Slice(sourceY * roomWidth + camera.X, SnesGameplayFrameRenderer.Width)
                    .CopyTo(output.AsSpan(screenY * SnesGameplayFrameRenderer.Width, SnesGameplayFrameRenderer.Width));
            }

            SnesGameplayFrameRendererConstants.DrawHudAndObjects((Rgba32[])(output), (SnesVram)(vram), (SnesCgram)(cgram), sprites);
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
            Position16<RoomPixels> layer1)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(xray);
            Ensure.NotNull(samus);
            Ensure.LengthEqual(frame, SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height);

            // Setup stages have not installed `$88:86EF` yet. States three through five have
            // already replaced the active table with `$00FF` endpoints while the two backed-up
            // BG2 screens are restored, so neither interval has a visible beam to composite.
            if (!xray.IsActive || xray.SetupStage != 0 ||
                xray.BeamPhase is not (XrayBeamPhase.Widening or XrayBeamPhase.Full))
            {
                return;
            }

            bool facingLeft = samus.PoseMetadata.ReadPoseXDirection() == 4;
            SamusMovementType movementType = samus.PoseMetadata.ReadMovementType();

            // `$88:88B8-$88F3` anchors the ray three pixels in front of Samus. Standing and
            // turning bodies place it sixteen pixels above the center; only stable movement
            // type five uses the crouching twelve-pixel offset.
            Offset16<ScreenPixels> samusOnScreen = samus.Position.Whole.RelativeToCamera(layer1);
            int originX = samusOnScreen.X + (facingLeft ? -3 : 3);
            int originY = samusOnScreen.Y -
                (movementType == SamusMovementType.Crouching ? 12 : 16);
            int centerAngle = xray.Angle.TableIndex;
            int angularWidth = xray.AngularWidth;
            int leftEdgeAngle = unchecked((byte)(centerAngle - angularWidth));
            int rightEdgeAngle = unchecked((byte)(centerAngle + angularWidth));

            // State zero calculates a genuinely zero-width horizontal table before changing
            // state to one. `$91:C901` special-cases exactly right/left so it produces a single
            // horizontal scanline instead of using the finite `$3C00` tangent-table sentinel.
            bool horizontalLine = angularWidth == 0 &&
                (centerAngle == SnesAngle.QuarterTurn.TableIndex ||
                 centerAngle == SnesAngle.ThreeQuarterTurn.TableIndex);
            var leftEdge = SnesGameplayFrameRenderer.ReadXrayDirection((int)(leftEdgeAngle));
            var rightEdge = SnesGameplayFrameRenderer.ReadXrayDirection((int)(rightEdgeAngle));

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
                        long leftCross = (long)leftEdge.X * fromOriginY - (long)leftEdge.Y * fromOriginX;
                        long rightCross = (long)rightEdge.X * fromOriginY - (long)rightEdge.Y * fromOriginX;
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
