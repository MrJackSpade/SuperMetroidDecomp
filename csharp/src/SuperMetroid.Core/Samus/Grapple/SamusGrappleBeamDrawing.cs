using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the grapple beam draws: its flare, the connected beam's segments and its endpoint.</summary>
public static class SamusGrappleBeamDrawing
{

    /// <summary>
    /// Reports whether grapple's replacement Samus draw-handler pointer is still installed.
    /// </summary>
    /// <remarks>
    /// Cancellation, release, drop, and grapple wall-jump are one-call teardown functions.
    /// On the frame that installs one of them, `$90:EB86` is still the selected handler even
    /// though its signed function-pointer test falls back to the ordinary body/echo path.
    /// Keeping that distinction explicit prevents the normal charge flare from leaking into
    /// the teardown frame; `$90:EB52`, which owns that flare, has not been restored yet.
    /// </remarks>
    public static bool UsesGrappleDrawingHandler(GrapplePhase phase) =>
        phase != GrapplePhase.Inactive;


    /// <summary>
    /// Mirrors `$90:EB86`'s signed function-pointer range test for the beam-specific path.
    /// </summary>
    public static bool UsesBeamSpecificDrawingPath(GrapplePhase phase) => phase is
        GrapplePhase.Firing or
        GrapplePhase.ConnectedSwinging or
        GrapplePhase.ConnectedLocked or
        GrapplePhase.WallGrab or
        GrapplePhase.WallGrabRelease;


    /// <summary>
    /// Ports <c>HandleGrappleBeamFlare</c> at `$9B:C036`. Call before atmosphere and Samus,
    /// exactly where the active half of `$90:EB86` calls it.
    /// </summary>
    /// <returns>True when the flare origin passed the native vertical visibility test.</returns>
    public static bool DrawFlareBeforeSamus(
        SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        ProjectileDrawArtwork objects)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(sprites);

        SamusGrappleState grapple = samus.Grapple;
        if (!UsesBeamSpecificDrawingPath(grapple.Phase) || grapple.FlareCounter == 0)
            return false;

        // While firing, `$90:EB86` calls `$9B:BF1B` immediately before the flare. Grapple
        // physics ran before ordinary Samus movement, so the renderer must reconstruct both
        // hand origins from the final body position or a running shot visibly lags one frame.
        // Connected functions already publish their exact rope/flare pair during movement.
        if (grapple.Phase == GrapplePhase.Firing)
            RefreshFiringDrawOrigins(samus, grapple);

        // Counter one is a sentinel, not merely the first ordinary animation tick. Native
        // code force-selects main-flare frame 16 and timer three on every such call, then
        // performs the usual 16-bit DEC/BMI. The first visible frame therefore stores two.
        if (grapple.FlareCounter == 1)
        {
            grapple.FlareAnimationFrame = ChargeFlareAnimationDefinitions.GrappleInitialFrame;
            grapple.FlareAnimationTimer = WrappingCountdown.Start(ChargeFlareAnimationDefinitions.GrappleInitialDelay);
        }

        grapple.FlareAnimationTimer = grapple.FlareAnimationTimer.Decrement().Remaining;
        if (grapple.FlareAnimationTimer.IsNegative)
        {
            grapple.FlareAnimationFrame = StepFlareFrame(grapple.FlareAnimationFrame, 1);
            int delay = ChargeFlareAnimationDefinitions.ReadMainFlareDelay(grapple.FlareAnimationFrame);
            if (ChargeFlareAnimationDefinitions.TryDecodeCommand(delay, out ChargeFlareDelayCommand command) &&
                command == ChargeFlareDelayCommand.Rewind)
            {
                // `$FE,n` is the compact loop command in the shared delay bytecode. The
                // subtraction applies to the already-incremented frame word and wraps like
                // 16-bit ADC/SBC; malformed ROM data remains visible instead of clamped.
                int rewind = ChargeFlareAnimationDefinitions.ReadMainFlareDelay(StepFlareFrame(grapple.FlareAnimationFrame, 1));
                grapple.FlareAnimationFrame = StepFlareFrame(grapple.FlareAnimationFrame, -rewind);
                delay = ChargeFlareAnimationDefinitions.ReadMainFlareDelay(grapple.FlareAnimationFrame);
            }

            grapple.FlareAnimationTimer = WrappingCountdown.FromRaw(delay);
        }

