using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// What Super Metroid's gameplay HUD shows, as values (#1466): each energy tank, the energy count, each ammunition
/// count, the AUTO reserve indicator, which item icons are shown and the shade the selection last gave each, and the
/// five-by-three minimap's cells. Each value keeps what the HUD last drew, as the cartridge's HUD keeps its last
/// writes; a value nothing has drawn since the HUD was set up shows the installed template. The installed artwork
/// draws them (<see cref="Rows"/>).
/// </summary>
public sealed class HudState
{
    /// <summary>Number of eight-pixel cells per HUD row.</summary>
    public const int WidthInTiles = GameplayHudDefinitions.Width;
    /// <summary>Number of HUD rows the HUD's values are drawn over, excluding the preceding static row.</summary>
    public const int MutableRowCount = GameplayHudDefinitions.Height;
    /// <summary>Ninety-six cells in the three rows the HUD's values are drawn over.</summary>
    public const int MutableTileCount = WidthInTiles * MutableRowCount;

    /// <summary>Each energy tank as last drawn, or null where the template shows.</summary>
    private readonly HudEnergyTank?[] energyTanks = new HudEnergyTank?[GameplayHudDefinitions.EnergyTankCount];

    /// <summary>The energy count as last drawn, or null before it has been.</summary>
    private int? energyCount;

    /// <summary>Each ammunition count as last drawn (missiles, Super Missiles, Power Bombs), or null before it has been.</summary>
    private readonly int?[] ammoCounts = new int?[GameplayHudDefinitions.AmmoCounterCount];

    /// <summary>The AUTO reserve indicator as last drawn, or null where the template shows.</summary>
    private HudReserveIndicator? reserveIndicator;

    /// <summary>Whether each item icon has been added since the HUD was set up.</summary>
    private readonly bool[] iconsAdded = new bool[GameplayHudDefinitions.ItemCount];

    /// <summary>The shade the selection last gave each icon's cells since its icon was drawn, or null.</summary>
    private readonly HudIconShade?[] iconShades = new HudIconShade?[GameplayHudDefinitions.ItemCount];

    /// <summary>Each minimap cell as last drawn, row by row, or null where the template's starter picture shows.</summary>
    internal readonly MinimapCell?[] Minimap = new MinimapCell?[GameplayHudPresentation.MinimapSize.X * GameplayHudPresentation.MinimapSize.Y];

    /// <summary>Previous item selection used to update both icon shades and sound edges.</summary>
    internal HudItem PreviousSelectedItem { get; set; }
    /// <summary>Current artwork binding, omitted from serialized gameplay state.</summary>
    [NonSerialized] private GameplayHudPresentation? presentation;

    /// <summary>Native $05F7: suppress minimap updates after a boss initializer.</summary>
    public bool MinimapDisabled { get; internal set; }

    /// <summary>$90:A7E2 blanks the fifteen minimap cells and disables updates.</summary>
    internal void DisableMinimapForBoss()
    {
        MinimapDisabled = true;
        Minimap.AsSpan().Fill(MinimapHiddenCell.Instance);
    }

    /// <summary>$82:E1B7 restores minimap updates when leaving the room.</summary>
    internal void EnableMinimapAfterDoorEntry() => MinimapDisabled = false;

    /// <summary>Whether the HUD has been set up from an inventory.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// One-frame publication of <c>QueueSfx1_Max6($39)</c> from <c>$80:9B44</c>.
    /// The frontend consumes it after the runtime frame has completed.
    /// </summary>
    public bool SelectionSoundRequestedThisFrame { get; private set; }
    /// <summary>Transfers the pending HUD sound once, including across frontend frame boundaries.</summary>
    public bool ConsumeSelectionSoundRequest()
    {
        bool requested = SelectionSoundRequestedThisFrame;
        SelectionSoundRequestedThisFrame = false;
        return requested;
    }

    /// <summary>
    /// Native queue suppression captured when the HUD requests its sound, rather than
    /// when the frontend eventually publishes it. Later explosion state changes must
    /// not retroactively admit or reject an earlier request.
    /// </summary>
    public bool SelectionSoundSuppressedThisFrame { get; private set; }

