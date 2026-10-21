using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The reverse of the import pipeline's draw-address codec, for oracle checks that start from a cartridge address:
/// every compiled PLM draw, and the draw of a family stored at an address. Families can share an address (Kraid's first
/// ceiling crumble is the elevatube block), so a lookup names the family. An address where the family stores no draw
/// yields <see cref="Unowned"/>, a draw no family owns, which every family's descriptors decline just as they declined
/// an address outside their family.
/// </summary>
internal static class NativePlmDraws
{
    /// <summary>Every compiled PLM draw of every family.</summary>
    internal static IEnumerable<PlmDraw> All =>
        Enum.GetValues<BombBlockRestoreDraw>().Select(Of)
            .Concat(Enum.GetValues<BombTorizoHandDraw>().Select(Of))
            .Concat([BotwoonWallPlmDrawDefinitions.Clear])
            .Concat(Enum.GetValues<ChozoStatueDraw>().Select(Of))
            .Concat(Enum.GetValues<ContactCrumbleRestoreDraw>().Select(Of))
            .Concat(Enum.GetValues<CrocomireArenaDraw>().Select(Of))
            .Concat(Enum.GetValues<DownwardGateDraw>().Select(Of))
            .Concat(Enum.GetValues<DraygonCannonDraw>().Select(Of))
            .Concat(Enum.GetValues<ElevatorPlatformDraw>().Select(Of))
            .Concat(Enum.GetValues<EscapeAnimalDraw>().Select(Of))
            .Concat(Enum.GetValues<KraidRoomDraw>().Select(Of))
            .Concat(Enum.GetValues<MotherBrainEscapeGateDraw>().Select(Of))
            .Concat(Enum.GetValues<MotherBrainFakeDeathDraw>().Select(Of))
            .Concat(Enum.GetValues<MotherBrainGlassDraw>().Select(Of))
            .Concat(Enum.GetValues<NoobTubeDraw>().Select(Of))
            .Concat(Enum.GetValues<RoomPlmBombedRevealDraw>().Select(Of))
            .Concat(Enum.GetValues<SamusEaterDraw>().Select(Of))
            .Concat(Enum.GetValues<SporeSpawnCeilingDraw>().Select(Of))
            .Concat(Enum.GetValues<StationDraw>().Select(Of))
            .Concat(Enum.GetValues<TourianAccessDraw>().Select(Of))
            .Concat(Enum.GetValues<GrappleBlockDraw>().Select(Of))
            .Concat(RoomPlmShotBlockDrawDefinitions.Calculated.Select(Of))
            .Concat(RoomPlmCollectibleDrawDefinitions.All.Select(frame => Of(frame.Draw)))
            .Concat(BlueDoorPlmDrawDefinitions.Editable.Select(list => list.Draw))
            .Concat(Enum.GetValues<ColoredDoorOrientation>().Select(BlueDoorPlmDrawDefinitions.Alias))
            .Concat(GreyDoorPlmDrawDefinitions.All.Select(list => list.Draw))
            .Concat(ColoredDoorPlmDrawDefinitions.All.Select(list => list.Draw))
            .Concat(EyeDoorPlmDrawDefinitions.All.Select(list => list.Draw))
            .Concat([CrateriaMainstreetEscapePassagePlmDefinitions.ClearPairDraw, OldTourianEscapeShaftWallPlmDefinitions.OpenWallDraw,
                MaridiaElevatubePlmDefinitions.BlockDraw, SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw]);

    /// <summary>A draw that belongs to no family.</summary>
    internal static PlmDraw Unowned { get; } = PlmDraw.Of(new UnownedFrame());

    /// <summary>Every compiled draw, grouped by the address the cartridge stores it at.</summary>
    private static readonly ILookup<int, PlmDraw> ByAddress = All.ToLookup(draw => (int)PlmDrawAddressCodec.Of(draw));

    /// <summary>The cartridge address of <paramref name="draw"/>.</summary>
    internal static ushort AddressOf(PlmDraw draw) => PlmDrawAddressCodec.Of(draw);

    /// <summary>Whether any compiled draw is stored at <paramref name="address"/>.</summary>
    internal static bool IsDrawAt(int address) => ByAddress.Contains(address);

    /// <summary>
    /// The draw of <typeparamref name="TFrame"/>'s family stored at <paramref name="address"/>, or <see cref="Unowned"/>
    /// when the family stores none there.
    /// </summary>
    internal static PlmDraw At<TFrame>(int address) where TFrame : struct =>
        ByAddress[address].OfType<PlmFrameDraw<TFrame>>().SingleOrDefault() ?? Unowned;

    /// <summary>Whether <typeparamref name="TFrame"/>'s family stores a draw at <paramref name="address"/>.</summary>
    internal static bool IsDrawAt<TFrame>(int address) where TFrame : struct =>
        ByAddress[address].OfType<PlmFrameDraw<TFrame>>().Any();

    /// <summary>The frame of a draw that belongs to no family.</summary>
    internal readonly record struct UnownedFrame;

    /// <summary>The draw of a frame.</summary>
    private static PlmDraw Of<TFrame>(TFrame frame) where TFrame : struct => PlmDraw.Of(frame);
}