        int screenX = unchecked((ushort)(grapple.BeamStart.X - layer1.X));
        int screenY = unchecked((ushort)(grapple.BeamStart.Y - layer1.Y));
        // The assembly tests only Y's high byte. X is allowed to wrap into high OAM, while
        // any Y outside unsigned screen rows `$00-$FF` suppresses the whole spritemap.
        if (ScreenConversions.IsOutsideSpriteRange(screenY))
            return false;

        // The first word of both native facing rows is zero. Grapple uses the
        // shared main flare only; charge-spark direction offsets do not apply.
        int tableIndex = unchecked((ushort)(
            grapple.FlareAnimationFrame + SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions.MainFlareSelectorOffset));
        // Flare compositions name characters $50-$5D of the standard OBJ sheet, which no beam or trail upload covers,
        // so the loaded beams' projectile sheet shows them as the standard page does.
        if (!SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteCatalog.Contains(tableIndex))
            throw new InvalidDataException($"Unknown charge-flare sprite selector {tableIndex:X4}.");
        SpriteSheetFrame image = objects.Sheet.Frame(SuperMetroid.Core.Samus.Artwork.ProjectileSheetDefinitions.FlareFrame(
            SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions.Selectors[tableIndex]));
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleFlare), image,
            Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
        return true;
    }


    /// <summary>Seeds the first flare and point frames and all sixteen independently timed rope segment slots.</summary>
    internal static void InitializeBeamAnimation(SamusGrappleState grapple)
    {
        // `$9B:C51E` installs counter one after initializing the rope instruction slots.
        // The debugger's already-connected seam has no preceding firing history, so it
        // begins at the same authentic first flare state rather than inventing a static OBJ.
        grapple.FlareCounter = 1;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = WrappingCountdown.Start(0);
        grapple.PointAnimationTimer = WrappingCountdown.Start(GrapplePointAnimationDefinitions.Delay);
        grapple.PointAnimationFrame = 0;
        // GrappleFunc_AF87 at $94:AF87 seeds sixteen independent segment instruction
        // slots. Slot 15 starts on tile $24, slot 14 on $23, slot 13 on $22, slot 12 on
        // $21, and that four-phase pattern repeats down to zero. Timer one makes $94:AFBA
        // consume each initial record on its first draw.
        for (int slot = 0; slot < grapple.SegmentAnimationFrames.Length; slot++)
        {
            grapple.SegmentAnimationTimers[slot] = WrappingCountdown.Start(1);
            grapple.SegmentAnimationFrames[slot] = unchecked((byte)(slot & 3));
            grapple.SegmentAnimationStarted[slot] = false;
        }
    }


    /// <summary>Recomputes visual beam origins from the final pose while retaining the separate physical endpoint offset.</summary>
    internal static void RefreshFiringDrawOrigins(
        SamusState samus,
        SamusGrappleState grapple)
    {
        bool useRunOffsets = SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Running;
        var origin = GrappleFiringDefinitions.Origin(grapple.FireDirection.ToAimIndex(), useRunOffsets);
        var flare = SamusGrappleMovement.ReadFlareOrigin(grapple, grapple.FireDirection.ToAimIndex(), useRunOffsets);
        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);
        int physicalYOffset = SamusPoseProjectileOriginDefinitions.ReadYOffset(samus.Pose);

        // Recompute from the final pose/position, not cached launch offsets. Physical
        // Start and presentation Flare remain separate coordinate pairs.
        grapple.RopeStart = samus.Position.Whole + new Offset16<RoomPixels>(
            origin.X, unchecked((short)(origin.Y - physicalYOffset)));
        grapple.BeamStart = samus.Position.Whole + new Offset16<RoomPixels>(
            flare.X, unchecked((short)(flare.Y - graphicsYOffset)));
    }


    /// <summary>
    /// Moves the main-flare stream position by <paramref name="delta"/> as the cartridge's sixteen-bit ADC/SBC does:
    /// the position wraps through 65535 and zero, so malformed rewind data stays visible instead of clamped.
    /// </summary>
    private static int StepFlareFrame(int frame, int delta) => unchecked((ushort)(frame + delta));


    /// <summary>
    /// Ports the post-Samus tile-selection and small-OBJ portion of `$90:EB86/$9B:BFA5/$94:AFBA`: the endpoint
    /// animation and rope orientation select the beam's characters, which its draws bind.
    /// </summary>
    public static void DrawConnectedBeam(
        SamusGrappleState grapple,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        SpriteSheet? sheet = null,
        SamusPoseId samusPose = 0)
    {
        Ensure.NotNull(grapple);
        Ensure.NotNull(sprites);
        if (!UsesBeamSpecificDrawingPath(grapple.Phase))
            return;

        // The native pair is a begin/end range, not two animation frames. Each
        // six-call interval advances the source by $200 through four endpoint tiles.
        GrapplePointAnimationDefinitions.Advance(grapple);
        // Natively `$9B:BFA5` uploads the endpoint frame and the four rope characters for the angle's sector to
        // OBJ characters $20-$24; the draws below bind that selection over the standard OBJ page.
        SpriteSheet installed = sheet ?? throw new InvalidOperationException("The grapple beam requires the installed grapple sheet.");
        SuperMetroid.Core.Samus.Artwork.GrappleRopeOrientation orientation =
            SuperMetroid.Core.Samus.Artwork.GrappleTileDefinitions.OrientationFor(grapple.Angle);

        // `$9B:BFA5` increments the shared flare counter after both tile records, saturating
        // at 120 by a signed comparison. It happens even at zero rope length; only the OAM
        // rope renderer below is conditional. This is why a newly fired beam can animate
        // its muzzle flare before its first eight-pixel body segment exists.
        if (grapple.FlareCounter < 120)
            grapple.FlareCounter++;
        if (grapple.RopeLength <= 0)
            return;

        // $94:AFCF recalculates the draw vector from endpoint-minus-flare geometry. This
        // is observably different from merely reversing EndAngle while firing, and it also
        // keeps the rope visually attached after pose-specific art-origin correction.
        RotationAngle drawAngle = SamusGrappleMovement.CalculateAngleFromXY(grapple.Anchor - grapple.BeamStart);
        // Preserve the fractional word across segments. Rounding each individual
        // displacement shortens diagonals and rounds negative steps incorrectly.
        int stepX = SamusGrappleSwing.ReadSignedSine(drawAngle.AddRaw(RotationAngle.QuarterTurn.RawValue).TableIndex)
            * GrappleBeamDefinitions.Rendering.SegmentSampleToFixedPoint;
        int stepY = SamusGrappleSwing.ReadSignedSine(drawAngle.TableIndex)
            * GrappleBeamDefinitions.Rendering.SegmentSampleToFixedPoint;

        // $94:AFDE derives X/Y flip bits from the grapple angle while retaining the packed
        // palette-five/priority-three instruction word. Tile $20 is the connected endpoint.
        int angleHigh = grapple.Angle.TableIndex;
        int flipBits = (angleHigh & 1 << 7) >> 1;
        flipBits |= 2 * ((((angleHigh ^ flipBits) & 1 << 6) ^ 1 << 6));
        SpriteMirror mirror = ((flipBits & 1 << 6) != 0 ? SpriteMirror.Horizontal : SpriteMirror.None) |
            ((flipBits & 1 << 7) != 0 ? SpriteMirror.Vertical : SpriteMirror.None);
        int screenXFixed = unchecked(SixteenSixteen.FromWhole(grapple.BeamStart.X - layer1.X - GrappleBeamDefinitions.Rendering.CharacterCenterOffset));
        int screenYFixed = unchecked(SixteenSixteen.FromWhole(grapple.BeamStart.Y - layer1.Y - GrappleBeamDefinitions.Rendering.CharacterCenterOffset));
        // Native enters the loop before decrementing quotient-minus-one: even a
        // positive length whose masked quotient is zero visits one segment.
        int segmentCount = Math.Max(1, (grapple.RopeLength / GrappleBeamDefinitions.Rendering.SegmentSpacing)
            & GrappleBeamDefinitions.Rendering.SegmentCountMask);
        for (int segment = 0; segment < segmentCount; segment++)
        {
            // $94:AFBA walks instruction slots from 15 downward regardless of rope length.
            // The first timer expiry reads the already-selected initial record; later
            // expiries advance through $21,$22,$23,$24 and $94:B0F4's goto back to $21.
            // Keeping the one-time first expiry explicit preserves the native five-draw
            // interval without pretending that all rope pieces share one animation frame.
            int instructionSlot = GrappleBeamDefinitions.Rendering.FirstSegmentSlot - segment;
            if ((grapple.SegmentAnimationTimers[instructionSlot] = grapple.SegmentAnimationTimers[instructionSlot].Decrement().Remaining).IsZero)
            {
                grapple.SegmentAnimationTimers[instructionSlot] = WrappingCountdown.Start(GrappleBeamDefinitions.Rendering.SegmentAnimationDelay);
                if (grapple.SegmentAnimationStarted[instructionSlot])
                {
                    grapple.SegmentAnimationFrames[instructionSlot] = unchecked((byte)(
                        (grapple.SegmentAnimationFrames[instructionSlot] + 1) % GrappleBeamDefinitions.Rendering.SegmentAnimationFrameCount));
                }
                else
                {
                    grapple.SegmentAnimationStarted[instructionSlot] = true;
                }
            }

            int screenX = FixedCoordinate.WholeOf(screenXFixed);
            int screenY = FixedCoordinate.WholeOf(screenYFixed);
            // Native ticks this slot before the visibility check, then abandons
            // the remaining rope slots entirely at the first off-screen segment.
            if ((screenX | screenY) >= GrappleBeamDefinitions.Rendering.ViewportExtent)
                break;
            // `$94:AFDE`'s direction flips are ORed into the segment's own; the sheet holds each combination.
            SpriteSheetFrame image = installed.Frame(SuperMetroid.Core.Samus.Artwork.GrappleSheetDefinitions.RopeFrame(
                orientation, grapple.SegmentAnimationFrames[instructionSlot], mirror));
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleRope, segment), image,
                Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
            screenXFixed = unchecked(screenXFixed + stepX);
            screenYFixed = unchecked(screenYFixed + stepY);
        }

        DrawBeamEndpoint(grapple, sprites, installed.Frame(SuperMetroid.Core.Samus.Artwork.GrappleSheetDefinitions.EndpointFrame(
            grapple.PointAnimationFrame)), layer1, samusPose);
    }


    /// <summary>Preserves the pose-selected endpoint routines at $94:B0F9/B14B.</summary>
    private static void DrawBeamEndpoint(SamusGrappleState grapple, SpriteFrameBuffer sprites, SpriteSheetFrame image,
        Position16<RoomPixels> layer1, SamusPoseId samusPose)
    {
        bool swinging = samusPose is SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose;
        int relativeY = unchecked((ushort)(grapple.Anchor.Y - layer1.Y));
        // The ordinary endpoint tests its center before subtracting the sprite
        // half-width. Swinging deliberately bypasses this vertical rejection.
        if (!swinging && ScreenConversions.IsOutsideSpriteRange(relativeY))
            return;

        // Only the swinging routine omits SEC between its two SBC operations.
        // Consequently unsigned camera subtraction can carry a borrow into the
        // half-character offset, even when wrapped coordinates look adjacent.
        int borrowX = swinging && grapple.Anchor.X < layer1.X ? 1 : 0;
        int borrowY = swinging && grapple.Anchor.Y < layer1.Y ? 1 : 0;
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleEndpoint), image,
            new Position16<ScreenPixels>(unchecked((ushort)(grapple.Anchor.X - layer1.X - GrappleBeamDefinitions.Rendering.CharacterCenterOffset - borrowX)), unchecked((ushort)(relativeY - GrappleBeamDefinitions.Rendering.CharacterCenterOffset - borrowY))),
            SpriteMirror.None));
    }


    /// <summary>Clears the three charge-flare words shared by grapple cancellation and connection cleanup.</summary>
    internal static void ClearFlareAnimation(SamusGrappleState grapple)
    {
        // `$9B:C856/$C8C5/$C9CE/$CB8B` all clear these same shared charge/grapple WRAM
        // words before restoring the ordinary draw handler and projectile palette.
        grapple.FlareCounter = 0;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = WrappingCountdown.Start(0);
    }
}
