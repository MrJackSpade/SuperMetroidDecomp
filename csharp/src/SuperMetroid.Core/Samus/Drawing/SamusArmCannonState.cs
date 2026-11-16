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

    /// <summary>The installed artwork the cover is placed by: installed content, not game state.</summary>
    private readonly SamusArtworkBinding _artwork;

    /// <summary>Starts a closed cover placed by <paramref name="artwork"/>'s arm-cannon placements.</summary>
    internal SamusArmCannonState(SamusArtworkBinding artwork) => _artwork = Ensure.NotNull(artwork);

    /// <summary>Whether the cover is open or opening; false while closed or closing.</summary>
    public bool IsOpen { get; internal set; }

    /// <summary>Whether an opening or closing is still under way.</summary>
    public bool IsTransitioning { get; internal set; }

    /// <summary>The cover picture shown, or null while the cover is closed.</summary>
    public SamusArmCannonCoverFrame? Frame { get; internal set; }

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
    public void Update(SamusState samus)
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

        if (IsTransitioning)
            AdvanceFrame();
        else
            TryStartTransition(samus.SelectedHudItem);

        DrawOrder = Placement(samus.Pose).DrawOrder;
    }

    /// <summary>
    /// Draws the direction-specific small sprite showing its selected cover frame. Samus's drawing places it
    /// before or after her body by <see cref="DrawOrder"/>.
    /// </summary>
    public void Draw(
        SpriteFrameBuffer sprites,
        SamusState samus,
        Position16<RoomPixels> cameraPosition,
        int frameCounter)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(samus);

        // The cannon shares Samus's invincibility flicker, and a closed frame performs
        // neither sprite nor artwork work. The game returns before even consulting pose data.
        if (Frame is not { } cover || (!samus.InvincibilityTimer.IsZero && SamusCadenceDefinitions.InvincibilityFlickerHidden.IsSetIn(frameCounter)))
            return;

        SamusArmCannonPlacement placement = Placement(samus.Pose);
        SamusAnimationFrame frame = samus.AnimationFrameOf(samus.Pose);
        SamusProjectileDirection direction = placement.Direction(frame);

        Offset16<RoomPixels> offset = placement.Offset(frame);
        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);

        // The cover is published wherever it lies, off the screen included; the display clips it (#1587).
        Offset16<ScreenPixels> place =
            (samus.Position.Whole + offset - Offset16<RoomPixels>.Of(0, graphicsYOffset)).RelativeToCamera(cameraPosition);
        // The cover's direction and picture name its sheet frame; a closed cover returned above.
        InstalledSpriteSheets sheets = samus.Presentation.Sheets;
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusArmCannon),
            sheets.Get(SpriteSheetId.SamusOverlays).Frame(SamusOverlaySheetDefinitions.ArmCannonFrame(direction, cover)),
            place, SpriteMirror.None));
    }

    /// <summary>
    /// Starts opening or closing only after the HUD item has been stable for two updates. The starting update already
    /// shows the transition's first picture: slightly open for an opening, fully open for a closing.
    /// </summary>
    private void TryStartTransition(HudItem selectedHudItem)
    {
        if (StableSelectionUpdates < 2)
            return;

        bool opens = SamusArmCannonOpening.Opens(selectedHudItem);
        if (IsOpen == opens)
            return;

        Frame = opens ? SamusArmCannonCoverFrame.SlightlyOpen : SamusArmCannonCoverFrame.FullyOpen;
        IsOpen = opens;
        IsTransitioning = true;
    }

    /// <summary>Where the installed artwork places the cover for <paramref name="pose"/>.</summary>
    private SamusArmCannonPlacement Placement(SamusPoseId pose) =>
        _artwork.ArmCannon.Placement(pose);

    /// <summary>
    /// Moves the cover one picture toward fully open or closed; reaching either ends the transition while keeping the
    /// open state.
    /// </summary>
    private void AdvanceFrame()
    {
        if (IsOpen)
        {
            Frame = Frame switch
            {
                null => SamusArmCannonCoverFrame.SlightlyOpen,
                SamusArmCannonCoverFrame.SlightlyOpen => SamusArmCannonCoverFrame.HalfOpen,
                SamusArmCannonCoverFrame.HalfOpen or SamusArmCannonCoverFrame.FullyOpen => SamusArmCannonCoverFrame.FullyOpen,
                _ => throw new InvalidOperationException($"Undefined arm cannon cover frame {Frame}."),
            };
            IsTransitioning = Frame != SamusArmCannonCoverFrame.FullyOpen;
        }
        else
        {
            Frame = Frame switch
            {
                SamusArmCannonCoverFrame.FullyOpen => SamusArmCannonCoverFrame.HalfOpen,
                SamusArmCannonCoverFrame.HalfOpen => SamusArmCannonCoverFrame.SlightlyOpen,
                SamusArmCannonCoverFrame.SlightlyOpen or null => null,
                _ => throw new InvalidOperationException($"Undefined arm cannon cover frame {Frame}."),
            };
            IsTransitioning = Frame is not null;
        }
    }

}
