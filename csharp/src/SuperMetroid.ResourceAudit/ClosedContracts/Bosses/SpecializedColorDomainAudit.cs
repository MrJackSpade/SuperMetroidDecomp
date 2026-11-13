using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Relational constraints where selecting a known stream narrows its valid array index.</summary>
internal static class SpecializedColorDomainAudit
{
    /// <summary>Checks literal indices against the selected color stream's shape after its enum domain has been validated separately.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.Name != "Resolve") return null;
        int? Constant(string name) => operation.Arguments.SingleOrDefault(argument => argument.Parameter?.Name == name)
            ?.Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        string? Outside(string parameter, int count, string selected) => Constant(parameter) is int index && (uint)index >= count
            ? $"Constant {parameter}={index} is outside the selected {selected} color domain (0..{count - 1})." : null;
        string type = operation.TargetMethod.ContainingType.ToDisplayString();
        if (type == typeof(InstalledPowerBombGlowColors).FullName && Constant("sequence") is int sequence)
        {
            var kind = (PowerBombGlowSequence)sequence;
            // Invalid enum members are reported by the independent domain check;
            // never let Count itself throw while auditing an invalid consumer.
            return Enum.IsDefined(kind) ? Outside("index", PowerBombGlowColorFormat.Count(kind), kind.ToString()) : null;
        }
        if (type == typeof(EnemyAuxiliaryColorCatalog).FullName && Constant("palette") is int palette)
        {
            var selected = EnemyAuxiliaryColorDefinitions.All.Where(row => (int)row.Id == palette).ToArray();
            return selected.Length != 1 ? null : Outside("frame", selected[0].FrameCount, selected[0].Id.ToString())
                ?? Outside("color", selected[0].ColorCount, selected[0].Id.ToString());
        }
        if (type == typeof(KraidColorCatalog).FullName && Constant("source") is int source)
        {
            var selected = (KraidPaletteSource)source;
            return Enum.IsDefined(selected) ? Outside("index", KraidPalettes.ColorCount(selected), selected.ToString()) : null;
        }
        if (type == typeof(DachoraColorCatalog).FullName && Constant("phase") is int phase)
        {
            var selected = (DachoraColourPhase)phase;
            if (!Enum.IsDefined(selected)) return null;
            return Outside("frame", selected == DachoraColourPhase.Default ? 1 : DachoraColorDefinitions.AnimatedFrameCount,
                selected.ToString());
        }
        return null;
    }
}
