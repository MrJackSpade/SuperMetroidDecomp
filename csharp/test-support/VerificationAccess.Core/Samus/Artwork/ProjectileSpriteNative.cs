using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Verification's native view of Samus projectile and charge-flare compositions: the bank-$93 address of each, as the
/// cartridge's timed records, flare table and WRAM slots store them.
/// </summary>
internal static class ProjectileSpriteNative
{
    /// <summary>The bank-$93 address of each projectile composition, in <see cref="ProjectileSpriteDefinitions.All"/> order.</summary>
    private static readonly ushort[] ProjectileAddresses =
    [
        0xa117, 0xa24d, 0xa254, 0xa25b, 0xa262, 0xa269, 0xa270, 0xa277, 0xa27e, 0xa37d, 0xa393, 0xa3b3,
        0xa3c9, 0xa7c9, 0xa7d0, 0xa7e6, 0xa7fc, 0xa812, 0xa828, 0xa83e, 0xa854, 0xa86a, 0xa880, 0xa896,
        0xaa84, 0xaa9a, 0xaab0, 0xaac6, 0xab04, 0xab2e, 0xab97, 0xab9e, 0xaba5, 0xabb3, 0xabba, 0xabc1,
        0xabd7, 0xabed, 0xac03, 0xad45, 0xad4c, 0xad53, 0xad5a, 0xad61, 0xad6d, 0xad7e, 0xad8a, 0xad9b,
        0xada7, 0xadb8, 0xadc4, 0xadd5, 0xade1, 0xadf2, 0xadfe, 0xae0f, 0xae1b, 0xae2c, 0xae38, 0xae65,
        0xae6c, 0xae73, 0xae7a, 0xae81, 0xae88, 0xae8f, 0xae96, 0xae9d, 0xaea4, 0xaeab, 0xaeb2, 0xaeb9,
        0xaec0, 0xaec7, 0xaece, 0xaed5, 0xaedc, 0xaee3, 0xaeea, 0xaef1, 0xaef8, 0xaeff, 0xaf06, 0xaf0d,
        0xaf14, 0xaf1b, 0xaf22, 0xaf29, 0xaf30, 0xaf37, 0xaf3e, 0xaf45, 0xaf4c, 0xaf62, 0xaf78, 0xafa2,
        0xafcc, 0xaff6, 0xb020, 0xb04a, 0xb074, 0xb09e, 0xb0c8, 0xb0f2, 0xb11c, 0xb146, 0xb170, 0xb19a,
        0xb1c4, 0xb1ee, 0xb218, 0xb242, 0xb26c, 0xb296, 0xb2c0, 0xb2ea, 0xb314, 0xb33e, 0xb394, 0xb3be,
        0xb3e8, 0xb412, 0xb43c, 0xb466, 0xb490, 0xb4ba, 0xb672, 0xb688, 0xb69e, 0xb6c8, 0xb6f2, 0xb71c,
        0xb746, 0xb770, 0xb79a, 0xb7c4, 0xb7ee, 0xb818, 0xb842, 0xb86c, 0xb896, 0xb8c0, 0xb8ea, 0xb914,
        0xb93e, 0xb968, 0xb992, 0xb9bc, 0xb9e6, 0xba10, 0xba3a, 0xba64, 0xbaba, 0xbae4, 0xbb0e, 0xbb38,
        0xbb62, 0xbb8c, 0xbbb6, 0xbbe0, 0xbc0a, 0xbc20, 0xbc4a, 0xbc74, 0xbc9e, 0xbcc8, 0xbced, 0xbd35,
        0xbd7d, 0xbdc5, 0xbe0d, 0xbe2d, 0xbe6b, 0xbea9, 0xbee7, 0xbf25, 0xbf59, 0xbfbf, 0xc025, 0xc08b,
        0xc0f1, 0xc107, 0xc131, 0xc15b, 0xc185, 0xc1af, 0xc1d4, 0xc21c, 0xc264, 0xc2ac, 0xc3b2, 0xc3d7,
        0xc41f, 0xc467, 0xc4af, 0xc669, 0xc6a7, 0xc721, 0xc79b, 0xc815, 0xc94d, 0xc972, 0xc9ba, 0xca02,
        0xca4a, 0xcc04, 0xcc42, 0xccbc, 0xcd36, 0xcdb0, 0xce2a, 0xce4a, 0xce88, 0xcec6, 0xcf04, 0xcf42,
        0xcf76, 0xcfdc, 0xd042, 0xd0a8, 0xd10e, 0xd124, 0xd162, 0xd1a0, 0xd1de, 0xd21c, 0xd25a, 0xd270,
        0xd2ae, 0xd2ec, 0xd32a, 0xd368, 0xd3a6, 0xd3bc, 0xd3fa, 0xd438, 0xd476, 0xd4f2, 0xd508, 0xd546,
        0xd584, 0xd5c2, 0xd600, 0xd63e, 0xd64a, 0xd66a, 0xd68a, 0xd6aa, 0xd6ca, 0xd6ea, 0xd6f6, 0xd716,
        0xd736, 0xd756, 0xd776, 0xd796, 0xd7a2, 0xd7c2, 0xd7e2, 0xd802, 0xd822, 0xd842, 0xd84e, 0xd86e,
        0xd88e, 0xd8ae, 0xd8ce, 0xd8ee, 0xd904, 0xd942, 0xd980, 0xd9be, 0xd9fc, 0xda3a, 0xda50, 0xda8e,
        0xdacc, 0xdb0a, 0xdb48, 0xdb86, 0xdba6, 0xdc02, 0xdc5e, 0xdcba, 0xdd16, 0xdd72, 0xdd92, 0xddee,
        0xde4a, 0xdea6, 0xdf02, 0xdf5e, 0xdf7e, 0xdfda, 0xe036, 0xe092, 0xe0ee, 0xe14a, 0xe16a, 0xe1c6,
        0xe222, 0xe27e, 0xe2da, 0xe336, 0xe356, 0xe3b2, 0xe40e, 0xe46a, 0xe4c6, 0xe522, 0xe542, 0xe59e,
        0xe5fa, 0xe656, 0xe6b2, 0xe70e, 0xe724, 0xe762, 0xe7a0, 0xe7de, 0xe81c, 0xe85a, 0xe870, 0xe8ae,
        0xe8ec, 0xe92a, 0xe968, 0xe9a6, 0xe9bc, 0xe9fa, 0xea38, 0xea76, 0xeab4, 0xeaf2, 0xeb08, 0xeb46,
        0xeb84, 0xebc2, 0xec00, 0xec3e, 0xec54, 0xec6a, 0xec80, 0xec96, 0xecac, 0xecc2, 0xecd8, 0xecee,
        0xed04, 0xed1a, 0xed30, 0xed46, 0xed5c, 0xed72, 0xed88, 0xed9e, 0xedb4, 0xedca, 0xede0, 0xedf6,
        0xedfd, 0xee04, 0xee0b, 0xee12, 0xee19, 0xee4c, 0xee58, 0xee8e, 0xee95, 0xeec8, 0xeed4, 0xef0a,
        0xef11, 0xef44, 0xef50, 0xef86, 0xef8d, 0xf002, 0xf009, 0xf03c, 0xf048, 0xf07e, 0xf085, 0xf0b8,
        0xf0c4, 0xf0fa, 0xf10d, 0xf14f, 0xf16f, 0xf194, 0xf1a7, 0xf1e9, 0xf209, 0xf22e, 0xf23a, 0xf270,
        0xf29a, 0xf2ce, 0xf2da, 0xf310, 0xf33a, 0xf36e, 0xf381, 0xf3c3, 0xf3e3, 0xf408, 0xf41b, 0xf45d,
        0xf47d, 0xf4a2, 0xf4ae, 0xf4e4, 0xf50e, 0xf542, 0xf54e, 0xf584, 0xf5ae,
    ];

