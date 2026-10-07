namespace SuperMetroid.Core.Game;

/// <summary>
/// Native ASCII names copied opaquely into the six-word spawn snapshot. Only the chosen
/// lexical spelling is retained as nonsense to derive; record layout and family composition calculate.
/// </summary>
internal static class RoomEnemySpawnNameDefinitions
{
    private enum DerivedName : ushort
    {
        /// <summary>$B4:DEF5, EnemyName_HZoomer: prefixed Zoomer label.</summary>
        HZoomer = 0xdef5,
        /// <summary>$B4:DFC7, EnemyName_MZoomer: prefixed Zoomer label.</summary>
        MZoomer = 0xdfc7,
        /// <summary>$B4:E14F, EnemyName_Sidehopper: prefixed SIDE label.</summary>
        Sidehopper = 0xe14f,
        /// <summary>$B4:DDB3, EnemyName_PirateGreyWall: native family/variant label identity.</summary>
        PirateGreyWall = 0xddb3,
        /// <summary>$B4:DDCF, EnemyName_PirateGreyWalking: native family/variant label identity.</summary>
        PirateGreyWalking = 0xddcf,
        /// <summary>$B4:DEA1, EnemyName_KihunterGreen: native family/variant label identity.</summary>
        KihunterGreen = 0xdea1,
        /// <summary>$B4:E099, EnemyName_GRipper_Ripper2: native family/variant label identity.</summary>
        GRipper_Ripper2 = 0xe099,
        /// <summary>$B4:E109, EnemyName_Shutter2_Kamer: native family/variant label identity.</summary>
        Shutter2_Kamer = 0xe109,
        /// <summary>$B4:E205, EnemyName_PirateGoldWall: native family/variant label identity.</summary>
        PirateGoldWall = 0xe205,
        /// <summary>$B4:E213, EnemyName_PirateMagentaWall: native family/variant label identity.</summary>
        PirateMagentaWall = 0xe213,
        /// <summary>$B4:E221, EnemyName_PirateSilverWall: native family/variant label identity.</summary>
        PirateSilverWall = 0xe221,
        /// <summary>$B4:E24B, EnemyName_PirateGoldNinja: native family/variant label identity.</summary>
        PirateGoldNinja = 0xe24b,
        /// <summary>$B4:E267, EnemyName_PirateSilverNinja: native family/variant label identity.</summary>
        PirateSilverNinja = 0xe267,
        /// <summary>$B4:E275, EnemyName_PirateGreenWalking: native family/variant label identity.</summary>
        PirateGreenWalking = 0xe275,
        /// <summary>$B4:E283, EnemyName_PirateRedWalking: native family/variant label identity.</summary>
        PirateRedWalking = 0xe283,
        /// <summary>$B4:E291, EnemyName_PirateGoldWalking: native family/variant label identity.</summary>
        PirateGoldWalking = 0xe291,
        /// <summary>$B4:E29F, EnemyName_PirateMagentaWalking: native family/variant label identity.</summary>
        PirateMagentaWalking = 0xe29f,
        /// <summary>$B4:E2AD, EnemyName_PirateSilverWalking: native family/variant label identity.</summary>
        PirateSilverWalking = 0xe2ad,
        /// <summary>$B4:E2C9, EnemyName_KihunterYellow: native family/variant label identity.</summary>
        KihunterYellow = 0xe2c9,
        /// <summary>$B4:E2D7, EnemyName_KihunterRed: native family/variant label identity.</summary>
        KihunterRed = 0xe2d7,
        /// <summary>$B4:E2E5, EnemyName_RobotNoPower: native family/variant label identity.</summary>
        RobotNoPower = 0xe2e5,
    }

    /// <summary>$B4:DD89, EnemyName_NoData: first fourteen-byte ASCII/population/debug-index record.</summary>
    private const ushort FirstNameRecord = 0xdd89;