    /// <summary>Absolute area-map tile (X 0-63, Y 0-31) selected by the latest minimap update.</summary>
    public Position16<MapTiles> MinimapCenter { get; private set; }

    /// <summary>
    /// Binds current host-owned HUD artwork. Rebinding changed content redraws the HUD's values from
    /// <paramref name="samus"/> over the new template, keeping the minimap's cells.
    /// </summary>
    public void BindPresentation(GameplayHudPresentation? value, SamusState? samus = null)
    {
        GameplayHudPresentation? previous = presentation;
        presentation = value;
        if (!IsInitialized || value is null || samus is null ||
            string.Equals(previous?.ContentIdentity, value.ContentIdentity, StringComparison.Ordinal))
            return;
        ShowTemplateExceptMinimap();
        ApplyCurrentPresentationState(samus);
    }

    /// <summary>
    /// Ports the visible data work of <c>$80:9A79</c> and its first
    /// <c>$80:9B44</c> update for an explicit inventory snapshot.
    /// </summary>
    public void Initialize(HudSnapshot snapshot)
    {
        _ = presentation ?? throw new InvalidOperationException("HUD initialization requires installed presentation assets.");
        ShowTemplateExceptMinimap();
        Array.Clear(Minimap);

        if (snapshot.EquippedItems.HasAny(SamusEquipmentFlags.XrayScope))
            AddIcon(itemIndex: 4);
        if (snapshot.EquippedItems.HasAny(SamusEquipmentFlags.GrappleBeam))
            AddIcon(itemIndex: 3);
        if (snapshot.MaxMissiles != 0)
            AddIcon(itemIndex: 0);
        if (snapshot.MaxSuperMissiles != 0)
            AddIcon(itemIndex: 1);
        if (snapshot.MaxPowerBombs != 0)
            AddIcon(itemIndex: 2);

        if (snapshot.ReserveMode == ReserveTankMode.Auto)
            DrawAutoReserve(snapshot.ReserveHealth != 0);

        DrawHealth(snapshot.Health, snapshot.MaxHealth);
        if (snapshot.MaxMissiles != 0)
            ammoCounts[0] = snapshot.Missiles;
        if (snapshot.MaxSuperMissiles != 0)
            ammoCounts[1] = snapshot.SuperMissiles;
        if (snapshot.MaxPowerBombs != 0)
            ammoCounts[2] = snapshot.PowerBombs;

        ToggleItemHighlight(snapshot.SelectedItem, HudIconShade.Selected);
        // `$80:9AC9` initializes samus_prev_hud_item_index to zero. HandleHudTilemap
        // performs the first live comparison on the next gameplay pass.
        PreviousSelectedItem = HudItem.Beam;
        SelectionSoundRequestedThisFrame = false;
        IsInitialized = true;
    }

