using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Exact state and one-sprite renderer for Samus's opening arm cannon.
/// </summary>
/// <remarks>
/// The cannon is not baked into every Samus body frame. Missile, Super Missile, and
/// grapple HUD selections open a four-state cover, then the draw places the cover's
/// direction- and frame-specific picture separately at the pose's placement (#1463).
/// Keeping this independent from pose art preserves its before/after-body priority.
/// </remarks>
public sealed class SamusArmCannonState
{
    /// <summary>Previous HUD selection used to delay cover transitions until the choice stabilizes.</summary>
    internal HudItem PreviousSelectedHudItem { get; set; }

    /// <summary>Installed arm-cannon artwork: installed content, not game state, bound again when a game is restored.</summary>
    public InstalledSamusArmCannonArtwork? Artwork { get; set; }

    /// <summary>Whether the cover is open or opening; false while closed or closing.</summary>
    public bool IsOpen { get; internal set; }

    /// <summary>Whether an opening or closing is still under way.</summary>
    public bool IsTransitioning { get; internal set; }

    /// <summary>
    /// The cover frame, 0 through 4; zero is invisible, one and two transition, three fully open, and a closing starts
    /// from four, which the same update steps to three. The setter rejects any other value.
    /// </summary>
    public int Frame
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ClosingStartFrame, value);
    }

    /// <summary>The frame a closing starts from, past the fully open frame: the cover's largest frame.</summary>
    private const int ClosingStartFrame = 4;

    /// <summary>
    /// The updates the HUD selection has held still, 0 through 2: a change restarts it at one and it saturates at two,
    /// when a transition may begin.
    /// </summary>
    public int StableSelectionUpdates
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, 2, value);
    }

    /// <summary>Where this frame's cover is drawn relative to the body, from the pose's drawing mode.</summary>
    public SamusArmCannonDrawOrder DrawOrder { get; internal set; }

    /// <summary>
    /// Runs the HUD selection stability update and the arm cannon's open-state update. Calling once
    /// per gameplay frame reproduces the selection stability update followed later by the cover update.
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
            StableSelectionUpdates = Math.Min(StableSelectionUpdates + 1, 2);
        }

        if (IsTransitioning || TryStartTransition(samus.SelectedHudItem))
            AdvanceFrame();

        DrawOrder = Placement(samus.Pose).DrawOrder;
        return new SamusArmCannonUpdateResult();
    }

    /// <summary>
    /// Draws the direction-specific small sprite showing its selected cover frame. The caller chooses
    /// before/after-body order from <see cref="DrawOrder"/>.
    /// </summary>
    public SamusArmCannonDrawResult Draw(
        SpriteFrameBuffer sprites,
        SamusState samus,
        Position16<RoomPixels> cameraPosition,
        int frameCounter)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(samus);

        // The cannon shares Samus's invincibility flicker, and a closed frame performs
        // neither sprite nor artwork work. The game returns before even consulting pose data.
        if (Frame == 0 || (!samus.InvincibilityTimer.IsZero && SamusCadenceDefinitions.InvincibilityFlickerHidden.IsSetIn(frameCounter)))
            return new SamusArmCannonDrawResult();

        SamusArmCannonPlacement placement = Placement(samus.Pose);
        SamusProjectileDirection direction = placement.Direction(samus.AnimationFrame);

        Offset16<RoomPixels> offset = placement.Offset(samus.AnimationFrame);
        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);

        // The cover is published wherever it lies, off the screen included; the display clips it (#1587).
        int screenX = CoordinateAxis.Wrap(samus.Position.X.Whole + offset.X - cameraPosition.X);
        int screenY = CoordinateAxis.Wrap(
            samus.Position.Y.Whole + offset.Y - graphicsYOffset - cameraPosition.Y);
        // Selector -> one of four orientation lists -> current cover frame, whose sheet frame is that cover as
        // the sprite shows it. Frame zero, the null entry, returned above.
        InstalledSpriteSheets sheets = samus.Presentation.Sheets ?? throw new InvalidOperationException(
            "Arm cannon requires the installed sprite sheets.");
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusArmCannon),
            sheets.Get(SpriteSheetId.SamusOverlays).Frame(SamusOverlaySheetDefinitions.ArmCannonFrame(direction, Frame)),
            new Position16<ScreenPixels>(screenX, screenY), SpriteMirror.None));

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
        Frame = opens ? 0 : ClosingStartFrame;
        IsOpen = opens;
        IsTransitioning = true;
        return true;
    }

    /// <summary>Where the installed artwork places the cover for <paramref name="pose"/>.</summary>
    private SamusArmCannonPlacement Placement(SamusPoseId pose) =>
        (Artwork ?? throw new InvalidOperationException(
            "Arm cannon requires installed drawing definitions.")).Placement(pose);

    /// <summary>Moves the cover one frame toward open three or closed zero, then clears the transition flag.</summary>
    private void AdvanceFrame()
    {
        if (IsOpen)
        {
            if (Frame + 1 < 3)
            {
                Frame++;
                return;
            }

            Frame = 3;
        }
        else
        {
            if (Frame > 1)
            {
                Frame--;
                return;
            }

            Frame = 0;
        }

        // The cover ends the transition while retaining the open state.
        IsTransitioning = false;
    }

}
