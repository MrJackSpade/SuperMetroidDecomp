using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Torizo character pages extraction reads (#1413): the import pipeline's private
/// knowledge of the cartridge, decoded once into the installed Torizo tile sheets.
/// </summary>
[CpuBusBoundary("catalog: Torizo cartridge sources")]
internal static class TorizoSourceDefinitions
{
    /// <summary>$AA:B0A5, the shared Bomb/Golden Torizo death and recovery characters.</summary>
    private const int SharedDeath = 0xaab0a5;

    /// <summary>$AA:B279, the alternating Bomb Torizo statue-crumble page.</summary>
    private const int StatueCrumble = 0xaab279;

    /// <summary>$AA:B479, the left-facing Bomb Torizo attack and death characters.</summary>
    private const int LeftAttack = 0xaab479;

    /// <summary>$AA:B679, the right-facing Bomb Torizo attack and death characters.</summary>
    private const int RightAttack = 0xaab679;

    /// <summary>$AF:E200, the initial Golden Torizo character upload.</summary>
    private const int GoldenAwakening = 0xafe200;

    /// <summary>$AF:C800, Golden Torizo's alternate left-side attack characters.</summary>
    private const int GoldenLeftAttack = 0xafc800;

    /// <summary>$AF:CA00, Golden Torizo's alternate right-side attack characters.</summary>
    private const int GoldenRightAttack = 0xafca00;

    /// <summary>$AD:B200, <c>Tiles_BombTorizosCrumblingChozo</c>, the fragments PLM $84:D376 uploads.</summary>
    private const int ChozoDebris = 0xadb200;

    /// <summary>The cartridge source of each installed Torizo character page.</summary>
    internal static IReadOnlyDictionary<TorizoTilePage, int> PageSources { get; } = new Dictionary<TorizoTilePage, int>
    {
        [TorizoTilePage.SharedDeath] = SharedDeath,
        [TorizoTilePage.StatueCrumble] = StatueCrumble,
        [TorizoTilePage.LeftAttack] = LeftAttack,
        [TorizoTilePage.RightAttack] = RightAttack,
        [TorizoTilePage.GoldenAwakening] = GoldenAwakening,
        [TorizoTilePage.GoldenLeftAttack] = GoldenLeftAttack,
        [TorizoTilePage.GoldenRightAttack] = GoldenRightAttack,
        [TorizoTilePage.ChozoDebris] = ChozoDebris,
    };
}