    /// <summary>
    /// Chosen label text from bank B4, copied opaquely by $A0:8923..8968 and retained in
    /// RoomEnemySpawnSnapshot.NameWords. No AI/physics quantity determines these spellings.
    /// Only the exact 69 labels plus ten composing lexical fragments documented in
    /// #1165 (stream 4, Batch 49) have the reviewed nonsense disposition.
    /// </summary>
    private static readonly Dictionary<ushort, string> Names = new Dictionary<ushort, string>
    {
        [0xdd97] = "ATOMIC",
        [0xdddd] = "BOTOON",
        [0xddeb] = "BOYON",
        [0xddf9] = "DESSGEEGA",
        [0xde07] = "DORI",
        [0xde15] = "DRAGON",
        [0xde23] = "EBI",
        [0xde31] = "EYE",
        [0xde3f] = "NAMI",
        [0xde4d] = "FISH",
        [0xde5b] = "GAI",
        [0xde69] = "GAMET",
        [0xde77] = "GEEGA",
        [0xde85] = "GERUDA",
        [0xdeaf] = "HAND",
        [0xdebd] = "HIBASHI",
        [0xdecb] = "HIRU",
        [0xded9] = "HOLTZ",
        [0xdee7] = "HOTARY",
        [0xdf03] = "KAGO",
        [0xdf11] = "KAME",
        [0xdf1f] = "KAMER",
        [0xdf2d] = "KANI",
        [0xdf3b] = "KOMA",
        [0xdf49] = "KZAN",
        [0xdf57] = "LAVAMAN",
        [0xdf65] = "MELLA",
        [0xdf73] = "MEMU",
        [0xdf81] = "MERO",
        [0xdf8f] = "METALEE",
        [0xdf9d] = "METMOD",
        [0xdfab] = "METROID",
        [0xdfb9] = "MULTI",
        [0xdfd5] = "NDRA",
        [0xdfe3] = "NOMI",
        [0xdff1] = "NOVA",
        [0xdfff] = "OUM",
        [0xe00d] = "OUMU",
        [0xe01b] = "PIPE",
        [0xe029] = "POLYP",
        [0xe037] = "PUROMI",
        [0xe045] = "PUU",
        [0xe053] = "PUYO",
        [0xe061] = "REFLEC",
        [0xe06f] = "RINKA",
        [0xe07d] = "RIO",
        [0xe08b] = "RIPPER",
        [0xe0a7] = "ROBO",
        [0xe0b5] = "RSTONE",
        [0xe0c3] = "SABOTEN",
        [0xe0d1] = "SBUG",
        [0xe0df] = "SCLAYD",
        [0xe0ed] = "SDEATH",
        [0xe0fb] = "SHUTTER",
        [0xe117] = "SIDE",
        [0xe125] = "SKREE",
        [0xe133] = "SPA",
        [0xe141] = "SQUEEWPT",
        [0xe15d] = "STOKE",
        [0xe16b] = "TOGE",
        [0xe179] = "VIOLA",
        [0xe187] = "WAVER",
        [0xe195] = "YARD",
        [0xe1a3] = "ZEB",
        [0xe1b1] = "ZEBBO",
        [0xe1bf] = "ZEELA",
        [0xe1cd] = "ZOA",
        [0xe1db] = "ZOOMER",
        [0xe2bb] = "FUNE",
    };

    /// <summary>
    /// $A0:88D0 RecordEnemySpawnData copies five little-endian ASCII words, skips the
    /// debug population pointer, then copies the ordinal debug-spritemap index.
    /// </summary>
    internal static RoomEnemySpawnNameWords Get(ushort pointer)
    {
        string name = Name(pointer) ?? throw new InvalidDataException(
            $"Enemy spawn-name pointer $B4:{pointer:X4} is absent from the compiled retail catalog.");
        return new(Word(0), Word(1), Word(2), Word(3), Word(4),
            (ushort)((pointer - FirstNameRecord) / 14));

        ushort Word(int index) => (ushort)(Character(index * 2) | Character(index * 2 + 1) << 8);
        int Character(int index) => index < name.Length ? name[index] : ' ';
    }

    /// <summary>The identities referenced by all retail enemy headers, in native record order.</summary>
    internal static IEnumerable<ushort> Pointers
    {
        get
        {
            for (int pointer = FirstNameRecord; pointer <= LastNameRecord; pointer += 14)
                if (Name((ushort)pointer) is not null) yield return (ushort)pointer;
        }
    }

