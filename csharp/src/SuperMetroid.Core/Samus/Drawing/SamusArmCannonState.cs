using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Samus's opening arm cannon: its cover's state and the one picture it draws.
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
    internal HudItem PreviousSelectedHudItem { get; private set; }

    /// <summary>The installed artwork the cover is placed by: installed content, not game state.</summary>
    private readonly SamusArtworkBinding _artwork;

    /// <summary>Starts a closed cover placed by <paramref name="artwork"/>'s arm-cannon placements.</summary>
    internal SamusArmCannonState(SamusArtworkBinding artwork) => _artwork = Ensure.NotNull(artwork);

    /// <summary>The cover: closed, opening, open or closing, with the picture it shows (#2560).</summary>
    private SamusArmCannonCover Cover { get; set; } = SamusArmCannonCover.Closed;

    /// <summary>
    /// The HUD selection's stability wait, lasting <see cref="SamusArmCannonDefinitions.StableSelectionUpdates"/>: a
    /// change starts it again, counting the changing update as its first, and a transition may begin once it is over.
    /// </summary>
    private readonly UpdateCountdown stability = UpdateCountdown.Lasting(SamusArmCannonDefinitions.StableSelectionUpdates);

    /// <summary>Where this frame's cover is drawn relative to the body, from the pose's drawing mode.</summary>
    public SamusArmCannonDrawOrder DrawOrder { get; private set; }

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
            stability.Restart(SamusArmCannonDefinitions.StableSelectionUpdates);
            stability.Count();
            PreviousSelectedHudItem = samus.SelectedHudItem;
        }
        else
        {
            stability.Step();
        }

        if (Cover is SamusArmCannonCover.Opening or SamusArmCannonCover.Closing)
            Cover = Cover.Advanced();
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

        // The cannon shares Samus's invincibility flicker; a closed or flickered-out cover draws nothing.
        if (Cover.Shown is not { } cover || (!samus.InvincibilityTimer.IsZero && SamusCadenceDefinitions.InvincibilityFlickerHidden.IsInSecondHalfAt(frameCounter)))
            return;

        SamusArmCannonPlacement placement = Placement(samus.Pose);
        SamusAnimationFrame frame = samus.AnimationFrameOf(samus.Pose);
        SamusProjectileDirection direction = placement.Direction(samus.Pose, frame);

        Offset16<RoomPixels> offset = placement.Offset(frame);
        int artOrigin = SamusPoseMetadata.ReadArtOrigin(samus);

        // The cover is published wherever it lies, off the screen included; the display clips it (#1587).
        Offset16<ScreenPixels> place =
            (samus.Position.Whole + offset - Offset16<RoomPixels>.Of(0, artOrigin)).RelativeToCamera(cameraPosition);
        // The cover's direction and picture name its sheet frame; a closed cover returned above.
        InstalledSpriteSheets sheets = samus.Presentation.Sheets;
        sprites.Add(new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusArmCannon),
            sheets.Get(SpriteSheetId.SamusOverlays).Frame(SamusOverlaySheetDefinitions.ArmCannonFrame(direction, cover)),
            place));
    }

    /// <summary>
    /// Starts opening or closing only after the HUD item has been stable for <see cref="SamusArmCannonDefinitions.StableSelectionUpdates"/>. The starting update already
    /// shows the transition's first picture: slightly open for an opening, fully open for a closing.
    /// </summary>
    private void TryStartTransition(HudItem selectedHudItem)
    {
        if (stability.IsRunning)
            return;

        bool opens = SamusArmCannonOpening.Opens(selectedHudItem);
        if (Cover.OpenOrOpening == opens)
            return;

        Cover = opens
            ? new SamusArmCannonCover.Opening(SamusArmCannonCoverFrame.SlightlyOpen)
            : new SamusArmCannonCover.Closing(SamusArmCannonCoverFrame.FullyOpen);
    }

    /// <summary>Where the installed artwork places the cover for <paramref name="pose"/>.</summary>
    private SamusArmCannonPlacement Placement(SamusPoseId pose) =>
        _artwork.ArmCannon.Placement(pose);

}
