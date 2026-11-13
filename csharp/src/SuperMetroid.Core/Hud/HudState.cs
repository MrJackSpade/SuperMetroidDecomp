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
    /// <summary>The values the HUD shows, as its last update read them.</summary>
    private HudSnapshot values;

    /// <summary>
    /// What each of the minimap's cells shows as the minimap's last update read it, row by row; empty before the minimap has
    /// been updated since the HUD was set up, while the template's starter picture shows.
    /// </summary>
    internal ImmutableArray<MinimapCell> Minimap { get; set; } = [];

    /// <summary>Previous item selection used to detect a change of selection and its sound.</summary>
    internal HudItem PreviousSelectedItem { get; set; }
    /// <summary>The installed HUD layout the values are placed by.</summary>
    internal GameplayHudPresentation? Presentation { get; private set; }

    /// <summary>Suppresses minimap updates after a boss initializer.</summary>
    public bool MinimapDisabled { get; internal set; }

    /// <summary>Hides the minimap's fifteen cells and disables updates.</summary>
    internal void DisableMinimapForBoss()
    {
        MinimapDisabled = true;
        Minimap = [.. Enumerable.Repeat<MinimapCell>(MinimapHiddenCell.Instance, MinimapCellCount)];
    }

    /// <summary>Restores minimap updates when leaving the room.</summary>
    internal void EnableMinimapAfterDoorEntry() => MinimapDisabled = false;

    /// <summary>Whether the HUD has been set up from an inventory.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Absolute area-map tile (X 0-63, Y 0-31) selected by the latest minimap update.</summary>
    public Position16<MapTiles> MinimapCenter { get; private set; }

    /// <summary>The minimap's cell count: its columns times its rows.</summary>
    private static int MinimapCellCount => GameplayHudPresentation.MinimapSize.X * GameplayHudPresentation.MinimapSize.Y;

    /// <summary>Binds current host-owned HUD artwork; the HUD's images are chosen from its values whenever it is shown.</summary>
    public void BindPresentation(GameplayHudPresentation? value) => Presentation = value;

    /// <summary>Sets the HUD up showing <paramref name="snapshot"/>'s values, the minimap showing the template's starter picture.</summary>
    public void Initialize(HudSnapshot snapshot)
    {
        _ = Presentation ?? throw new InvalidOperationException("HUD initialization requires installed presentation assets.");
        values = snapshot;
        Minimap = [];
        // HUD setup initializes the previous selected item to zero. The HUD update
        // performs the first live comparison on the next gameplay pass.
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
        bool timeIsFrozen = false)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(audio);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating gameplay counters.");

        values = HudSnapshot.Of(samus);

        // Selecting an item publishes the item-select sound. This belongs in the live HUD update rather than the input
        // handler: the game suppresses the sound (but not the visual change) while spinning, wall-jumping, grappling, or
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
    /// Updates the visible 5x3 minimap for a supplied room/map position: explores the cell Samus is in (and the corner
    /// above a sloped hallway) and reads the cells around it, including the blinking center cell.
    /// </summary>
    public void UpdateMinimap(SharedGameState system, MinimapFrame frame)
    {
        Ensure.NotNull(system);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating its minimap.");
        if (frame.PresentationMap is null)
            throw new InvalidOperationException("Minimap updates require an installed area-map definition.");
        if (frame.PresentationMap.Area != frame.AreaIndex)
            throw new ArgumentException("Map presentation belongs to a different area.", nameof(frame.PresentationMap));
        if (frame.RoomSize.X == 0 || frame.RoomSize.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(frame.RoomSize));
        // The room definition holds its map coordinates below 256.
        if (frame.RoomMap.X > byte.MaxValue || frame.RoomMap.Y > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(frame.RoomMap));

        // The minimap update returns rather than indexing unrelated map memory if Samus is outside
        // the active room. The 16-pixel comparison consumes the same positions as collision.
        if (MinimapDisabled || !frame.RoomSize.Contains(frame.Samus.ToRoomBlock()))
            return;

        // One area-map tile represents one 256x256 room screen. The Y formula has
        // an intentional +1 because the HUD map convention places the room definition's Y one
        // row above the screen containing Samus.
        int centerX = (frame.RoomMap.X + RoomGridConversions.PixelToScreen(frame.Samus.X)) & 63;
        int centerY = frame.RoomMap.Y + RoomGridConversions.PixelToScreen(frame.Samus.Y) + 1;
        if ((uint)centerY >= 32)
            return;
        MinimapCenter = new(CoordinateAxis.Wrap(centerX), CoordinateAxis.Wrap(centerY));

        // Exploration is persistent game state, not HUD-local rendering state. Earlier
        // builds kept this bit in HudState, which made the minimap look correct until a
        // save/load or HUD reconstruction silently erased the visited route.
        system.MarkExploredMapTile(frame.AreaIndex, new Position16<MapTiles>(CoordinateAxis.Wrap(centerX), CoordinateAxis.Wrap(centerY)));
        bool hasAreaMap = system.HasAreaMap(frame.AreaIndex);
        int columns = GameplayHudPresentation.MinimapSize.X;
        var cells = new MinimapCell[MinimapCellCount];

        for (int outputY = 0; outputY < GameplayHudPresentation.MinimapSize.Y; outputY++)
        {
            int mapY = centerY + outputY - 1;
            for (int outputX = 0; outputX < columns; outputX++)
            {
                int destination = outputY * columns + outputX;
                int mapX = (centerX + outputX - 2) & 63;
                if ((uint)mapY >= 32)
                {
                    cells[destination] = MinimapHiddenCell.Instance;
                    continue;
                }

                var cell = new Position16<MapTiles>(CoordinateAxis.Wrap(mapX), CoordinateAxis.Wrap(mapY));
                bool explored = system.IsMapTileExplored(frame.AreaIndex, cell);
                if (!AreaMapVisibility.IsVisible(
                        explored,
                        hasAreaMap,
                        frame.PresentationMap.IsRevealedByMapStation(cell),
                        frame.PresentationMap.IsDiscoverable(cell),
                        frame.MapRevealMode))
                {
                    cells[destination] = MinimapHiddenCell.Instance;
                    continue;
                }

                cells[destination] = new MinimapMapCell(frame.PresentationMap.Cell(cell),
                    explored ? MinimapShade.Explored : MinimapShade.Unexplored);
                // The center slope also explores the corner above it. The top
                // row has already been read, so that cell acquires the explored
                // shade on the next minimap update.
                if (outputX == 2 && outputY == 1 && explored && centerY > 0 &&
                    frame.PresentationMap.RevealsCellAbove(cell))
                    system.MarkExploredMapTile(frame.AreaIndex, new Position16<MapTiles>(CoordinateAxis.Wrap(centerX), CoordinateAxis.Wrap(centerY - 1)));
            }
        }

        // Every eight frames the minimap turns the center cell to the Samus-location
        // shade. The other half-cycle leaves its explored shade intact, producing the
        // familiar blinking Samus location without a separate sprite. The center is always
        // the explored cell Samus stands in, so it is always a revealed map cell.
        int center = columns + 2;
        if (!HudCadenceDefinitions.SamusSquareShowsExploredShade.IsSetIn(frame.FrameCounter) && cells[center] is MinimapMapCell located)
            cells[center] = located with { Shade = MinimapShade.SamusLocation };
        Minimap = [.. cells];
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
        for (int order = 0; order < GameplayHudDefinitions.ItemCount; order++)
        {
            HudIcon item = GameplayHudDefinitions.Icons[order];
            bool held = Holds(item);
            // The selected item's icon shows the selected shade, every other held item's the shade the selection leaves.
            HudIconShade? shade = values.SelectedItem == GameplayHudDefinitions.IconItems[order] ? HudIconShade.Selected
                : held ? HudIconShade.Deselected : null;
            if (art.Icon(item, held, shade) is { } icon)
                images.Add(icon);
        }
        int fullTanks = values.Health / 100;
        int tankCount = Math.Min(values.MaxHealth / 100, GameplayHudDefinitions.EnergyTankCount);
        for (int tank = 0; tank < tankCount; tank++)
            images.Add(art.EnergyTank(tank, tank < fullTanks ? HudEnergyTank.Full : HudEnergyTank.Empty));
        images.AddRange(art.EnergyCount(values.Health % 100));
        if (values.MaxMissiles != 0)
            images.AddRange(art.Ammo(HudAmmoCounter.Missiles, values.Missiles));
        if (values.MaxSuperMissiles != 0)
            images.AddRange(art.Ammo(HudAmmoCounter.SuperMissiles, values.SuperMissiles));
        if (values.MaxPowerBombs != 0)
            images.AddRange(art.Ammo(HudAmmoCounter.PowerBombs, values.PowerBombs));
        // AUTO shows its arrows filled or empty; the pause screen's switch to manual (the only way manual is reached)
        // clears its cells; before the first reserve tank the template shows.
        HudReserveIndicator? reserve = values.ReserveMode switch
        {
            ReserveTankMode.Auto => values.ReserveHealth != 0 ? HudReserveIndicator.Filled : HudReserveIndicator.Empty,
            ReserveTankMode.Manual => HudReserveIndicator.Cleared,
            ReserveTankMode.None => null,
            _ => throw new InvalidOperationException($"Undefined {nameof(ReserveTankMode)} {values.ReserveMode}."),
        };
        if (reserve is { } indicator)
            images.AddRange(art.AutoReserve(indicator));
        int columns = GameplayHudPresentation.MinimapSize.X;
        for (int index = 0; index < Minimap.Length; index++)
            images.Add(art.MinimapCell(new(index % columns, index / columns), Minimap[index]));
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
