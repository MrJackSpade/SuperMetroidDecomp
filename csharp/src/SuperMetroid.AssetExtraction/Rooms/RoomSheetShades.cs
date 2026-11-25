using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The shades the import pre-colours a tileset's room pictures in (#1512): every colouring a colour effect of its rooms
/// shows besides the tileset's standard colours, which gameplay names by the effect's typed state.
/// </summary>
internal static partial class RoomSheetShades
{
    /// <summary>
    /// The shades of <paramref name="tileset"/>'s room pictures, its blocks, backgrounds, written pictures and floor: the
    /// records its rooms' palette effects show, and the colours of the bosses its rooms hold that recolour the room.
    /// </summary>
    internal static IReadOnlyList<SheetShade> Room(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        SelectedRoom[] rooms = [.. RoomsOf(tileset)];
        var shades = new List<SheetShade>(PaletteFxRoomShades.Of(rooms, inputs.Maps.RoomPaletteFx));

        // What a door's fade leaves of the room, which the Godot game's assets are written from.
        shades.AddRange(DoorFadeRoomShades.Of());
        foreach ((EnemyDefinitionId[] enemies, Func<RoomBlockSheetInputs, IEnumerable<SheetShade>> roomShades) in EnemyRoomShades)
        {
            if (enemies.Any(enemy => Places(rooms, enemy)))
                shades.AddRange(roomShades(inputs));
        }
        return shades;
    }

    /// <summary>The shades a room of the listed enemies' rooms adds, from the room's sheet inputs.</summary>
    /// <param name="Enemies">The enemies whose rooms add the shades.</param>
    /// <param name="Shades">The shades the rooms add.</param>
    private sealed partial record EnemyRoomShade(EnemyDefinitionId[] Enemies, Func<RoomBlockSheetInputs, IEnumerable<SheetShade>> Shades);

    /// <summary>
    /// The enemies whose colours recolour the rooms that place them, each with the shades they bring, in the order a
    /// tileset's shades list them: Kraid's backdrop, Spore Spawn's room, the Wrecked Ship's power-on Phantoon brings, the
    /// Ceres elevator room's glow, the Tourian statue's looks, Mother Brain's room and the Ridleys' rooms.
    /// </summary>
    private static readonly EnemyRoomShade[] EnemyRoomShades =
    [
        new([EnemyDefinitionId.Kraid], inputs => KraidBackdropShades.Of(inputs.Bosses.KraidColors
            ?? throw new InvalidDataException("Kraid's room is shaded from the installed Kraid colours."))),
        new([EnemyDefinitionId.SporeSpawn], inputs => SporeSpawnSheetShades.Room(inputs.Bosses)),
        new([EnemyDefinitionId.PhantoonBody], inputs => PhantoonSheetShades.PowerOn(inputs.Bosses)),
        new([EnemyDefinitionId.CeresElevatorDoor], ElevatorRoomGlow),
        new([EnemyDefinitionId.TourianStatue], TourianStatueLooks),
        new([EnemyDefinitionId.MotherBrainBody], inputs => MotherBrainSheetShades.Room(inputs.Maps)),
        new([EnemyDefinitionId.RidleyCeres, EnemyDefinitionId.Ridley], inputs => RidleySheetShades.Room(inputs.Maps, inputs.Bosses)),
    ];

    /// <summary>
    /// The shades of the pictures <paramref name="writers"/> write over a room (#1544): each writer's own effects alone,
    /// the self-destruct warning's alarm; no colour effect of the room recolours them.
    /// </summary>
    /// <exception cref="InvalidDataException">A writer is undefined, or the Ceres colours are not installed.</exception>
    internal static IReadOnlyList<SheetShade> Written(RoomBlockSheetInputs inputs, IEnumerable<TileWordWriter> writers)
    {
        Ensure.NotNull(inputs);
        return [.. Ensure.NotNull(writers).OrderBy(WrittenPictureSheetComposer.WriterOrder.PlaceOf).SelectMany(writer => WrittenBy(inputs, writer))];
    }

    /// <summary>The shades of the pictures <paramref name="writer"/> writes.</summary>
    /// <exception cref="InvalidDataException">The writer is undefined, or the Ceres colours are not installed.</exception>
    private static IReadOnlyList<SheetShade> WrittenBy(RoomBlockSheetInputs inputs, TileWordWriter writer) => writer switch
    {
        TileWordWriter.ZebesEscapeText => [],
        TileWordWriter.CeresEscapeText => RidleySheetShades.CeresWarning(
            inputs.Maps.CeresRidleyColors),
        _ => throw new InvalidDataException($"Undefined tile word writer {writer}."),
    };

