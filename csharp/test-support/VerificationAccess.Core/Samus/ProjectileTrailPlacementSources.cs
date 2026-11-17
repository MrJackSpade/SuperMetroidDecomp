using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Where each trail placement list is stored in bank $9B, decoded from the cartridge's own trail selector and direction
/// tables: a selector row points at its family's direction table, and each direction entry points at a list.
/// </summary>
internal static class ProjectileTrailPlacementSources
{
    /// <summary>Bytes in one placement record: the left and right sparkles' signed X and Y offsets.</summary>
    internal const int RecordByteCount = 4;

    /// <summary>
    /// How many placements each list holds of its own, up to where the next list begins (#1926): a frame whose trail word
    /// reads past them leaves no trail (#1625).
    /// </summary>
    internal static readonly IReadOnlyDictionary<ProjectileTrailPlacementList, int> OwnPlacementCounts =
        new Dictionary<ProjectileTrailPlacementList, int>
        {
            [ProjectileTrailPlacementList.UnchargedDefault] = 8,
            [ProjectileTrailPlacementList.UnchargedWaveVertical] = 16,
            [ProjectileTrailPlacementList.UnchargedWaveHorizontal] = 16,
            [ProjectileTrailPlacementList.UnchargedWaveRisingDiagonal] = 16,
            [ProjectileTrailPlacementList.UnchargedWaveFallingDiagonal] = 16,
            [ProjectileTrailPlacementList.UnchargedIceSpazerUp] = 3,
            [ProjectileTrailPlacementList.UnchargedIceSpazerDown] = 3,
            [ProjectileTrailPlacementList.UnchargedIceSpazerUpRight] = 3,
            [ProjectileTrailPlacementList.UnchargedIceSpazerRight] = 3,
            [ProjectileTrailPlacementList.UnchargedIceSpazerDownRight] = 3,
            [ProjectileTrailPlacementList.UnchargedIceSpazerDownLeft] = 3,
            [ProjectileTrailPlacementList.UnchargedIceSpazerLeft] = 3,
            [ProjectileTrailPlacementList.UnchargedIceSpazerUpLeft] = 3,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerUp] = 10,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerUpRight] = 10,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerRight] = 10,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerDownRight] = 10,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerDown] = 10,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerDownLeft] = 10,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerLeft] = 10,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerUpLeft] = 10,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaUp] = 2,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaUpRight] = 2,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaRight] = 2,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaDownRight] = 2,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaDown] = 2,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaDownLeft] = 2,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaLeft] = 2,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaUpLeft] = 2,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaUp] = 9,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaUpRight] = 9,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaRight] = 9,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaDownRight] = 9,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaDown] = 9,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaDownLeft] = 9,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaLeft] = 9,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaUpLeft] = 9,
            [ProjectileTrailPlacementList.ChargedDefault] = 8,
            [ProjectileTrailPlacementList.ChargedWaveVertical] = 16,
            [ProjectileTrailPlacementList.ChargedWaveHorizontal] = 16,
            [ProjectileTrailPlacementList.ChargedWaveRisingDiagonal] = 16,
            [ProjectileTrailPlacementList.ChargedWaveFallingDiagonal] = 16,
            [ProjectileTrailPlacementList.ChargedIceSpazerUp] = 10,
            [ProjectileTrailPlacementList.ChargedIceSpazerUpRight] = 10,
            [ProjectileTrailPlacementList.ChargedIceSpazerRight] = 10,
            [ProjectileTrailPlacementList.ChargedIceSpazerDownRight] = 10,
            [ProjectileTrailPlacementList.ChargedIceSpazerDown] = 10,
            [ProjectileTrailPlacementList.ChargedIceSpazerDownLeft] = 10,
            [ProjectileTrailPlacementList.ChargedIceSpazerLeft] = 10,
            [ProjectileTrailPlacementList.ChargedIceSpazerUpLeft] = 10,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerUp] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerUpRight] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerRight] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerDownRight] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerDown] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerDownLeft] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerLeft] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerUpLeft] = 22,
            [ProjectileTrailPlacementList.ChargedIcePlasmaUp] = 8,
            [ProjectileTrailPlacementList.ChargedIcePlasmaUpRight] = 8,
            [ProjectileTrailPlacementList.ChargedIcePlasmaRight] = 8,
            [ProjectileTrailPlacementList.ChargedIcePlasmaDownRight] = 8,
            [ProjectileTrailPlacementList.ChargedIcePlasmaDown] = 8,
            [ProjectileTrailPlacementList.ChargedIcePlasmaDownLeft] = 8,
            [ProjectileTrailPlacementList.ChargedIcePlasmaLeft] = 8,
            [ProjectileTrailPlacementList.ChargedIcePlasmaUpLeft] = 8,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaUp] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaUpRight] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaRight] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaDownRight] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaDown] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaDownLeft] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaLeft] = 22,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaUpLeft] = 22,
            [ProjectileTrailPlacementList.SpazerAttackTrailingPair] = 3,
            [ProjectileTrailPlacementList.SpazerAttackTrailingPairDown] = 3,
        };

    /// <summary>
    /// The address of <paramref name="list"/>'s first placement, read through the selector row and direction entry of a
    /// family that names it, or of the Spazer special attack's trailing pair, whose row is the Spazer row of the special
    /// beam attacks' selector table (#2348).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No selector row's family names <paramref name="list"/>.</exception>
    internal static SnesAddress Start(IImportCartridgeSource cartridge, ProjectileTrailPlacementList list)
    {
        if (SamusProjectileDirectionCodec.InAimOrder.Where(direction => ProjectileTrailPlacements.SpazerAttackTrailingPairList(direction) == list).ToArray()
            is [var trailing, ..])
        {
            SnesAddress row = SamusSourceDefinitionsConstants2.SpecialBeamAttackTrailSelectors
                .AddWithinBank(SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail).BeamCombination.TableIndex * sizeof(ushort));
            return Pointer(cartridge, Pointer(cartridge, row).AddWithinBank(trailing.ToAimIndex() * sizeof(ushort)));
        }
        foreach ((ProjectileTrailChoice row, ProjectileTrailFamily? family) in ProjectileTrailPlacementDefinitions.Families)
        {
            if (family is not { } named)
                continue;
            SamusProjectileDirectionMap<ProjectileTrailPlacementList?> lists = ProjectileTrailPlacementDefinitions.Lists[named];
            if (SamusProjectileDirectionCodec.InAimOrder.Where(direction => lists.For(direction) == list).ToArray() is not [var direction, ..])
                continue;
            SnesAddress selector = SelectorTable(row.Group).AddWithinBank(row.Beams.TableIndex * sizeof(ushort));
            SnesAddress entry = Pointer(cartridge, selector).AddWithinBank(direction.ToAimIndex() * sizeof(ushort));
            return Pointer(cartridge, entry);
        }
        throw new ArgumentOutOfRangeException(nameof(list), list, "No trail selector row reaches this placement list.");
    }

    /// <summary>The selector table a projectile of <paramref name="group"/> reads its row from.</summary>
    private static SnesAddress SelectorTable(ProjectileTrailFamilyGroup group) => group switch
    {
        ProjectileTrailFamilyGroup.Uncharged => SamusSourceDefinitionsConstants2.UnchargedTrailSelectors,
        ProjectileTrailFamilyGroup.Charged => SamusSourceDefinitionsConstants2.ChargedTrailSelectors,
        _ => throw new ArgumentOutOfRangeException(nameof(group), group, "Undefined trail selector table."),
    };

    /// <summary>The bank-local pointer stored at <paramref name="address"/>, in <paramref name="address"/>'s bank.</summary>
    private static SnesAddress Pointer(IImportCartridgeSource cartridge, SnesAddress address) =>
        new(address.Bank, RomDataReader.ReadWordFixedBank(cartridge, address));
}
