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

    /// <summary>Whether the cover is open or opening; false while closed or closing.</summary>
    public bool IsOpen { get; internal set; }

    /// <summary>Whether an opening or closing is still under way.</summary>
    public bool IsTransitioning { get; internal set; }

    /// <summary>The cover frame, 0 through 65535; zero is invisible, one and two transition, three fully open.</summary>
    public int Frame
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The updates the HUD selection has held still, 0 through 65535; a change restarts it at one and it saturates at
    /// two, when a transition may begin.
    /// </summary>
    public int StableSelectionUpdates
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Where this frame's cover is drawn relative to the body, from the pose's drawing mode.</summary>
    public SamusArmCannonDrawOrder DrawOrder { get; internal set; }

    /// <summary>
    /// The draw order a pose's drawing mode selects. The drawing handler reads only the mode's low nibble: zero hides
    /// the cover, two draws it after the body and every other value draws it before the body.
    /// </summary>
    internal static SamusArmCannonDrawOrder DrawOrderOf(int drawingMode) => (drawingMode & 15) switch
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
            StableSelectionUpdates = 1;
            PreviousSelectedHudItem = samus.SelectedHudItem;
        }
        else
        {
            int incremented = unchecked((ushort)(StableSelectionUpdates + 1));
            StableSelectionUpdates = incremented < 3 ? incremented : 2;
        }

        if (IsTransitioning || TryStartTransition(samus.SelectedHudItem))
            AdvanceFrame();

        DrawOrder = DrawOrderOf(Placement(samus.Pose).DrawingMode);
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
        if (StableSelectionUpdates < 2)
            return false;

        bool opens = SamusArmCannonOpening.Opens(selectedHudItem);
        if (IsOpen == opens)
            return false;

        // Closing begins from synthetic frame four; the same call then advances it to
        // visible frame three.
        Frame = opens ? 0 : 4;
        IsOpen = opens;
        IsTransitioning = true;
        return true;
    }

    /// <summary>Where the installed artwork places the cover for <paramref name="pose"/>.</summary>
    private SamusArmCannonPlacement Placement(SamusPoseId pose) =>
        (Artwork ?? throw new InvalidOperationException(
            "Arm cannon requires installed drawing definitions.")).Placement(pose);

    /// <summary>Moves the cover one native frame toward open three or closed zero, then clears the transition flag.</summary>
    private void AdvanceFrame()
    {
        if (IsOpen)
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

        // `$90:C658` ends the transition while retaining the open state.
        IsTransitioning = false;
    }

}
