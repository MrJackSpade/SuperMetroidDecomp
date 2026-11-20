using System.Collections.Immutable;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Super Metroid's gameplay HUD (#1466, #1555): the values it shows and the map cells its minimap shows, each as of the
/// HUD's last update, and the images those values select (<see cref="Shown"/>), drawn afresh from them every time.
/// </summary>
/// <remarks>
/// The HUD shows Samus's values as the HUD's own update last read them, not as they stand at every moment: gameplay
/// updates the HUD once per gameplay pass, after the pass's damage and pickups, and the pause screen once per paused
/// update; a pickup's message box holds the HUD until the box closes, so the new count appears only then. The minimap
/// likewise shows the cells its own update last read, so a cell Samus's update explores after the minimap has read it
/// (the corner above a sloped hallway) takes its explored shade on the next update. Nothing else of what the HUD drew
/// before is kept: every image is chosen from these values whenever the HUD is shown.
/// </remarks>
public sealed class HudState
{
    /// <summary>The HUD's icons in the order the HUD lists them (#2567).</summary>
    private static readonly IReadOnlyList<HudIcon> IconOrder = Ensure.EveryMemberOnce<HudIcon>(
    [
        HudIcon.Missile,
        HudIcon.SuperMissile,
        HudIcon.PowerBomb,
        HudIcon.Grapple,
        HudIcon.XRay,
    ]);

    /// <summary>The values the HUD shows, as its last update read them.</summary>
    private HudSnapshot values;

    /// <summary>
    /// What the minimap shows around Samus's cell as its last update read it; none before the minimap has been updated
    /// since the HUD was set up, while the template's starter picture shows.
    /// </summary>
    private MinimapGrid? Minimap { get; set; }

    /// <summary>Previous item selection used to detect a change of selection and its sound.</summary>
    private HudItem PreviousSelectedItem { get; set; }
    /// <summary>The installed HUD layout the values are placed by.</summary>
    private GameplayHudPresentation? Presentation { get; set; }

    /// <summary>Suppresses minimap updates once a boss has set up its fight.</summary>
    private bool MinimapDisabled { get; set; }

    /// <summary>Hides the minimap's cells and disables updates.</summary>
    internal void DisableMinimapForBoss()
    {
        MinimapDisabled = true;
        Minimap = MinimapGrid.Hidden;
    }

    /// <summary>Restores minimap updates when leaving the room.</summary>
    internal void EnableMinimapAfterDoorEntry() => MinimapDisabled = false;

    /// <summary>Whether the HUD has been set up from an inventory.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Binds current host-owned HUD artwork; the HUD's images are chosen from its values whenever it is shown.</summary>
    public void BindPresentation(GameplayHudPresentation value) => Presentation = Ensure.NotNull(value);

    /// <summary>Sets the HUD up showing <paramref name="snapshot"/>'s values, the minimap showing the template's starter picture.</summary>
    public void Initialize(HudSnapshot snapshot)
    {
        _ = Presentation ?? throw new InvalidOperationException("HUD initialization requires installed presentation assets.");
        values = snapshot;
        Minimap = null;
        // Setting the HUD up starts the previous selection at the beam; the HUD's update
        // makes its first comparison with Samus's selection on the next gameplay pass.
        PreviousSelectedItem = HudItem.Beam;
        IsInitialized = true;
    }

    /// <summary>
    /// The HUD's update: reads Samus's values for the HUD to show and, when the selected item has changed, queues the
    /// item-selection sound into <paramref name="audio"/> at once, as the HUD does. Gameplay runs it every gameplay
    /// pass, so enemy damage that changes Samus's state during the pass reaches the next accepted frame.
    /// </summary>
    /// <param name="samus">Samus, whose values the HUD shows.</param>
    /// <param name="audio">The update's audio requests the selection sound joins.</param>
    /// <param name="timeIsFrozen">Whether time is frozen, which silences the selection.</param>
    internal void UpdateGameplayCounters(
        SamusState samus,
        GameplayAudioRequests audio,
        bool timeIsFrozen)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(audio);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating gameplay counters.");

