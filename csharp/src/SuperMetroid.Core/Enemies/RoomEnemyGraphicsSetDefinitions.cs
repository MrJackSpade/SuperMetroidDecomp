namespace SuperMetroid.Core.Enemies;

/// <summary>Compiled graphics-set membership and native staging order for all retail room states.</summary>
/// <remarks>
/// The 302 bank-$B4 lists contain 425 records in the pinned NTSC J/U v1.0
/// cartridge. VRAM/palette destination words are fixed engine staging metadata;
/// editable pixel/color content is loaded separately through enemy artwork assets.
/// </remarks>
public static class RoomEnemyGraphicsSetDefinitions
{
    /// <summary>All retail sets sorted by native pointer for binary lookup.</summary>
    private static readonly RoomEnemyGraphicsSetDefinition[] Definitions =
    [
        ..BuildSegment0(),
        ..BuildSegment1(),
    ];

    /// <summary>Number of distinct retail bank-$B4 graphics-set identities in the compiled catalog, including empty sets and separately addressed sets with matching content.</summary>
    public const int ListCount = 302;
    /// <summary>Total four-byte member records across all compiled retail lists, excluding terminators and trailing metadata; not a count of unique enemy definitions or artwork images.</summary>
    public const int RecordCount = 425;

    /// <summary>Rejects a compiled catalog with missing, duplicate or misordered retail records.</summary>
    static RoomEnemyGraphicsSetDefinitions()
    {
        if (Definitions.Length != ListCount || Definitions.Sum(list => list.Records.Length) != RecordCount)
            throw new InvalidDataException("Compiled enemy graphics-set counts changed.");
        for (int index = 1; index < Definitions.Length; index++)
            if (Definitions[index - 1].Pointer >= Definitions[index].Pointer)
                throw new InvalidDataException("Compiled enemy graphics sets are not strictly ordered.");
    }

