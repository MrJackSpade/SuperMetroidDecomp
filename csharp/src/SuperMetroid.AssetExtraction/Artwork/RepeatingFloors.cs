using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Which turning floors the cartridge repeats beyond their edges (#1646), decided from the Mode 7 settings each scene that
/// shows a floor runs with (the settings shadow at <c>$7E:005F</c>, written to <c>M7SEL</c> each frame by
/// <c>$80:9225</c>): settings 0 repeat the 1024 by 1024 floor beyond its edges, so every pixel of the floor, its clear
/// margin as well, comes into view with the period of the whole floor, and the floor's picture is the whole floor; settings
/// <c>$80</c> show nothing beyond its edges, so what lies beyond the part that shows anything shows nothing either, and its
/// picture is the box of that part.
/// </summary>
/// <remarks>
/// <para>
/// The scenes, from the disassembly: the title, <c>$80</c> (<c>Setup_PPU_TitleSequence</c>, <c>$8B:8018</c>); the Ceres
/// cinematics' flight, station and explosion shots, <c>$80</c> (<c>Setup_PPU_CeresCutscene</c>, <c>$8B:81B3</c>), and the
/// approach to Zebes, <c>$80</c> (<c>CinematicFunction_FlyToZebes_MosaicTransition</c>, <c>$8B:C7E8</c>), the 0 that
/// <c>$8B:C048</c> writes being shown with background mode 1, which shows no floor; the ending's two escape scenes and the
/// planet's explosion, 0 (<c>Setup_PPU_ZebesDestruction</c>, <c>$8B:82AB</c>, run by the ending's setup at
/// <c>$8B:D48E</c>, and 0 again at <c>$8B:DA66</c> as the explosion's crossfade ends); the gunship flying away,
/// <c>$80</c> (<c>CinematicFunc_Ending_SpaceView_GunshipEmergence_Setup</c>, <c>$8B:DBD1</c>); the reward icon's shot,
/// <c>$80</c> (<c>CinematicFunction_PostCredits_IdleSamus2</c>, <c>$8B:E357</c>), the 0 that <c>$8B:E4B4</c> writes
/// coming with background mode 1 (<c>$8B:E498</c>).
/// </para>
/// <para>
/// In rooms: the Ceres elevator shaft, 0, which the console's start (<c>$80:881E</c>) leaves and the shaft's door
/// (<c>$8F:E4E0</c>) never writes (#1653); Ceres Ridley's getaway, <c>$80</c> (<c>SetupCeresRidleyMode7</c>,
/// <c>$A6:AA64</c>), the 0 that <c>$A6:AB41</c> writes as the getaway ends coming with background mode 1.
/// </para>
/// </remarks>
internal static class RepeatingFloors
{
    /// <summary>The frontend floor sheets whose scenes repeat the floor beyond its edges: the ending's escape and explosion.</summary>
    private static readonly FrozenSet<SpriteSheetId> RepeatedFrontendFloors = new[]
    {
        SpriteSheetId.EndingEscapeAFloor, SpriteSheetId.EndingEscapeBFloor, SpriteSheetId.EndingPlanetExplosionFloor,
    }.ToFrozenSet();

    /// <summary>Whether the scene showing frontend floor sheet <paramref name="floor"/> repeats the floor beyond its edges.</summary>
    internal static bool Repeats(SpriteSheetId floor) => RepeatedFrontendFloors.Contains(floor);

    /// <summary>
    /// Whether some retail room of <paramref name="tileset"/> repeats its turning floor beyond its edges: a room state that
    /// shows the elevator shaft's floor (<see cref="RoomTurningFloors"/>), which the shaft repeats; Ceres Ridley's getaway
    /// shows nothing beyond its floor.
    /// </summary>
    internal static bool Repeats(TilesetDefinition tileset) =>
        RetailRooms.All.SelectMany(RoomStateLists.GetStates)
            .Any(state => RoomTurningFloors.Of(state) == RoomTurningFloor.ElevatorShaft && RoomTilesets.Get(state.GraphicsSet) == tileset);
}