    /// <summary>
    /// Replays the mutable counter portion of <c>$80:9B44</c> from live Samus state. The
    /// counters are redrawn every gameplay frame and the next accepted frame shows them;
    /// initialization is not the sole owner of the energy digits. Updating them on every
    /// gameplay pass prevents enemy damage from changing physics state while the visible
    /// HUD remains frozen at 99.
    /// </summary>
    [IndependentFlag("Time freeze and sound suppression are separate runtime states supplied by the frame.")]
    public void UpdateGameplayCounters(
        SamusState samus,
        bool timeIsFrozen = false,
        bool soundSuppressed = false)
    {
        Ensure.NotNull(samus);
        if (!IsInitialized)
            throw new InvalidOperationException("Initialize the HUD before updating gameplay counters.");

        SelectionSoundRequestedThisFrame = false;

        SelectionSoundSuppressedThisFrame = soundSuppressed;

        // Permanent pickups can introduce an inventory family after HUD initialization.
        // The cartridge's item routines add those icons immediately; replay the same
        // guarded additions here before updating their counters. Each addition is
        // idempotent and preserves an already-shown icon and the live minimap.
        if ((samus.EquippedItems & SamusEquipmentFlags.XrayScope) != SamusEquipmentFlags.None)
            AddIcon(itemIndex: 4);
        if ((samus.EquippedItems & SamusEquipmentFlags.GrappleBeam) != SamusEquipmentFlags.None)
            AddIcon(itemIndex: 3);
        if (samus.MaxMissiles != 0)
            AddIcon(itemIndex: 0);
        if (samus.MaxSuperMissiles != 0)
            AddIcon(itemIndex: 1);
        if (samus.MaxPowerBombs != 0)
            AddIcon(itemIndex: 2);

        DrawHealth(samus.Health, samus.MaxHealth);
        if (samus.MaxMissiles != 0)
            ammoCounts[0] = samus.Missiles;
        if (samus.MaxSuperMissiles != 0)
            ammoCounts[1] = samus.SuperMissiles;
        if (samus.MaxPowerBombs != 0)
            ammoCounts[2] = samus.PowerBombs;
        if (samus.ReserveTankMode == ReserveTankMode.Auto)
            DrawAutoReserve(samus.ReserveEnergy != 0);

        // `$80:9BD3-$9C20` gives the newly selected icon the selected shade, returns the
        // old icon to the deselected shade, then publishes sound $39. This belongs in the
        // live HUD update rather than the input handler: native suppresses the sound (but
        // not the visual change) while spinning, wall-jumping, grappling, or time is frozen.
        if (samus.SelectedHudItem != PreviousSelectedItem)
        {
            ToggleItemHighlight(samus.SelectedHudItem, HudIconShade.Selected);
            ToggleItemHighlight(PreviousSelectedItem, HudIconShade.Deselected);
            PreviousSelectedItem = samus.SelectedHudItem;

            SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
            SelectionSoundRequestedThisFrame =
                movementType is not (SamusMovementType.SpinJumping or SamusMovementType.WallJumping) &&
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                !timeIsFrozen;
        }
    }

    /// <summary>
    /// Ports the visible 5x3 result of <c>UpdateMinimap</c> at <c>$90:A91B</c> for a
    /// supplied room/map position, including exploration bits and the blinking center cell.
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
        // The room header stores its map coordinates as bytes.
        if (frame.RoomMap.X > byte.MaxValue || frame.RoomMap.Y > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(frame.RoomMap));

        // $90:A91B returns rather than indexing unrelated map memory if Samus is outside
        // the active room. The 16-pixel comparison consumes the same positions as collision.
        if (MinimapDisabled || !frame.RoomSize.Contains(frame.Samus.ToRoomBlock()))
            return;

        // One area-map tile represents one 256x256 room screen. The native Y formula has
        // an intentional +1 because the HUD map convention places the room header's Y one
        // row above the screen containing Samus.
        int centerX = (frame.RoomMap.X + RoomGridConversions.PixelToScreen(frame.Samus.X)) & 63;
        int centerY = frame.RoomMap.Y + RoomGridConversions.PixelToScreen(frame.Samus.Y) + 1;
        if ((uint)centerY >= 32)
            return;
        MinimapCenter = new((ushort)centerX, (ushort)centerY);

        // Exploration is persistent game state, not HUD-local rendering state. Earlier
        // builds kept this bit in HudState, which made the minimap look correct until a
        // save/load or HUD reconstruction silently erased the visited route.
        system.MarkExploredMapTile(frame.AreaIndex, new Position16<MapTiles>(unchecked((ushort)centerX), unchecked((ushort)centerY)));
        bool hasAreaMap = system.HasAreaMap(frame.AreaIndex);
        int columns = GameplayHudPresentation.MinimapSize.X;