    /// <summary>Whether any of <paramref name="rooms"/> places <paramref name="enemy"/>.</summary>
    private static bool Places(IEnumerable<SelectedRoom> rooms, EnemyDefinitionId enemy) =>
        rooms.Any(room => room.State.EnemyPopulation.Placements.Span.Any(record => record.DefinitionId == enemy));

    /// <summary>
    /// The Ceres elevator room's glow, which its rotating-room door cycles through the first tile group's colours nine to
    /// fourteen, each step from the installed door colours.
    /// </summary>
    /// <exception cref="InvalidDataException">The door colours are not installed.</exception>
    private static IEnumerable<SheetShade> ElevatorRoomGlow(RoomBlockSheetInputs inputs)
    {
        CeresDoorVisuals door = inputs.Objects.Areas.CeresDoorVisual
            ?? throw new InvalidDataException("The Ceres elevator room's glow is shaded from the installed Ceres door colours.");
        const int firstInk = 2 * 16 + 9;
        for (int step = 0; step < CeresDoorShades.ElevatorRoomGlowSteps; step++)
        {
            int shown = step;
            yield return SheetShade.Run(CeresDoorShades.ElevatorRoomGlow(shown), firstInk,
                [.. Enumerable.Range(0, CeresDoorVisualDefinitions.AnimationColorCount).Select(colour => door.AnimationColor(shown, colour))]);
        }
    }

    /// <summary>
    /// The looks of the Tourian statues drawn in the room's tiles: Kraid's in the sixth tile group and Draygon's in the
    /// fifth, each with its three eye colours blacked out as its release starts, then greyed.
    /// </summary>
    /// <exception cref="InvalidDataException">The statue colours are not installed.</exception>
    private static IEnumerable<SheetShade> TourianStatueLooks(RoomBlockSheetInputs inputs)
    {
        TourianStatuePalettes statue = inputs.Objects.Areas.TourianStatueColors
            ?? throw new InvalidDataException("The Tourian statues' looks are shaded from the installed statue colours.");
        RgbColor[] grey = [.. Enumerable.Range(0, TourianStatueColorDefinitions.GreyColorCount).Select(statue.GreyColor)];
        const int kraid = 7 * 16, draygon = 6 * 16;
        yield return SheetShade.Run(TourianStatueShades.KraidCleared, kraid + 12, new RgbColor[3]);
        yield return SheetShade.Run(TourianStatueShades.KraidGrey, kraid, grey);
        yield return SheetShade.Run(TourianStatueShades.DraygonCleared, draygon + 9, new RgbColor[3]);
        yield return SheetShade.Run(TourianStatueShades.DraygonGrey, draygon, grey);
    }

    /// <summary>
    /// The shades of <paramref name="tileset"/>'s room effects' pictures on the effect overlay: the room effects' palette
    /// blends and, where its rooms hold Phantoon, the Wrecked Ship's power coming on.
    /// </summary>
    internal static IReadOnlyList<SheetShade> RoomEffects(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        return [.. RoomFxBlendSheetShades.Of(inputs.Maps.RoomFxPaletteBlends),
            .. Places(RoomsOf(tileset), EnemyDefinitionId.PhantoonBody) ? PhantoonSheetShades.PowerOn(inputs.Bosses) : []];
    }

    /// <summary>
    /// The shades of a tileset's pictures of the HUD (#1731, #2289): a door's dark shades of the HUD and the room effects'
    /// palette blends, which shade the HUD as they shade the effects.
    /// </summary>
    internal static IReadOnlyList<SheetShade> Hud(RoomBlockSheetInputs inputs) =>
        [.. DoorFadeHudShades.Of(), .. RoomFxBlendSheetShades.Of(Ensure.NotNull(inputs).Maps.RoomFxPaletteBlends)];

    /// <summary><paramref name="sheet"/>, also pre-coloured in <paramref name="shades"/>.</summary>
    internal static ComposedWrittenPictureSheet With(ComposedWrittenPictureSheet sheet, IReadOnlyList<SheetShade> shades) =>
        Ensure.NotNull(sheet) with { Sheet = ShadedSheets.With(sheet.Sheet, shades) };

    /// <summary><paramref name="background"/>, its image also pre-coloured in <paramref name="shades"/>.</summary>
    internal static ComposedRoomBackground With(ComposedRoomBackground background, IReadOnlyList<SheetShade> shades) =>
        Ensure.NotNull(background) with { Sheet = ShadedSheets.With(background.Sheet, shades) };

    /// <summary>Every retail room state that loads <paramref name="tileset"/>, with its room.</summary>
    private static IEnumerable<SelectedRoom> RoomsOf(TilesetDefinition tileset) =>
        RetailRooms.All.SelectMany(room => RoomStateLists.GetStates(room)
            .Where(state => RoomTilesets.Get(state.GraphicsSet) == tileset)
            .Select(state => new SelectedRoom(room, state)));
}
