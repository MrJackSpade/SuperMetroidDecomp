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
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedDefault, ProjectileTrailCourse.AnyDirection)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWave, ProjectileTrailCourse.Vertical)] = 16,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWave, ProjectileTrailCourse.Horizontal)] = 16,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWave, ProjectileTrailCourse.RisingDiagonal)] = 16,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWave, ProjectileTrailCourse.FallingDiagonal)] = 16,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIceSpazer, ProjectileTrailCourse.Up)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIceSpazer, ProjectileTrailCourse.Down)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIceSpazer, ProjectileTrailCourse.UpRight)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIceSpazer, ProjectileTrailCourse.Right)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIceSpazer, ProjectileTrailCourse.DownRight)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIceSpazer, ProjectileTrailCourse.DownLeft)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIceSpazer, ProjectileTrailCourse.Left)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIceSpazer, ProjectileTrailCourse.UpLeft)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIceSpazer, ProjectileTrailCourse.Up)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIceSpazer, ProjectileTrailCourse.UpRight)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIceSpazer, ProjectileTrailCourse.Right)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIceSpazer, ProjectileTrailCourse.DownRight)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIceSpazer, ProjectileTrailCourse.Down)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIceSpazer, ProjectileTrailCourse.DownLeft)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIceSpazer, ProjectileTrailCourse.Left)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIceSpazer, ProjectileTrailCourse.UpLeft)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIcePlasma, ProjectileTrailCourse.Up)] = 2,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIcePlasma, ProjectileTrailCourse.UpRight)] = 2,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIcePlasma, ProjectileTrailCourse.Right)] = 2,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIcePlasma, ProjectileTrailCourse.DownRight)] = 2,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIcePlasma, ProjectileTrailCourse.Down)] = 2,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIcePlasma, ProjectileTrailCourse.DownLeft)] = 2,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIcePlasma, ProjectileTrailCourse.Left)] = 2,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedIcePlasma, ProjectileTrailCourse.UpLeft)] = 2,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIcePlasma, ProjectileTrailCourse.Up)] = 9,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIcePlasma, ProjectileTrailCourse.UpRight)] = 9,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIcePlasma, ProjectileTrailCourse.Right)] = 9,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIcePlasma, ProjectileTrailCourse.DownRight)] = 9,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIcePlasma, ProjectileTrailCourse.Down)] = 9,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIcePlasma, ProjectileTrailCourse.DownLeft)] = 9,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIcePlasma, ProjectileTrailCourse.Left)] = 9,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.UnchargedWaveIcePlasma, ProjectileTrailCourse.UpLeft)] = 9,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedDefault, ProjectileTrailCourse.AnyDirection)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWave, ProjectileTrailCourse.Vertical)] = 16,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWave, ProjectileTrailCourse.Horizontal)] = 16,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWave, ProjectileTrailCourse.RisingDiagonal)] = 16,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWave, ProjectileTrailCourse.FallingDiagonal)] = 16,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIceSpazer, ProjectileTrailCourse.Up)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIceSpazer, ProjectileTrailCourse.UpRight)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIceSpazer, ProjectileTrailCourse.Right)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIceSpazer, ProjectileTrailCourse.DownRight)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIceSpazer, ProjectileTrailCourse.Down)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIceSpazer, ProjectileTrailCourse.DownLeft)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIceSpazer, ProjectileTrailCourse.Left)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIceSpazer, ProjectileTrailCourse.UpLeft)] = 10,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIceSpazer, ProjectileTrailCourse.Up)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIceSpazer, ProjectileTrailCourse.UpRight)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIceSpazer, ProjectileTrailCourse.Right)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIceSpazer, ProjectileTrailCourse.DownRight)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIceSpazer, ProjectileTrailCourse.Down)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIceSpazer, ProjectileTrailCourse.DownLeft)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIceSpazer, ProjectileTrailCourse.Left)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIceSpazer, ProjectileTrailCourse.UpLeft)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIcePlasma, ProjectileTrailCourse.Up)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIcePlasma, ProjectileTrailCourse.UpRight)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIcePlasma, ProjectileTrailCourse.Right)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIcePlasma, ProjectileTrailCourse.DownRight)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIcePlasma, ProjectileTrailCourse.Down)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIcePlasma, ProjectileTrailCourse.DownLeft)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIcePlasma, ProjectileTrailCourse.Left)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedIcePlasma, ProjectileTrailCourse.UpLeft)] = 8,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIcePlasma, ProjectileTrailCourse.Up)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIcePlasma, ProjectileTrailCourse.UpRight)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIcePlasma, ProjectileTrailCourse.Right)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIcePlasma, ProjectileTrailCourse.DownRight)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIcePlasma, ProjectileTrailCourse.Down)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIcePlasma, ProjectileTrailCourse.DownLeft)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIcePlasma, ProjectileTrailCourse.Left)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.ChargedWaveIcePlasma, ProjectileTrailCourse.UpLeft)] = 22,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.SpazerAttackTrailingPair, ProjectileTrailCourse.AnyDirection)] = 3,
            [new ProjectileTrailPlacementList(ProjectileTrailFamily.SpazerAttackTrailingPair, ProjectileTrailCourse.Down)] = 3,
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
