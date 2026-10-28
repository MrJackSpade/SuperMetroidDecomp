using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Projectiles;

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
    /// The address of <paramref name="list"/>'s first placement, read through the selector row and direction entry of a
    /// family that names it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No selector row's family names <paramref name="list"/>.</exception>
    internal static SnesAddress Start(IImportCartridgeSource cartridge, ProjectileTrailPlacementList list)
    {
        foreach ((ProjectileTrailSelectorRow row, ProjectileTrailFamily? family) in ProjectileTrailPlacementDefinitions.Families)
        {
            if (family is not { } named)
                continue;
            int direction = Array.IndexOf(ProjectileTrailPlacementDefinitions.Lists[named], list);
            if (direction < 0)
                continue;
            SnesAddress selector = SelectorTable(row.Group).AddWithinBank(row.Beams.TableIndex * sizeof(ushort));
            SnesAddress entry = Pointer(cartridge, selector).AddWithinBank(direction * sizeof(ushort));
            return Pointer(cartridge, entry);
        }
        throw new ArgumentOutOfRangeException(nameof(list), list, "No trail selector row reaches this placement list.");
    }

    /// <summary>The selector table a projectile of <paramref name="group"/> reads its row from.</summary>
    private static SnesAddress SelectorTable(ProjectileTrailFamilyGroup group) => group switch
    {
        ProjectileTrailFamilyGroup.Uncharged => SamusSourceDefinitions.UnchargedTrailSelectors,
        ProjectileTrailFamilyGroup.Charged => SamusSourceDefinitions.ChargedTrailSelectors,
        ProjectileTrailFamilyGroup.SpecialBeamAttack => SamusSourceDefinitions.SpecialBeamAttackTrailSelectors,
        _ => throw new ArgumentOutOfRangeException(nameof(group), group, "Undefined trail selector table."),
    };

    /// <summary>The bank-local pointer stored at <paramref name="address"/>, in <paramref name="address"/>'s bank.</summary>
    private static SnesAddress Pointer(IImportCartridgeSource cartridge, SnesAddress address) =>
        new(address.Bank, RomDataReader.ReadWordFixedBank(cartridge, address));
}