        for (int outputY = 0; outputY < GameplayHudPresentation.MinimapSize.Y; outputY++)
        {
            int mapY = centerY + outputY - 1;
            for (int outputX = 0; outputX < columns; outputX++)
            {
                int destination = outputY * columns + outputX;
                int mapX = (centerX + outputX - 2) & 63;
                if ((uint)mapY >= 32)
                {
                    Minimap[destination] = MinimapHiddenCell.Instance;
                    continue;
                }

                var cell = new Position16<MapTiles>(unchecked((ushort)mapX), unchecked((ushort)mapY));
                bool explored = system.IsMapTileExplored(frame.AreaIndex, cell);
                if (!AreaMapVisibility.IsVisible(
                        explored,
                        hasAreaMap,
                        frame.PresentationMap.IsRevealedByMapStation(cell),
                        frame.PresentationMap.IsDiscoverable(cell),
                        frame.MapRevealMode))
                {
                    Minimap[destination] = MinimapHiddenCell.Instance;
                    continue;
                }

                MapCell mapCell = frame.PresentationMap.Cell(cell);
                Minimap[destination] = new MinimapMapCell(mapCell.Shape, mapCell.Mirroring,
                    explored ? MinimapShade.Explored : MinimapShade.Unexplored);
                // The center slope also explores the corner above it. The native top
                // row has already been rendered and its explored bits latched, so that
                // cell acquires the explored shade on the next minimap update.
                if (outputX == 2 && outputY == 1 && explored && centerY > 0 &&
                    frame.PresentationMap.RevealsCellAbove(cell))
                    system.MarkExploredMapTile(frame.AreaIndex, new Position16<MapTiles>(unchecked((ushort)centerX), unchecked((ushort)(centerY - 1))));
            }
        }