    /// <summary>The bank-$93 address of each flare composition, in <see cref="ChargeFlareSpriteDefinitions.All"/> order.</summary>
    internal static readonly ushort[] FlareAddresses =
    [
        0xab6c, 0xab73, 0xab7a, 0xab81, 0xa6fd, 0xa70e, 0xa71f, 0xa730, 0xa741, 0xa752, 0xa763, 0xa774,
        0xa785, 0xa796, 0xa7a7, 0xa7b8, 0xa8de, 0xa8ef, 0xa900, 0xa911, 0xa922, 0xa933, 0xa944, 0xa955,
        0xa966, 0xa977, 0xa988, 0xa999,
    ];

    /// <summary>The bank-$93 address of <paramref name="sprite"/>.</summary>
    internal static ushort Address(ProjectileSpriteId sprite) => sprite.Group >= ProjectileSpriteGroup.FlareMain
        ? FlareAddresses[Place(ChargeFlareSpriteDefinitions.All, sprite)]
        : ProjectileAddresses[Place(ProjectileSpriteDefinitions.All, sprite)];

    /// <summary>The projectile composition stored at bank-$93 <paramref name="address"/>.</summary>
    /// <exception cref="InvalidDataException">No projectile composition is stored there.</exception>
    internal static ProjectileSpriteId Projectile(int address) => Array.IndexOf(ProjectileAddresses, (ushort)address) is int place and >= 0
        ? ProjectileSpriteDefinitions.All[place]
        : throw new InvalidDataException($"No projectile composition is stored at $93:{address:X4}.");

