using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Exact state and one-OBJ renderer for Samus's opening arm cannon at
/// <c>$90:C5C4-$90:C790</c>.
/// </summary>
/// <remarks>
/// The cannon is not baked into every Samus body frame. Missile, Super Missile, and
/// grapple HUD selections open a four-state cover, then the draw routine places the cover's
/// direction- and frame-specific picture separately at the pose's placement (#1463).
/// Keeping this independent from pose art preserves its native before/after-body priority.
/// </remarks>
public sealed class SamusArmCannonState
{
    /// <summary>Previous HUD selection used to delay cover transitions until the choice stabilizes.</summary>
    internal HudItem PreviousSelectedHudItem { get; set; }

    /// <summary>Host-owned visual data; rebound after a restored debugger state.</summary>
    [field: NonSerialized]
    public InstalledSamusArmCannonArtwork? Artwork { get; set; }

    /// <summary>Low byte of WRAM `$0AA6`, 0 through 255: zero closed, one open/opening.</summary>
    public int OpenFlag
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>High byte at WRAM `$0AA7`, 0 through 255: one while either transition is incomplete.</summary>
    public int CloseFlag
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>WRAM `$0AA8`, 0 through 65535; zero is invisible, one/two transition, three fully open.</summary>
    public int Frame
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>WRAM `$0AAA`, 0 through 65535; the HUD selection producer saturates this at two.</summary>
    public int ToggleFlag
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Pose-record byte one copied to WRAM `$0AAC` on every draw pass, 0 through 65535.</summary>
    public int DrawingMode
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// Where this frame's cover is drawn relative to the body. The full source byte is retained in
    /// <see cref="DrawingMode"/> because `$90:C5E0-$C5E6` stores it unmasked; only the drawing handler
    /// reads its low nibble, where zero hides the cover, two draws it after the body and every other
    /// value draws it before the body.
    /// </summary>
    public SamusArmCannonDrawOrder DrawOrder => (DrawingMode & 15) switch
    {
        0 => SamusArmCannonDrawOrder.Hidden,
        2 => SamusArmCannonDrawOrder.AfterBody,
        _ => SamusArmCannonDrawOrder.BeforeBody,
    };

    /// <summary>
    /// Runs the HUD selection stability update and `HandleArmCannonOpenState`. Calling once
    /// per gameplay frame reproduces `$90:C519-$C534` followed later by `$90:C5C4`.
    /// </summary>
    public SamusArmCannonUpdateResult Update(SamusState samus)
    {
        Ensure.NotNull(samus);
        bool itemChanged = samus.SelectedHudItem != PreviousSelectedHudItem;
        if (itemChanged)
        {
            ToggleFlag = 1;
            PreviousSelectedHudItem = samus.SelectedHudItem;
        }
        else
        {
            int incremented = unchecked((ushort)(ToggleFlag + 1));
            ToggleFlag = incremented < 3 ? incremented : 2;
        }

        if (CloseFlag != 0 || TryStartTransition(samus.SelectedHudItem))
            AdvanceFrame();

        DrawingMode = Placement(samus.Pose).DrawingMode;
        return new SamusArmCannonUpdateResult();
    }

    /// <summary>
    /// Draws the direction-specific small OBJ bound to its selected cover character. The caller chooses
    /// before/after-body order from <see cref="DrawOrder"/>.
    /// </summary>
    public SamusArmCannonDrawResult Draw(
        SpriteFrameBuffer sprites,
        SamusState samus,
        Position16<RoomPixels> layer1,
        int frameCounter)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(samus);

        // The cannon shares Samus's invincibility flicker, and a closed frame performs
        // neither OAM nor DMA work. Native returns before even consulting pose data.
        if (Frame == 0 || (!samus.InvincibilityTimer.IsZero && (frameCounter & 1) != 0))
            return new SamusArmCannonDrawResult();

        SamusArmCannonPlacement placement = Placement(samus.Pose);
        int selector = placement.Direction(samus.AnimationFrame);
        if (selector >= SamusDrawingDefinitions.ArmCannon.DirectionCount)
            throw new InvalidDataException($"Arm-cannon direction selector {selector} is outside 0..9.");

        Offset16<RoomPixels> offset = placement.Offset(samus.AnimationFrame);
        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);

        int screenX = unchecked((short)(samus.Position.X.Whole + offset.X - layer1.X));
        int screenY = unchecked((short)(
            samus.Position.Y.Whole + offset.Y - graphicsYOffset - layer1.Y));
        bool spriteWritten = screenX >= 0 && screenX < 256 && screenY >= 0 && screenY < 256;
        if (spriteWritten)
        {
            // Selector -> one of four orientation lists -> current cover frame. Natively `$90:C7DF` uploads that
            // character to the tile the sprite names; the sheet frame is that cover as the sprite shows it. Frame
            // zero, the null entry, returned above.
            InstalledSpriteSheets sheets = samus.Presentation.BodyFrames.Sheets ?? throw new InvalidOperationException(
                "Arm cannon requires the installed sprite sheets.");
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusArmCannon),
                sheets.Get(SpriteSheetId.SamusOverlays).Frame(SamusOverlaySheetDefinitions.ArmCannonFrame(selector, Frame)),
                new Position16<ScreenPixels>(unchecked((ushort)screenX), unchecked((ushort)screenY)), SpriteMirror.None));
        }

        return new SamusArmCannonDrawResult();
    }

    /// <summary>Starts opening or closing only after the HUD item has been stable for two updates.</summary>
    private bool TryStartTransition(HudItem selectedHudItem)
    {
        if (ToggleFlag < 2)
            return false;

        int desiredOpenFlag = SamusArmCannonOpening.DesiredOpenFlag(selectedHudItem);
        if (OpenFlag == desiredOpenFlag)
            return false;

        // The packed native word write changes the low open byte and sets high close byte
        // one simultaneously. Closing begins from synthetic frame four; the same call then
        // advances it to visible frame three.
        Frame = desiredOpenFlag != 0 ? 0 : 4;
        OpenFlag = desiredOpenFlag;
        CloseFlag = 1;
        return true;
    }

    /// <summary>Where the installed artwork places the cover for <paramref name="pose"/>.</summary>
    private SamusArmCannonPlacement Placement(SamusPoseId pose) =>
        (Artwork ?? throw new InvalidOperationException(
            "Arm cannon requires installed drawing definitions.")).Placement(pose);

    /// <summary>Moves the cover one native frame toward open three or closed zero, then clears the transition flag.</summary>
    private void AdvanceFrame()
    {
        if (OpenFlag != 0)
        {
            int incremented = unchecked((ushort)(Frame + 1));
            if (unchecked((short)(incremented - 3)) < 0)
            {
                Frame = incremented;
                return;
            }

            Frame = 3;
        }
        else
        {
            int decremented = unchecked((ushort)(Frame - 1));
            if (decremented != 0 && unchecked((short)decremented) >= 0)
            {
                Frame = decremented;
                return;
            }

            Frame = 0;
        }

        // `$90:C658` stores the low byte as a 16-bit word, clearing only the transition
        // byte while retaining the desired open state.
        CloseFlag = 0;
    }

}