        // Every eight NMI frames the native code turns the center cell to the Samus-location
        // shade. The other half-cycle leaves its explored shade intact, producing the
        // familiar blinking Samus location without a separate sprite. The center is always
        // the explored cell Samus stands in, so it is always a revealed map cell.
        int center = columns + 2;
        if ((frame.FrameCounter & 8) == 0 && Minimap[center] is MinimapMapCell located)
            Minimap[center] = located with { Shade = MinimapShade.SamusLocation };
    }

    /// <summary>
    /// The HUD's four rows as the installed artwork draws them now, row by row: the static top row, then the template
    /// with every value drawn over it. A HUD with no artwork bound shows nothing, and one not yet set up shows only its
    /// top row.
    /// </summary>
    public BackgroundTile[] Rows()
    {
        var rows = new BackgroundTile[WidthInTiles + MutableTileCount];
        if (presentation is null)
            return rows;
        presentation.TopRow().CopyTo(rows, 0);
        if (!IsInitialized)
            return rows;
        Span<BackgroundTile> drawn = rows.AsSpan(WidthInTiles);
        presentation.ApplyTemplate(drawn);
        for (int item = 0; item < iconsAdded.Length; item++)
        {
            if (iconsAdded[item])
                presentation.TryApplyIcon(drawn, item);
            if (iconShades[item] is { } shade)
                presentation.ToggleItemHighlight(drawn, GameplayHudDefinitions.IconItems[item],
                    shade == HudIconShade.Selected ? presentation.SelectedPalette : presentation.DeselectedPalette);
        }
        for (int tank = 0; tank < energyTanks.Length; tank++)
            if (energyTanks[tank] is { } shown)
                presentation.DrawEnergyTank(drawn, tank, shown);
        if (energyCount is { } energy)
            presentation.DrawEnergyCount(drawn, energy);
        for (int counter = 0; counter < ammoCounts.Length; counter++)
            if (ammoCounts[counter] is { } count)
                presentation.ApplyAmmo(drawn, counter, count);
        switch (reserveIndicator)
        {
            case HudReserveIndicator.Filled:
                presentation.ApplyAutoReserve(drawn, containsEnergy: true);
                break;
            case HudReserveIndicator.Empty:
                presentation.ApplyAutoReserve(drawn, containsEnergy: false);
                break;
            case HudReserveIndicator.Cleared:
                presentation.ClearAutoReserve(drawn);
                break;
            case null:
                break;
            default:
                throw new InvalidOperationException($"Undefined reserve indicator {reserveIndicator}.");
        }
        int columns = GameplayHudPresentation.MinimapSize.X;
        for (int index = 0; index < Minimap.Length; index++)
            if (Minimap[index] is { } cell)
                presentation.DrawMinimapCell(drawn, new(checked((ushort)(index % columns)), checked((ushort)(index / columns))), cell);
        return rows;
    }

    /// <summary>Updates the energy tanks and the two-digit energy remainder.</summary>
    private void DrawHealth(int health, int maxHealth)
    {
        _ = presentation ?? throw new InvalidOperationException("HUD energy requires installed presentation assets.");
        int fullTanks = health / 100;
        int tankCount = Math.Min(maxHealth / 100, GameplayHudDefinitions.EnergyTankCount);
        for (int tank = 0; tank < tankCount; tank++)
            energyTanks[tank] = tank < fullTanks ? HudEnergyTank.Full : HudEnergyTank.Empty;
        energyCount = health % 100;
    }

    /// <summary>Draws the AUTO reserve indicator when its selected tank contains energy.</summary>
    private void DrawAutoReserve(bool containsEnergy)
    {
        _ = presentation ?? throw new InvalidOperationException("HUD reserve indicator requires installed presentation assets.");
        reserveIndicator = containsEnergy ? HudReserveIndicator.Filled : HudReserveIndicator.Empty;
    }

    /// <summary>Ports $82:AF33 without reinitializing icons, counters or the minimap.</summary>
    public void ClearAutoReserveIndicator()
    {
        if (!IsInitialized) throw new InvalidOperationException("Initialize the HUD before clearing AUTO.");
        _ = presentation ?? throw new InvalidOperationException("HUD reserve indicator requires installed presentation assets.");
        reserveIndicator = HudReserveIndicator.Cleared;
    }

    /// <summary>
    /// Adds item <paramref name="itemIndex"/>'s icon if the HUD has not yet. The cartridge draws an icon only over a
    /// blank anchor, so an added icon replaces any shade the selection gave the cells beneath it only when it draws.
    /// </summary>
    private void AddIcon(int itemIndex)
    {
        GameplayHudPresentation art = presentation ?? throw new InvalidOperationException("HUD icons require installed presentation assets.");
        if (iconsAdded[itemIndex])
            return;
        iconsAdded[itemIndex] = true;
        if (art.IconShowsOverTemplate(itemIndex))
            iconShades[itemIndex] = null;
    }

    /// <summary>Gives the selected item's icon cells <paramref name="shade"/>, without redrawing the icon.</summary>
    internal void ToggleItemHighlight(HudItem selectedItem, HudIconShade shade)
    {
        _ = presentation ?? throw new InvalidOperationException("HUD selection requires installed presentation assets.");
        int itemIndex = Array.IndexOf(GameplayHudDefinitions.IconItems, selectedItem);
        if (itemIndex >= 0)
            iconShades[itemIndex] = shade;
    }

    /// <summary>Returns every value but the minimap's to the template, as applying it does.</summary>
    private void ShowTemplateExceptMinimap()
    {
        Array.Clear(energyTanks);
        energyCount = null;
        Array.Clear(ammoCounts);
        reserveIndicator = null;
        Array.Clear(iconsAdded);
        Array.Clear(iconShades);
    }

    /// <summary>Restores visible icons, counters, reserve mode, and selection after artwork rebind.</summary>
    private void ApplyCurrentPresentationState(SamusState samus)
    {
        if ((samus.EquippedItems & SamusEquipmentFlags.XrayScope) != SamusEquipmentFlags.None)
            AddIcon(4);
        if ((samus.EquippedItems & SamusEquipmentFlags.GrappleBeam) != SamusEquipmentFlags.None)
            AddIcon(3);
        if (samus.MaxMissiles != 0) AddIcon(0);
        if (samus.MaxSuperMissiles != 0) AddIcon(1);
        if (samus.MaxPowerBombs != 0) AddIcon(2);
        DrawHealth(samus.Health, samus.MaxHealth);
        if (samus.MaxMissiles != 0) ammoCounts[0] = samus.Missiles;
        if (samus.MaxSuperMissiles != 0) ammoCounts[1] = samus.SuperMissiles;
        if (samus.MaxPowerBombs != 0) ammoCounts[2] = samus.PowerBombs;
        if (samus.ReserveTankMode == ReserveTankMode.Auto) DrawAutoReserve(samus.ReserveEnergy != 0);
        ToggleItemHighlight(samus.SelectedHudItem, HudIconShade.Selected);
    }
}