        values = HudSnapshot.Of(samus);

        // Selecting an item publishes the item-select sound. This belongs in the live HUD update rather than the input
        // phase: the game suppresses the sound (but not the visual change) while spinning, wall-jumping, grappling, or
        // time is frozen.
        if (samus.SelectedHudItem != PreviousSelectedItem)
        {
            PreviousSelectedItem = samus.SelectedHudItem;

            SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
            if (movementType is not (SamusMovementType.SpinJumping or SamusMovementType.WallJumping) &&
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                !timeIsFrozen)
                audio.QueueSound(SamusSound.HudWeaponSelect, SoundRequestPriority.Normal);
        }
    }

    /// <summary>
    /// Updates the visible minimap for a supplied room/map position: explores the cell Samus is in (and the corner
    /// above a sloped hallway) and reads the cells around it, including the blinking center cell.
    /// </summary>
    public void UpdateMinimap(SharedGameState system, MinimapFrame frame)
    {
        Ensure.NotNull(system);
        Ensure.NotNull(frame);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating its minimap.");
        if (Ensure.NotNull(frame.PresentationMap).Area != frame.Area)
            throw new ArgumentException("Map presentation belongs to a different area.", nameof(frame.PresentationMap));
        if (frame.RoomSize.X == 0 || frame.RoomSize.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(frame.RoomSize));

        // The minimap does nothing while the block Samus stands in lies outside the room, the same blocks collision
        // places her in.
        if (MinimapDisabled || !frame.RoomSize.Contains(frame.Samus.ToRoomBlock()))
            return;

        // One area-map cell shows one room screen: Samus's cell is the room's cell moved by the screen she is in.
        // A room running past the map's right edge wraps to its left edge (AreaMapSize.WrapColumn).
        int centerX = AreaMapSize.WrapColumn(frame.RoomMap.X + RoomGridConversions.PixelToScreen(frame.Samus.X));
        int centerY = frame.RoomMap.Y + RoomGridConversions.PixelToScreen(frame.Samus.Y);
        if (!AreaMapSize.ContainsRow(centerY))
            return;

        // Exploration is persistent game state, not the HUD's own drawing state, so saving,
        // loading and rebuilding the HUD keep the visited route.
        system.MarkExploredMapTile(frame.Area, new Position16<MapTiles>(centerX, centerY));
        bool hasAreaMap = system.HasAreaMap(frame.Area);
        MinimapGrid minimap = MinimapGrid.Read(place =>
        {
            int mapY = centerY + place.Y;
            if (!AreaMapSize.ContainsRow(mapY))
                return MinimapHiddenCell.Instance;
            var cell = new Position16<MapTiles>(AreaMapSize.WrapColumn(centerX + place.X), mapY);
            bool explored = system.IsMapTileExplored(frame.Area, cell);
            if (!AreaMapVisibility.IsVisible(
                    explored,
                    hasAreaMap,
                    frame.PresentationMap.IsRevealedByMapStation(cell),
                    frame.PresentationMap.IsDiscoverable(cell),
                    frame.MapRevealMode))
                return MinimapHiddenCell.Instance;
            // The center slope also explores the corner above it. The top row has already been read, so that cell
            // acquires the explored shade on the next minimap update.
            if (place == MinimapGrid.SamusCell && explored && centerY > 0 && frame.PresentationMap.RevealsCellAbove(cell))
                system.MarkExploredMapTile(frame.Area, new Position16<MapTiles>(centerX, centerY - 1));
            return new MinimapMapCell(frame.PresentationMap.Cell(cell), explored ? MinimapShade.Explored : MinimapShade.Unexplored);
        });

        // At its blink cadence the minimap turns the center cell to the Samus-location
        // shade. The other half-cycle leaves its explored shade intact, producing the
        // familiar blinking Samus location without a separate sprite. The center is always
        // the explored cell Samus stands in, so it is always a revealed map cell.
        if (!HudCadenceDefinitions.SamusSquareShowsExploredShade.IsSetIn(frame.FrameCounter) && minimap[MinimapGrid.SamusCell] is MinimapMapCell located)
            minimap = minimap.WithSamusCell(located with { Shade = MinimapShade.SamusLocation });
        Minimap = minimap;
    }

    /// <summary>
    /// The HUD's images as its values select them now, in the order they are drawn (#1466, #1555): the static top row, then
    /// the template with each value's images over it. A HUD with no artwork bound shows nothing, and one not yet set up
    /// shows only its top row.
    /// </summary>
    public ShownHud Shown()
    {
        if (Presentation is not { } art)
            return ShownHud.Nothing;
        var images = ImmutableArray.CreateBuilder<PlacedHudImage>();
        images.Add(GameplayHudPresentation.TopRow);
        if (!IsInitialized)
            return new ShownHud(images.ToImmutable());
        images.Add(GameplayHudPresentation.Template);
        foreach (HudIcon item in IconOrder)
        {
            bool held = Holds(item);
            // The selected item's icon shows the selected shade, every other held item's the shade the selection leaves.
            HudIconShade? shade = values.SelectedItem == HudIconDefinitions.Items[item] ? HudIconShade.Selected
                : held ? HudIconShade.Deselected : null;
            if (art.Icon(item, held, shade) is { } icon)
                images.Add(icon);
        }
        int fullTanks = values.Health / SamusEnergyDefinitions.EnergyPerTank;
        int tankCount = Math.Min(values.MaxHealth / SamusEnergyDefinitions.EnergyPerTank, GameplayHudDefinitions.EnergyTankCount);
        for (int tank = 0; tank < tankCount; tank++)
            images.Add(art.EnergyTank(tank, tank < fullTanks ? HudEnergyTank.Full : HudEnergyTank.Empty));
        images.AddRange(art.EnergyCount(values.Health % SamusEnergyDefinitions.EnergyPerTank));
        // Each ammunition counter shows its digits once Samus can carry any of it.
        foreach ((HudAmmoCounter counter, int count, int maximum) in (ReadOnlySpan<(HudAmmoCounter, int, int)>)[
            (HudAmmoCounter.Missiles, values.Missiles, values.MaxMissiles),
            (HudAmmoCounter.SuperMissiles, values.SuperMissiles, values.MaxSuperMissiles),
            (HudAmmoCounter.PowerBombs, values.PowerBombs, values.MaxPowerBombs)])
        {
            if (maximum != 0)
                images.AddRange(art.Ammo(counter, count));
        }
        // AUTO shows its arrows filled or empty; the pause screen's switch to manual (the only way manual is reached)
        // clears its place; before the first reserve tank the template shows.
        HudReserveIndicator? reserve = values.ReserveMode switch
        {
            ReserveTankMode.Auto => values.ReserveHealth != 0 ? HudReserveIndicator.Filled : HudReserveIndicator.Empty,
            ReserveTankMode.Manual => HudReserveIndicator.Cleared,
            ReserveTankMode.None => null,
            _ => throw new InvalidOperationException($"Undefined {nameof(ReserveTankMode)} {values.ReserveMode}."),
        };
        if (reserve is { } indicator)
            images.Add(art.AutoReserve(indicator));
        if (Minimap is { } shown)
        {
            foreach (Offset16<MapTiles> place in MinimapGrid.Places)
                images.Add(art.MinimapCell(MinimapGrid.Square(place), shown[place]));
        }
        return new ShownHud(images.ToImmutable());
    }

    /// <summary>Whether Samus holds the item <paramref name="icon"/> shows.</summary>
    private bool Holds(HudIcon icon) => icon switch
    {
        HudIcon.Missile => values.MaxMissiles != 0,
        HudIcon.SuperMissile => values.MaxSuperMissiles != 0,
        HudIcon.PowerBomb => values.MaxPowerBombs != 0,
        HudIcon.Grapple => values.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam),
        HudIcon.XRay => values.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope),
        _ => throw new ArgumentOutOfRangeException(nameof(icon), icon, "Undefined item icon."),
    };
}