    /// <summary>Gets a retail graphics set by its native bank-$B4 identity.</summary>
    /// <param name="pointer">Native 16-bit list-start word, without the implied $B4 bank; not an index into the catalog.</param>
    /// <returns>The shared compiled definition whose record order controls tile staging and palette installation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pointer is not one of the compiled retail list identities.</exception>
    public static RoomEnemyGraphicsSetDefinition Get(ushort pointer)
    {
        int low = 0;
        int high = Definitions.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            RoomEnemyGraphicsSetDefinition candidate = Definitions[middle];
            if (candidate.Pointer == pointer) return candidate;
            if (candidate.Pointer < pointer) low = middle + 1;
            else high = middle - 1;
        }
        throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
            "Pointer is not a retail bank-$B4 enemy graphics set.");
    }

    /// <summary>Enumerates every compiled set identity for ROM-oracle tests.</summary>
    public static IEnumerable<ushort> Pointers => Definitions.Select(list => list.Pointer);

    /// <summary>First pointer-ordered portion of the compiled bank-$B4 graphics-set catalog.</summary>
    private static RoomEnemyGraphicsSetDefinition[] BuildSegment0() =>
    [
        new(0x8000, [
        ]),
        new(0x8009, [
            new(EnemyDefinitionId.Choot, 0x0001),
            new(EnemyDefinitionId.Skultera, 0x0002),
            new(EnemyDefinitionId.Kamer2, 0x0003),
        ]),
        new(0x801f, [
            new(EnemyDefinitionId.Alcoon, 0x0001),
            new(EnemyDefinitionId.Zero, 0x0001),
        ]),
        new(0x8031, [
            new(EnemyDefinitionId.Waver, 0x0001),
            new(EnemyDefinitionId.Zoomer, 0x0002),
        ]),
        new(0x8043, [
            new(EnemyDefinitionId.FaceBlock, 0x0007),
            new(EnemyDefinitionId.Stoke, 0x0007),
        ]),
        new(0x8055, [
            new(EnemyDefinitionId.FaceBlock, 0x0007),
            new(EnemyDefinitionId.Stoke, 0x0007),
        ]),
        new(0x8067, [
            new(EnemyDefinitionId.Zoomer, 0x0001),
            new(EnemyDefinitionId.Skree, 0x0002),
            new(EnemyDefinitionId.Ripper, 0x0003),
        ]),
        new(0x807d, [
            new(EnemyDefinitionId.Waver, 0x0001),
            new(EnemyDefinitionId.Mellow, 0x0002),
        ]),
        new(0x808f, [
            new(EnemyDefinitionId.PirateGreyWalking, 0x0001),
            new(EnemyDefinitionId.PirateGreyWall, 0x0002),
        ]),
        new(0x80a1, [
            new(EnemyDefinitionId.YappingMaw, 0x0001),
            new(EnemyDefinitionId.Waver, 0x0002),
        ]),
        new(0x80b3, [
            new(EnemyDefinitionId.BombTorizo, 0x0001),
        ]),
        new(0x80c1, [
            new(EnemyDefinitionId.PirateGreenWalking, 0x0001),
            new(EnemyDefinitionId.Beetom, 0x0002),
        ]),
        new(0x80d3, [
        ]),
        new(0x80dd, [
            new(EnemyDefinitionId.Eye, 0x0001),
        ]),
        new(0x80eb, [
            new(EnemyDefinitionId.BombTorizo, 0x0001),
        ]),
        new(0x8107, [
        ]),
        new(0x8111, [
            new(EnemyDefinitionId.PirateSilverNinja, 0x0002),
        ]),
        new(0x8129, [
        ]),
        new(0x8133, [
            new(EnemyDefinitionId.FaceBlock, 0x0001),
        ]),
        new(0x8141, [
            new(EnemyDefinitionId.FaceBlock, 0x0001),
        ]),
        new(0x814f, [
            new(EnemyDefinitionId.Sbug, 0x0007),
            new(EnemyDefinitionId.FaceBlock, 0x0007),
        ]),
        new(0x8161, [
            new(EnemyDefinitionId.Skultera, 0x0001),
            new(EnemyDefinitionId.Zeb, 0x0002),
            new(EnemyDefinitionId.GRipper, 0x0003),
            new(EnemyDefinitionId.Tripper, 0x0007),
        ]),
        new(0x8185, [
            new(EnemyDefinitionId.Sbug, 0x0007),
        ]),
        new(0x8193, [
            new(EnemyDefinitionId.ShipTop, 0x0002),
            new(EnemyDefinitionId.ShipBottomEntrance, 0x0007),
        ]),
        new(0x81a5, [
            new(EnemyDefinitionId.Boyon, 0x0001),
            new(EnemyDefinitionId.YappingMaw, 0x0002),
        ]),
        new(0x81c1, [
            new(EnemyDefinitionId.HZoomer, 0x0001),
            new(EnemyDefinitionId.Sova, 0x0001),
        ]),
        new(0x81d3, [
            new(EnemyDefinitionId.PirateGreyWall, 0x0001),
        ]),
        new(0x81e1, [
            new(EnemyDefinitionId.Waver, 0x0001),
            new(EnemyDefinitionId.Choot, 0x0007),
        ]),
        new(0x81f3, [
        ]),
        new(0x81fd, [
            new(EnemyDefinitionId.YappingMaw, 0x0001),
            new(EnemyDefinitionId.Zebbo, 0x0002),
        ]),
        new(0x820f, [
            new(EnemyDefinitionId.Sbug, 0x0007),
            new(EnemyDefinitionId.FaceBlock, 0x0007),
        ]),
        new(0x8221, [
            new(EnemyDefinitionId.Sciser, 0x0007),
            new(EnemyDefinitionId.Puyo, 0x0007),
        ]),
        new(0x8233, [
        ]),
        new(0x823d, [
            new(EnemyDefinitionId.Kago, 0x0001),
        ]),
        new(0x824b, [
        ]),
        new(0x8255, [
        ]),
        new(0x825f, [
            new(EnemyDefinitionId.Mellow, 0x0001),
            new(EnemyDefinitionId.Rio, 0x0002),
        ]),
        new(0x8271, [
            new(EnemyDefinitionId.Kago, 0x0002),
            new(EnemyDefinitionId.MZoomer, 0x0003),
        ]),
        new(0x8283, [
            new(EnemyDefinitionId.ShipTop, 0x0002),
            new(EnemyDefinitionId.ShipBottomEntrance, 0x0007),
        ]),
        new(0x8295, [
            new(EnemyDefinitionId.Sbug, 0x0007),
        ]),
        new(0x82a3, [
            new(EnemyDefinitionId.EtecoonEscape, 0x0001),
            new(EnemyDefinitionId.DachoraEscape, 0x0007),
        ]),
        new(0x82b5, [
        ]),
        new(0x82bf, [
            new(EnemyDefinitionId.KihunterGreen, 0x0001),
            new(EnemyDefinitionId.Sciser, 0x0002),
            new(EnemyDefinitionId.Zero, 0x0002),
        ]),
        new(0x82d5, [
            new(EnemyDefinitionId.Sidehopper, 0x0001),
            new(EnemyDefinitionId.Zeela, 0x0002),
        ]),
        new(0x82e7, [
        ]),
        new(0x82f1, [
        ]),
        new(0x82fb, [
            new(EnemyDefinitionId.KihunterGreen, 0x0007),
        ]),
        new(0x8309, [
        ]),
        new(0x8313, [
            new(EnemyDefinitionId.FaceBlock, 0x0001),
        ]),
        new(0x8321, [
            new(EnemyDefinitionId.Zeb, 0x0001),
            new(EnemyDefinitionId.Zeela, 0x0002),
        ]),
        new(0x8333, [
            new(EnemyDefinitionId.TourianStatue, 0x0001),
            new(EnemyDefinitionId.TourianStatueGhost, 0xd003),
        ]),
        new(0x8345, [
        ]),
        new(0x834f, [
            new(EnemyDefinitionId.YappingMaw, 0x0001),
            new(EnemyDefinitionId.SidehopperLarge, 0x0002),
        ]),
        new(0x8361, [
            new(EnemyDefinitionId.Skultera, 0x0001),
            new(EnemyDefinitionId.Puyo, 0x0002),
            new(EnemyDefinitionId.Zero, 0x0003),
        ]),
        new(0x8377, [
            new(EnemyDefinitionId.Zoomer, 0x0001),
            new(EnemyDefinitionId.FaceBlock, 0x0002),
        ]),
        new(0x8389, [
            new(EnemyDefinitionId.FaceBlock, 0x0001),
            new(EnemyDefinitionId.Zoomer, 0x0002),
            new(EnemyDefinitionId.Skree, 0x0003),
            new(EnemyDefinitionId.Rio, 0x0007),
        ]),
        new(0x83a3, [
            new(EnemyDefinitionId.Cacatac, 0x0001),
            new(EnemyDefinitionId.Zeela, 0x0002),
        ]),
        new(0x83b5, [
            new(EnemyDefinitionId.SidehopperLarge, 0x0002),
            new(EnemyDefinitionId.FaceBlock, 0x0007),
        ]),
        new(0x83c7, [
        ]),
        new(0x83d1, [
            new(EnemyDefinitionId.Eye, 0x0007),
            new(EnemyDefinitionId.FaceBlock, 0x0007),
        ]),
        new(0x83e3, [
            new(EnemyDefinitionId.Zeela, 0x0001),
        ]),
        new(0x83f1, [
            new(EnemyDefinitionId.Ripper, 0x0001),
            new(EnemyDefinitionId.Beetom, 0x0002),
            new(EnemyDefinitionId.Geega, 0x0003),
        ]),
        new(0x8407, [
            new(EnemyDefinitionId.Boulder, 0x0001),
        ]),
        new(0x8415, [
        ]),
        new(0x841f, [
        ]),
        new(0x8429, [
            new(EnemyDefinitionId.Rio, 0x0001),
            new(EnemyDefinitionId.Sidehopper, 0x0002),
            new(EnemyDefinitionId.Zeb, 0x0003),
        ]),
        new(0x843f, [
        ]),
        new(0x8449, [
            new(EnemyDefinitionId.YappingMaw, 0x0001),
            new(EnemyDefinitionId.Cacatac, 0x0002),
        ]),
        new(0x845b, [
        ]),
        new(0x8465, [
        ]),
        new(0x846f, [
            new(EnemyDefinitionId.ShutterShootable, 0x0001),
            new(EnemyDefinitionId.Metaree, 0x0001),
            new(EnemyDefinitionId.SidehopperLarge, 0x0002),
        ]),
        new(0x8485, [
        ]),
        new(0x848f, [
        ]),
        new(0x8499, [
        ]),
        new(0x84a3, [
        ]),
        new(0x84b7, [
            new(EnemyDefinitionId.Eye, 0x0007),
            new(EnemyDefinitionId.FaceBlock, 0x0007),
        ]),
        new(0x84c9, [
            new(EnemyDefinitionId.Fireflea, 0x0002),
        ]),
        new(0x84d7, [
            new(EnemyDefinitionId.Beetom, 0x0001),
        ]),
        new(0x84e5, [
            new(EnemyDefinitionId.Zebbo, 0x0001),
            new(EnemyDefinitionId.Beetom, 0x0002),
        ]),
        new(0x84f7, [
            new(EnemyDefinitionId.SidehopperLarge, 0x0001),
            new(EnemyDefinitionId.Sidehopper, 0x0002),
            new(EnemyDefinitionId.ShutterShootable, 0x0003),
        ]),
        new(0x850d, [
            new(EnemyDefinitionId.Reflec, 0x0001),
            new(EnemyDefinitionId.Beetom, 0x0002),
        ]),
        new(0x8529, [
        ]),
        new(0x8533, [
            new(EnemyDefinitionId.KihunterGreen, 0x0001),
        ]),
        new(0x8541, [
            new(EnemyDefinitionId.Etecoon, 0x0001),
            new(EnemyDefinitionId.Zeela, 0x0002),
            new(EnemyDefinitionId.Ripper2, 0x0003),
        ]),
        new(0x8557, [
            new(EnemyDefinitionId.Zeb, 0x0001),
        ]),
        new(0x8565, [
            new(EnemyDefinitionId.Sidehopper, 0x0001),
            new(EnemyDefinitionId.ShutterGrowing, 0x0002),
            new(EnemyDefinitionId.Waver, 0x0003),
            new(EnemyDefinitionId.Zeb, 0x0007),
        ]),
        new(0x857f, [
            new(EnemyDefinitionId.Fireflea, 0x0001),
            new(EnemyDefinitionId.Waver, 0x0002),
            new(EnemyDefinitionId.YappingMaw, 0x0003),
        ]),
        new(0x8595, [
            new(EnemyDefinitionId.Sbug, 0x0007),
            new(EnemyDefinitionId.FaceBlock, 0x0007),
        ]),
        new(0x85a7, [
            new(EnemyDefinitionId.Sidehopper, 0x0001),
            new(EnemyDefinitionId.Geega, 0x0002),
        ]),
        new(0x85b9, [
            new(EnemyDefinitionId.Zeela, 0x0001),
            new(EnemyDefinitionId.Metaree, 0x0002),
            new(EnemyDefinitionId.Rio, 0x0003),
            new(EnemyDefinitionId.Dachora, 0x0007),
        ]),
        new(0x85d3, [
            new(EnemyDefinitionId.Zeela, 0x0001),
            new(EnemyDefinitionId.Zebbo, 0x0002),
        ]),
        new(0x85e5, [
        ]),
        new(0x85ef, [
            new(EnemyDefinitionId.Kraid, 0x0007),
        ]),
        new(0x85fd, [
        ]),
        new(0x8607, [
            new(EnemyDefinitionId.ShutterShootable, 0x0001),
        ]),
        new(0x8615, [
        ]),
        new(0x861f, [
            new(EnemyDefinitionId.Boyon, 0x0001),
        ]),
        new(0x862d, [
            new(EnemyDefinitionId.Zeb, 0x0001),
            new(EnemyDefinitionId.Sbug, 0x0002),
        ]),
        new(0x863f, [
            new(EnemyDefinitionId.Zero, 0x0002),
            new(EnemyDefinitionId.Cacatac, 0x0003),
        ]),
        new(0x8651, [
            new(EnemyDefinitionId.PirateGreenWalking, 0x0001),
            new(EnemyDefinitionId.MiniKraid, 0x0003),
        ]),
        new(0x8663, [
            new(EnemyDefinitionId.SporeSpawn, 0x0001),
        ]),
        new(0x8671, [
            new(EnemyDefinitionId.Skree, 0x0001),
            new(EnemyDefinitionId.Boyon, 0x0007),
        ]),
        new(0x8683, [
            new(EnemyDefinitionId.Gamet, 0x0001),
            new(EnemyDefinitionId.Puyo, 0x0002),
        ]),
        new(0x8695, [
            new(EnemyDefinitionId.Viola, 0x0001),
        ]),
        new(0x86a3, [
            new(EnemyDefinitionId.PirateGoldNinja, 0x0001),
        ]),
        new(0x86b1, [
            new(EnemyDefinitionId.Chozo, 0x0001),
            new(EnemyDefinitionId.Magdollite, 0x0003),
            new(EnemyDefinitionId.Holtz, 0x0007),
        ]),
        new(0x86c7, [
            new(EnemyDefinitionId.Gamet, 0x0001),
        ]),
        new(0x86d5, [
            new(EnemyDefinitionId.ShutterGrowing, 0x0001),
            new(EnemyDefinitionId.Dessgeega, 0x0002),
            new(EnemyDefinitionId.Mella, 0x0003),
            new(EnemyDefinitionId.Sova, 0x0007),
        ]),
        new(0x86ef, [
            new(EnemyDefinitionId.YappingMaw, 0x0001),
        ]),
        new(0x86fd, [
            new(EnemyDefinitionId.KihunterRed, 0x0001),
            new(EnemyDefinitionId.Reflec, 0x0002),
        ]),
        new(0x870f, [
            new(EnemyDefinitionId.Ripper2, 0x0001),
        ]),
        new(0x871d, [
        ]),
        new(0x8727, [
            new(EnemyDefinitionId.PirateGoldWall, 0x0001),
            new(EnemyDefinitionId.PirateGoldWalking, 0x0002),
            new(EnemyDefinitionId.Viola, 0x0003),
        ]),
        new(0x873d, [
        ]),
        new(0x8747, [
        ]),
        new(0x8751, [
        ]),
        new(0x875b, [
        ]),
        new(0x8765, [
            new(EnemyDefinitionId.DessgeegaLarge, 0x0001),
            new(EnemyDefinitionId.Multiviola, 0x0002),
        ]),
        new(0x8777, [
        ]),
        new(0x8781, [
            new(EnemyDefinitionId.Ridley, 0x0001),
            new(EnemyDefinitionId.RidleyExplosion, 0xe001),
        ]),
        new(0x8793, [
        ]),
        new(0x879d, [
        ]),
        new(0x87a7, [
        ]),
        new(0x87b1, [
        ]),
        new(0x87bb, [
            new(EnemyDefinitionId.Beetom, 0x0001),
        ]),
        new(0x87c9, [
            new(EnemyDefinitionId.Squeept, 0x0001),
            new(EnemyDefinitionId.Sova, 0x0002),
            new(EnemyDefinitionId.Dragon, 0x0003),
        ]),
        new(0x87df, [
            new(EnemyDefinitionId.Gamet, 0x0001),
            new(EnemyDefinitionId.GRipper, 0x0002),
            new(EnemyDefinitionId.Kamer2, 0x0003),
        ]),
        new(0x87f5, [
            new(EnemyDefinitionId.Namihe, 0x0001),
            new(EnemyDefinitionId.PirateGoldWall, 0x0002),
            new(EnemyDefinitionId.PirateGoldWalking, 0x0003),
        ]),
        new(0x880b, [
            new(EnemyDefinitionId.Fune, 0x0001),
            new(EnemyDefinitionId.Kamer, 0x0002),
            new(EnemyDefinitionId.Kago, 0x0003),
            new(EnemyDefinitionId.GRipper, 0x0007),
        ]),
        new(0x8825, [
            new(EnemyDefinitionId.Multiviola, 0x0001),
        ]),
        new(0x8833, [
            new(EnemyDefinitionId.PirateRedWalking, 0x0001),
            new(EnemyDefinitionId.MZoomer, 0x0003),
        ]),
        new(0x8845, [
            new(EnemyDefinitionId.PirateRedWalking, 0x0001),
            new(EnemyDefinitionId.Multiviola, 0x0002),
            new(EnemyDefinitionId.Cacatac, 0x0003),
            new(EnemyDefinitionId.Sova, 0x0003),
        ]),
        new(0x885f, [
            new(EnemyDefinitionId.Zebbo, 0x0001),
            new(EnemyDefinitionId.Holtz, 0x0007),
        ]),
        new(0x8871, [
            new(EnemyDefinitionId.Fireflea, 0x0001),
            new(EnemyDefinitionId.Boulder, 0x0002),
            new(EnemyDefinitionId.Fune, 0x0003),
        ]),
        new(0x8887, [
            new(EnemyDefinitionId.Multiviola, 0x0001),
            new(EnemyDefinitionId.Magdollite, 0x0007),
        ]),
        new(0x8899, [
        ]),
        new(0x88a3, [
            new(EnemyDefinitionId.Namihe, 0x0001),
        ]),
        new(0x88b1, [
            new(EnemyDefinitionId.Puromi, 0x0007),
        ]),
        new(0x88bf, [
            new(EnemyDefinitionId.Dragon, 0x0001),
            new(EnemyDefinitionId.Geruta, 0x0002),
        ]),
        new(0x88d1, [
            new(EnemyDefinitionId.Alcoon, 0x0001),
        ]),
        new(0x88df, [
        ]),
        new(0x88e9, [
        ]),
        new(0x88f3, [
        ]),
        new(0x88fd, [
        ]),
        new(0x8907, [
            new(EnemyDefinitionId.ShutterShootable, 0x0007),
            new(EnemyDefinitionId.Sova, 0x0002),
            new(EnemyDefinitionId.Geruta, 0x0003),
        ]),
        new(0x891d, [
            new(EnemyDefinitionId.PirateGoldWalking, 0x0007),
        ]),
        new(0x892b, [
            new(EnemyDefinitionId.Sova, 0x0001),
            new(EnemyDefinitionId.Dessgeega, 0x0002),
        ]),
        new(0x893d, [
            new(EnemyDefinitionId.Hibashi, 0x0001),
            new(EnemyDefinitionId.Dragon, 0x0007),
            new(EnemyDefinitionId.Gamet, 0x0007),
        ]),
        new(0x8953, [
            new(EnemyDefinitionId.Mella, 0x0001),
            new(EnemyDefinitionId.GRipper, 0x0002),
            new(EnemyDefinitionId.Kamer2, 0x0003),
        ]),
        new(0x8969, [
        ]),
        new(0x8973, [
        ]),
    ];

    /// <summary>Remaining pointer-ordered portion of the compiled bank-$B4 graphics-set catalog.</summary>
    private static RoomEnemyGraphicsSetDefinition[] BuildSegment1() =>
    [
        new(0x897d, [
            new(EnemyDefinitionId.Sova, 0x0001),
        ]),
        new(0x898b, [
            new(EnemyDefinitionId.Hibashi, 0x0001),
            new(EnemyDefinitionId.DessgeegaLarge, 0x0002),
            new(EnemyDefinitionId.ShutterShootable, 0x0003),
            new(EnemyDefinitionId.KzanTop, 0x0007),
        ]),
        new(0x89a5, [
            new(EnemyDefinitionId.Dragon, 0x0001),
            new(EnemyDefinitionId.Gamet, 0x0002),
        ]),
        new(0x89b7, [
        ]),
        new(0x89c1, [
        ]),
        new(0x89cb, [
            new(EnemyDefinitionId.Sova, 0x0001),
        ]),
        new(0x89d9, [
            new(EnemyDefinitionId.Waver, 0x0001),
            new(EnemyDefinitionId.Ripper2, 0x0001),
            new(EnemyDefinitionId.Cacatac, 0x0002),
            new(EnemyDefinitionId.Sova, 0x0002),
        ]),
        new(0x89f3, [
            new(EnemyDefinitionId.Tripper, 0x0007),
        ]),
        new(0x8a01, [
            new(EnemyDefinitionId.Boyon, 0x0001),
            new(EnemyDefinitionId.Ripper2, 0x0002),
        ]),
        new(0x8a13, [
            new(EnemyDefinitionId.Metaree, 0x0001),
            new(EnemyDefinitionId.Gamet, 0x0002),
        ]),
        new(0x8a25, [
            new(EnemyDefinitionId.Sova, 0x0001),
            new(EnemyDefinitionId.Geruta, 0x0002),
        ]),
        new(0x8a37, [
            new(EnemyDefinitionId.Dragon, 0x0001),
            new(EnemyDefinitionId.Sova, 0x0002),
        ]),
        new(0x8a49, [
            new(EnemyDefinitionId.Fune, 0x0001),
            new(EnemyDefinitionId.Multiviola, 0x0002),
        ]),
        new(0x8a5b, [
            new(EnemyDefinitionId.Sova, 0x0001),
            new(EnemyDefinitionId.Fune, 0x0002),
        ]),
        new(0x8a6d, [
            new(EnemyDefinitionId.GoldenTorizo, 0x0001),
        ]),
        new(0x8a7b, [
            new(EnemyDefinitionId.Tripper, 0x0007),
            new(EnemyDefinitionId.ShutterShootable, 0x0002),
        ]),
        new(0x8a8d, [
        ]),
        new(0x8a97, [
            new(EnemyDefinitionId.Viola, 0x0001),
            new(EnemyDefinitionId.Zebbo, 0x0007),
        ]),
        new(0x8aa9, [
            new(EnemyDefinitionId.Multiviola, 0x0001),
            new(EnemyDefinitionId.Alcoon, 0x0002),
        ]),
        new(0x8abb, [
            new(EnemyDefinitionId.Geruta, 0x0001),
            new(EnemyDefinitionId.Metaree, 0x0002),
        ]),
        new(0x8acd, [
            new(EnemyDefinitionId.Gamet, 0x0001),
            new(EnemyDefinitionId.Skree, 0x0002),
        ]),
        new(0x8adf, [
            new(EnemyDefinitionId.Viola, 0x0001),
        ]),
        new(0x8aed, [
            new(EnemyDefinitionId.Sova, 0x0002),
        ]),
        new(0x8afb, [
            new(EnemyDefinitionId.Hibashi, 0x0001),
            new(EnemyDefinitionId.KihunterRed, 0x0002),
            new(EnemyDefinitionId.ShutterShootable, 0x0003),
        ]),
        new(0x8b11, [
            new(EnemyDefinitionId.Crocomire, 0xd007),
        ]),
        new(0x8b1f, [
        ]),
        new(0x8b29, [
            new(EnemyDefinitionId.Fune, 0x0001),
            new(EnemyDefinitionId.LavaRocks, 0x0002),
        ]),
        new(0x8b3b, [
            new(EnemyDefinitionId.Fune, 0x0001),
            new(EnemyDefinitionId.Gamet, 0x0002),
        ]),
        new(0x8b4d, [
            new(EnemyDefinitionId.Bull, 0x0001),
        ]),
        new(0x8b5b, [
            new(EnemyDefinitionId.Kamer2, 0x0001),
            new(EnemyDefinitionId.Skultera, 0x0002),
        ]),
        new(0x8b6d, [
            new(EnemyDefinitionId.Coven, 0x0001),
            new(EnemyDefinitionId.KzanTop, 0x0002),
            new(EnemyDefinitionId.Atomic, 0x0003),
            new(EnemyDefinitionId.Sbug, 0x0003),
        ]),
        new(0x8b87, [
            new(EnemyDefinitionId.Chozo, 0x0001),
            new(EnemyDefinitionId.Sbug, 0x0007),
        ]),
        new(0x8b99, [
            new(EnemyDefinitionId.KihunterYellow, 0x0001),
            new(EnemyDefinitionId.Atomic, 0x0002),
            new(EnemyDefinitionId.Spark, 0x0003),
        ]),
        new(0x8baf, [
            new(EnemyDefinitionId.Robot, 0x0001),
            new(EnemyDefinitionId.Atomic, 0x0002),
            new(EnemyDefinitionId.Spark, 0x0003),
        ]),
        new(0x8bc5, [
        ]),
        new(0x8bcf, [
        ]),
        new(0x8bd9, [
        ]),
        new(0x8be3, [
        ]),
        new(0x8bed, [
        ]),
        new(0x8bf7, [
        ]),
        new(0x8c01, [
            new(EnemyDefinitionId.Chozo, 0x0001),
            new(EnemyDefinitionId.Robot, 0x0003),
        ]),
        new(0x8c13, [
        ]),
        new(0x8c1d, [
        ]),
        new(0x8c27, [
        ]),
        new(0x8c31, [
        ]),
        new(0x8c3b, [
            new(EnemyDefinitionId.Coven, 0x0001),
            new(EnemyDefinitionId.Sbug, 0x0007),
            new(EnemyDefinitionId.Bull, 0x0007),
        ]),
        new(0x8c51, [
        ]),
        new(0x8c5b, [
            new(EnemyDefinitionId.RobotNoPower, 0x0007),
            new(EnemyDefinitionId.Sbug, 0x0007),
        ]),
        new(0x8c6d, [
            new(EnemyDefinitionId.Sbug, 0x0001),
        ]),
        new(0x8c7b, [
            new(EnemyDefinitionId.Spark, 0x0001),
            new(EnemyDefinitionId.Puyo, 0x0002),
            new(EnemyDefinitionId.Robot, 0x0003),
        ]),
        new(0x8c91, [
        ]),
        new(0x8c9b, [
            new(EnemyDefinitionId.Spark, 0x0001),
        ]),
        new(0x8ca9, [
            new(EnemyDefinitionId.Coven, 0x0001),
            new(EnemyDefinitionId.Atomic, 0x0002),
            new(EnemyDefinitionId.Sbug, 0x0002),
        ]),
        new(0x8cbf, [
            new(EnemyDefinitionId.Coven, 0x0001),
            new(EnemyDefinitionId.Atomic, 0x0002),
            new(EnemyDefinitionId.Sbug, 0x0002),
            new(EnemyDefinitionId.RobotNoPower, 0x0007),
        ]),
        new(0x8cd9, [
            new(EnemyDefinitionId.Coven, 0x0001),
            new(EnemyDefinitionId.Sbug, 0x0002),
        ]),
        new(0x8ceb, [
            new(EnemyDefinitionId.Spark, 0x0001),
        ]),
        new(0x8cf9, [
            new(EnemyDefinitionId.KzanTop, 0x0007),
        ]),
        new(0x8d07, [
            new(EnemyDefinitionId.Robot, 0x0001),
            new(EnemyDefinitionId.Bull, 0x0002),
            new(EnemyDefinitionId.Spark, 0x0003),
        ]),
        new(0x8d1d, [
            new(EnemyDefinitionId.PhantoonBody, 0x0007),
        ]),
        new(0x8d2b, [
            new(EnemyDefinitionId.Spark, 0x0001),
            new(EnemyDefinitionId.Atomic, 0x0002),
            new(EnemyDefinitionId.KzanTop, 0x0003),
        ]),
        new(0x8d41, [
            new(EnemyDefinitionId.Powamp, 0x0001),
            new(EnemyDefinitionId.Sciser, 0x0003),
        ]),
        new(0x8d53, [
            new(EnemyDefinitionId.Oum, 0x0001),
            new(EnemyDefinitionId.Mochtroid, 0x0002),
        ]),
        new(0x8d65, [
            new(EnemyDefinitionId.Choot, 0x0001),
            new(EnemyDefinitionId.Zeb, 0x0002),
        ]),
        new(0x8d77, [
            new(EnemyDefinitionId.Sciser, 0x0001),
        ]),
        new(0x8d85, [
            new(EnemyDefinitionId.Ripper, 0x0003),
            new(EnemyDefinitionId.Owtch, 0x0007),
        ]),
        new(0x8d97, [
            new(EnemyDefinitionId.Mochtroid, 0x0001),
        ]),
        new(0x8da5, [
        ]),
        new(0x8daf, [
        ]),
        new(0x8db9, [
            new(EnemyDefinitionId.Puyo, 0x0001),
            new(EnemyDefinitionId.Sciser, 0x0001),
            new(EnemyDefinitionId.Zoa, 0x0002),
        ]),
        new(0x8dcf, [
        ]),
        new(0x8dd9, [
        ]),
        new(0x8de3, [
            new(EnemyDefinitionId.Bull, 0x0001),
        ]),
        new(0x8df1, [
            new(EnemyDefinitionId.Bull, 0x0001),
        ]),
        new(0x8dff, [
            new(EnemyDefinitionId.Shaktool, 0x0001),
            new(EnemyDefinitionId.Yard, 0x0002),
        ]),
        new(0x8e10, [
            new(EnemyDefinitionId.DraygonBody, 0x0007),
        ]),
        new(0x8e1e, [
            new(EnemyDefinitionId.Yard, 0x0001),
        ]),
        new(0x8e2c, [
            new(EnemyDefinitionId.Skultera, 0x0001),
            new(EnemyDefinitionId.Sciser, 0x0001),
            new(EnemyDefinitionId.Zebbo, 0x0002),
        ]),
        new(0x8e42, [
            new(EnemyDefinitionId.PirateMagentaWalking, 0x0001),
            new(EnemyDefinitionId.PirateMagentaWall, 0x0002),
        ]),
        new(0x8e54, [
        ]),
        new(0x8e5e, [
            new(EnemyDefinitionId.Menu, 0x0001),
            new(EnemyDefinitionId.Puyo, 0x0002),
            new(EnemyDefinitionId.Tripper, 0x0002),
        ]),
        new(0x8e74, [
        ]),
        new(0x8e7e, [
            new(EnemyDefinitionId.NoobTubeCrack, 0x0007),
        ]),
        new(0x8e8c, [
        ]),
        new(0x8e96, [
            new(EnemyDefinitionId.Boyon, 0x0001),
            new(EnemyDefinitionId.Zebbo, 0x0002),
        ]),
        new(0x8ea8, [
            new(EnemyDefinitionId.MamaTurtle, 0x0001),
        ]),
        new(0x8eb6, [
            new(EnemyDefinitionId.Sciser, 0x0001),
        ]),
        new(0x8ec4, [
            new(EnemyDefinitionId.Owtch, 0x0001),
            new(EnemyDefinitionId.Menu, 0x0002),
        ]),
        new(0x8ed6, [
            new(EnemyDefinitionId.Owtch, 0x0001),
            new(EnemyDefinitionId.Choot, 0x0002),
            new(EnemyDefinitionId.Skultera, 0x0003),
        ]),
        new(0x8eec, [
            new(EnemyDefinitionId.Sciser, 0x0001),
        ]),
        new(0x8efa, [
            new(EnemyDefinitionId.Puyo, 0x0001),
            new(EnemyDefinitionId.MZoomer, 0x0001),
        ]),
        new(0x8f0c, [
            new(EnemyDefinitionId.Puyo, 0x0001),
            new(EnemyDefinitionId.Boyon, 0x0001),
            new(EnemyDefinitionId.Choot, 0x0002),
        ]),
        new(0x8f22, [
            new(EnemyDefinitionId.Owtch, 0x0001),
            new(EnemyDefinitionId.Choot, 0x0002),
            new(EnemyDefinitionId.Skultera, 0x0003),
        ]),
        new(0x8f38, [
            new(EnemyDefinitionId.Mochtroid, 0x0001),
            new(EnemyDefinitionId.Puyo, 0x0002),
            new(EnemyDefinitionId.Sciser, 0x0002),
            new(EnemyDefinitionId.Yard, 0x0003),
        ]),
        new(0x8f52, [
            new(EnemyDefinitionId.Evir, 0x0001),
        ]),
        new(0x8f60, [
        ]),
        new(0x8f6a, [
            new(EnemyDefinitionId.Evir, 0x0001),
        ]),
        new(0x8f78, [
            new(EnemyDefinitionId.Bull, 0x0001),
        ]),
        new(0x8f86, [
            new(EnemyDefinitionId.Bull, 0x0001),
        ]),
        new(0x8f94, [
            new(EnemyDefinitionId.Zoa, 0x0001),
        ]),
        new(0x8fa2, [
            new(EnemyDefinitionId.Mochtroid, 0x0001),
            new(EnemyDefinitionId.Puyo, 0x0002),
            new(EnemyDefinitionId.Sciser, 0x0002),
        ]),
        new(0x8fb8, [
            new(EnemyDefinitionId.Puyo, 0x0001),
            new(EnemyDefinitionId.Tripper, 0x0001),
        ]),
        new(0x8fca, [
        ]),
        new(0x8fd4, [
            new(EnemyDefinitionId.Zoa, 0x0001),
            new(EnemyDefinitionId.Owtch, 0x0002),
        ]),
        new(0x8fe6, [
            new(EnemyDefinitionId.Choot, 0x0001),
            new(EnemyDefinitionId.Puyo, 0x0002),
            new(EnemyDefinitionId.Boyon, 0x0002),
        ]),
        new(0x8ffc, [
        ]),
        new(0x9006, [
        ]),
        new(0x9010, [
        ]),
        new(0x901a, [
            new(EnemyDefinitionId.Sciser, 0x0001),
        ]),
        new(0x9028, [
            new(EnemyDefinitionId.Owtch, 0x0001),
            new(EnemyDefinitionId.Botwoon, 0x0007),
        ]),
        new(0x903a, [
            new(EnemyDefinitionId.Skultera, 0x0001),
            new(EnemyDefinitionId.Sciser, 0x0002),
        ]),
        new(0x904c, [
            new(EnemyDefinitionId.YappingMaw, 0x0001),
            new(EnemyDefinitionId.Zoa, 0x0007),
        ]),
        new(0x905e, [
            new(EnemyDefinitionId.Boulder, 0x0001),
        ]),
        new(0x906c, [
            new(EnemyDefinitionId.Boulder, 0x0001),
        ]),
        new(0x907a, [
            new(EnemyDefinitionId.Cacatac, 0x0007),
            new(EnemyDefinitionId.Zoa, 0x0007),
        ]),
        new(0x908c, [
            new(EnemyDefinitionId.Cacatac, 0x0001),
            new(EnemyDefinitionId.Zoa, 0x0001),
        ]),
        new(0x909e, [
            new(EnemyDefinitionId.Skultera, 0x0001),
            new(EnemyDefinitionId.PirateMagentaWalking, 0x0002),
        ]),
        new(0x90b0, [
            new(EnemyDefinitionId.PirateSilverWall, 0x0001),
            new(EnemyDefinitionId.PirateSilverWalking, 0x0002),
        ]),
        new(0x90c2, [
        ]),
        new(0x90cc, [
            new(EnemyDefinitionId.Metroid, 0x0001),
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x90de, [
            new(EnemyDefinitionId.CorpseTorizo, 0x0001),
        ]),
        new(0x90ec, [
            new(EnemyDefinitionId.CorpseSidehopper, 0x0001),
            new(EnemyDefinitionId.CorpseSidehopper2, 0x0001),
            new(EnemyDefinitionId.BabyMetroid, 0x0001),
        ]),
        new(0x9102, [
            new(EnemyDefinitionId.MotherBrainHead, 0x0001),
            new(EnemyDefinitionId.Rinka, 0x0002),
            new(EnemyDefinitionId.MotherBrainBody, 0xd001),
        ]),
        new(0x9118, [
        ]),
        new(0x9122, [
            new(EnemyDefinitionId.SidehopperTourian, 0x0001),
        ]),
        new(0x9130, [
            new(EnemyDefinitionId.ShutterDestroyable, 0x0001),
        ]),
        new(0x913e, [
        ]),
        new(0x9148, [
            new(EnemyDefinitionId.Metroid, 0x0001),
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x915a, [
            new(EnemyDefinitionId.Metroid, 0x0001),
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x916c, [
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x917a, [
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x9188, [
            new(EnemyDefinitionId.PirateSilverWall, 0x0001),
        ]),
        new(0x9196, [
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x91a4, [
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x91b2, [
            new(EnemyDefinitionId.PirateSilverWalking, 0x0001),
            new(EnemyDefinitionId.ShutterGrowing, 0x0002),
        ]),
        new(0x91c4, [
        ]),
        new(0x91ce, [
        ]),
        new(0x91ee, [
        ]),
        new(0x91f8, [
            new(EnemyDefinitionId.Metroid, 0x0001),
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x920a, [
            new(EnemyDefinitionId.Rinka, 0x0002),
        ]),
        new(0x9218, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x9226, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x9234, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x9242, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x9250, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x925e, [
            new(EnemyDefinitionId.RidleyCeres, 0x0001),
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x9270, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x927e, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x928c, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x929a, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x92a8, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
        new(0x92b6, [
            new(EnemyDefinitionId.CeresDoor, 0xd002),
        ]),
    ];
}
