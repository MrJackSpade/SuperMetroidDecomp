using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.CatalogInventories;

/// <summary>
/// Cartridge-backed development inventory, never a production ROM fallback.
/// Flags plausible five-byte OAM records, not a rendering proof: an extended
/// or BG2 command payload may coincidentally pass the shallow count check.
/// Every family still needs a consumer and exact OAM parity test before extraction.
/// </summary>
internal sealed record EnemyVisualSelectorInventory(Dictionary<int, ushort> Keyed, List<string> Unresolved)
{
    /// <summary>
    /// Scans every Core instruction-program catalog's presentation operands against the cartridge:
    /// each bank-resolved selector address with its native target, and the catalogs skipped because
    /// their bank cannot be resolved by the standard probe.
    /// </summary>
    /// <param name="rom">The retail cartridge whose selector words are read.</param>
    /// <param name="owners">When given, receives the owning catalog's name for each selector address.</param>
    internal static EnemyVisualSelectorInventory Collect(ISnesAddressSpace rom, Dictionary<int, string>? owners = null)
    {
        var keyed = new Dictionary<int, ushort>();
        var unresolved = new List<string>();

        // One-frame programs declare a single operand instead of an indexed list.
        // Ignoring that shape omitted Kzan's live operand (#1166).
        foreach (var catalog in InstructionProgramCatalog.All()
                     .Where(catalog => catalog.PresentationOperands is not null || catalog.SinglePresentationOperand is not null))
        {
            string name = catalog.Type.Name;
            if (catalog.MechanicsWords is null || (catalog.CompiledMechanicsByteProbe is null && catalog.DeclaredBank is null))
            {
                unresolved.Add($"{name}: no standard mechanics-word/bank probe");
                continue;
            }
            ushort firstAddress = catalog.MechanicsWords[0].Address;
            // A declared bank also covers catalogs with sparse mechanics words
            // but no byte-level ownership probe, such as the cutscene Baby.
            int? declaredBank = catalog.DeclaredBank;
            if (declaredBank is > byte.MaxValue) declaredBank >>= 16;
            byte[] banks = declaredBank.HasValue
                ? [checked((byte)declaredBank.Value)]
                : Enumerable.Range(0x80, 0x60)
                    .Where(bank => (catalog.CompiledMechanicsByteProbe ?? throw new InvalidOperationException("Verification requires catalog.CompiledMechanicsByteProbe."))(((int)new SnesAddress(checked((byte)bank), firstAddress))))
                    .Select(bank => (byte)bank).ToArray();
            if (banks.Length != 1)
            {
                unresolved.Add($"{name}: {banks.Length} matching banks");
                continue;
            }
            IReadOnlyList<ushort> presentation = catalog.PresentationOperands ?? [(catalog.SinglePresentationOperand ?? throw new InvalidOperationException("Verification requires catalog.SinglePresentationOperand."))];
            foreach (ushort address in presentation)
            {
                int source = ((int)new SnesAddress(banks[0], address));
                ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(source);
                if (keyed.TryGetValue(source, out ushort previous) &&
                    previous != pointer)
                    throw new InvalidDataException(
                        $"Conflicting visual selector ${source:X6}.");
                keyed[source] = pointer;
                owners?[source] = name;
            }
        }
        // Typed projectile programs (#641) key their installed frames by the bank-$86 operand address that drew
        // them; those keys still select the native spritemap through the shared selector table.
        foreach ((string owner, IEnumerable<ushort> keys) in new (string, IEnumerable<ushort>)[]
                 {
                     (nameof(CrocomireProjectileVisualDefinitions), CrocomireProjectileVisualDefinitions.ProjectileFrames.Select((frame => EnemyProjectileFrames.ProgramFrameKey(CrocomireProjectileVisualDefinitions.ShownProjectileFrames[frame])))),
                     (nameof(DraygonProjectileVisualDefinitions), DraygonProjectileVisualDefinitions.ProjectileFrames.Select((frame => EnemyProjectileFrames.ProgramFrameKey(DraygonProjectileVisualDefinitions.ShownProjectileFrames[frame])))),
                     (nameof(KraidRockVisualDefinitions), KraidRockVisualDefinitions.ProjectileFrames.Select((frame => EnemyProjectileFrames.ProgramFrameKey(KraidRockVisualDefinitions.ShownProjectileFrames[frame])))),
                 })
        {
            foreach (ushort key in keys)
            {
                int source = EnemyProjectileCodePointers.BankBase | key;
                ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(source);
                if (keyed.TryGetValue(source, out ushort previous) && previous != pointer)
                    throw new InvalidDataException($"Conflicting visual selector ${source:X6}.");
                keyed[source] = pointer;
                owners?[source] = owner;
            }
        }
        return new(keyed, unresolved);
    }
}