    /// <summary>$B4:E2E5, EnemyName_RobotNoPower: final fourteen-byte name record.</summary>
    private const ushort LastNameRecord = 0xe2e5;

    /// <summary>$B4:E08B, EnemyName_Ripper: lexical stem shared by the Ripper2 label.</summary>
    private const ushort RipperStem = 0xe08b;
    /// <summary>$B4:E0FB, EnemyName_ShutterGrowing: lexical stem shared by the Shutter2 label.</summary>
    private const ushort ShutterStem = 0xe0fb;
    /// <summary>$B4:E0A7, EnemyName_Robot: lexical stem shared by the RobotNoPower label.</summary>
    private const ushort RobotStem = 0xe0a7;
    /// <summary>$B4:E1DB, EnemyName_Zoomer: shared Zoomer label stem.</summary>
    private const ushort ZoomerStem = 0xe1db;
    /// <summary>$B4:E117, EnemyName_SidehopperLarge_SidehopperTourian: shared SIDE label stem.</summary>
    private const ushort SideStem = 0xe117;

    /// <summary>Native BATTA family digits: wall=1, ninja=2, walking=3.</summary>
    private enum PirateKind { Wall = 1, Ninja = 2, Walking = 3 }
    /// <summary>Exclusive native pirate color variants used by EnemyName_Pirate* records.</summary>
    private enum PirateColor { Grey, Green, Red, Gold, Magenta, Silver }

    private static string PirateName(PirateKind kind, PirateColor color) =>
        "BATTA" + (char)('0' + (int)kind) + (color switch
        {
            PirateColor.Grey => "",
            PirateColor.Green => "Br",
            PirateColor.Red => "No",
            PirateColor.Gold => "Na",
            PirateColor.Magenta => "Ma",
            PirateColor.Silver => "Tu",
            _ => throw new InvalidOperationException("Unknown pirate label color."),
        });
    // Numeric family identifiers and shared lexical stems are composed per access;
    // only the chosen lexical stems/suffixes/prefixes have the narrow retained disposition.
    private static string? Name(ushort pointer) => (DerivedName)pointer switch
    {
        DerivedName.HZoomer => 'H' + Names[ZoomerStem],
        DerivedName.MZoomer => 'M' + Names[ZoomerStem],
        DerivedName.Sidehopper => 'S' + Names[SideStem],
        DerivedName.PirateGreyWall => PirateName(PirateKind.Wall, PirateColor.Grey),
        DerivedName.PirateGreyWalking => PirateName(PirateKind.Walking, PirateColor.Grey),
        DerivedName.KihunterGreen => "HACHI" + (char)('0' + 1),
        DerivedName.GRipper_Ripper2 => Names[RipperStem] + '2',
        DerivedName.Shutter2_Kamer => Names[ShutterStem] + '2',
        DerivedName.PirateGoldWall => PirateName(PirateKind.Wall, PirateColor.Gold),
        DerivedName.PirateMagentaWall => PirateName(PirateKind.Wall, PirateColor.Magenta),
        DerivedName.PirateSilverWall => PirateName(PirateKind.Wall, PirateColor.Silver),
        DerivedName.PirateGoldNinja => PirateName(PirateKind.Ninja, PirateColor.Gold),
        DerivedName.PirateSilverNinja => PirateName(PirateKind.Ninja, PirateColor.Silver),
        DerivedName.PirateGreenWalking => PirateName(PirateKind.Walking, PirateColor.Green),
        DerivedName.PirateRedWalking => PirateName(PirateKind.Walking, PirateColor.Red),
        DerivedName.PirateGoldWalking => PirateName(PirateKind.Walking, PirateColor.Gold),
        DerivedName.PirateMagentaWalking => PirateName(PirateKind.Walking, PirateColor.Magenta),
        DerivedName.PirateSilverWalking => PirateName(PirateKind.Walking, PirateColor.Silver),
        DerivedName.KihunterYellow => "HACHI" + (char)('0' + 2),
        DerivedName.KihunterRed => "HACHI" + (char)('0' + 3),
        DerivedName.RobotNoPower => Names[RobotStem] + '2',
        _ => Names.TryGetValue(pointer, out string? name) ? name : null,
    };
}