    /// <summary>The timed record stored at bank-$93 <paramref name="address"/>.</summary>
    /// <exception cref="InvalidDataException">No timed record is stored there.</exception>
    internal static SamusProjectileFrame Frame(int address) =>
        Array.FindIndex(SamusProjectileRecordDefinitions.Records, record => record.Address == address) is int record and >= 0
            ? new SamusProjectileFrame(record)
            : throw new InvalidDataException($"No timed projectile record is stored at $93:{address:X4}.");

    /// <summary>Where <paramref name="sprite"/> sits in <paramref name="all"/>.</summary>
    private static int Place(IReadOnlyList<ProjectileSpriteId> all, ProjectileSpriteId sprite)
    {
        for (int place = 0; place < all.Count; place++)
            if (all[place] == sprite)
                return place;
        throw new InvalidDataException($"{sprite.Name} is not one of these compositions.");
    }

    /// <summary>Native views of the projectile compositions.</summary>
    extension(ProjectileSpriteDefinitions)
    {
        /// <summary>The bank-$93 address of each projectile composition, in storage order.</summary>
        internal static IReadOnlyList<ushort> NativePointers => ProjectileAddresses;

        /// <summary>The installed name of the projectile composition stored at bank-$93 <paramref name="address"/>.</summary>
        internal static string Name(int address) => Projectile(address).Name;
    }

    /// <summary>The timed records by bank-$93 address.</summary>
    extension(SamusProjectileRecordDefinitions)
    {
        /// <summary>Every timed record's bank-$93 address, in address order.</summary>
        internal static IReadOnlyList<ushort> TimedRecordAddresses =>
            [.. SamusProjectileRecordDefinitions.Records.Select(record => checked((ushort)record.Address)).Order()];
    }

    /// <summary>A timed record's stock composition by address.</summary>
    extension(SamusProjectileRecord record)
    {
        /// <summary>The bank-$93 address of the record's stock composition.</summary>
        internal int Spritemap => Address(record.Sprite);
    }

    /// <summary>Binding keys by record address.</summary>
    extension(ProjectileFrameBindingFormat)
    {
        /// <summary>The binding key of the timed record stored at bank-$93 <paramref name="address"/>.</summary>
        internal static string FrameNameAt(int address) => ProjectileFrameBindingFormat.FrameName(Frame(address).Record);
    }

    /// <summary>Installed bindings by record address.</summary>
    extension(ProjectileFrameBindingCatalog bindings)
    {
        /// <summary>The bank-$93 address of the composition the record stored at bank-$93 <paramref name="address"/> draws.</summary>
        internal ushort Resolve(int address) => Address(bindings.Resolve(Frame(address)));
    }

    /// <summary>A projectile slot's composition by address.</summary>
    extension(SamusProjectileSlot slot)
    {
        /// <summary>The bank-$93 address of the composition the slot shows, or zero for none.</summary>
        internal int SpritemapPointer
        {
            get => slot.Sprite is { } sprite ? Address(sprite) : 0;
            set => slot.Sprite = value == 0 ? null : Projectile(value);
        }
    }

    /// <summary>A bomb slot's composition by address.</summary>
    extension(SamusBombProjectileSlot slot)
    {
        /// <summary>The bank-$93 address of the composition the bomb shows, or zero for none.</summary>
        internal int SpritemapPointer
        {
            get => slot.Sprite is { } sprite ? Address(sprite) : 0;
            set => slot.Sprite = value == 0 ? null : Projectile(value);
        }
    }
}
