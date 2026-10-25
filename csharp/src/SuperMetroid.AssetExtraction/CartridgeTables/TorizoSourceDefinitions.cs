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
    /// <summary>The cartridge source of each installed Torizo character page.</summary>
    internal static IReadOnlyDictionary<TorizoTilePage, int> PageSources { get; } = new Dictionary<TorizoTilePage, int>
    {
        // $AA:B0A5, the shared Bomb/Golden Torizo death and recovery characters.
        [TorizoTilePage.SharedDeath] = 0xaab0a5,
        // $AA:B279, the alternating Bomb Torizo statue-crumble page.
        [TorizoTilePage.StatueCrumble] = 0xaab279,
        // $AA:B479, the left-facing Bomb Torizo attack and death characters.
        [TorizoTilePage.LeftAttack] = 0xaab479,
        // $AA:B679, the right-facing Bomb Torizo attack and death characters.
        [TorizoTilePage.RightAttack] = 0xaab679,
        // $AF:E200, the initial Golden Torizo character upload.
        [TorizoTilePage.GoldenAwakening] = 0xafe200,
        // $AF:C800, Golden Torizo's alternate left-side attack characters.
        [TorizoTilePage.GoldenLeftAttack] = 0xafc800,
        // $AF:CA00, Golden Torizo's alternate right-side attack characters.
        [TorizoTilePage.GoldenRightAttack] = 0xafca00,
        // $AD:B200, <c>Tiles_BombTorizosCrumblingChozo</c>, the fragments PLM $84:D376 uploads.
        [TorizoTilePage.ChozoDebris] = 0xadb200,
    };
}
