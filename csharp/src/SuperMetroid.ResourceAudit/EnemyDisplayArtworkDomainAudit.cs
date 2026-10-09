using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Checks sparse bank/pointer pairs rather than treating either dimension as contiguous.</summary>
internal static class EnemyDisplayArtworkDomainAudit
{
    /// <summary>Rejects sparse bank/pointer pairs not present in the reviewed enemy frame domains.</summary>
    /// <param name="operation">Semantic invocation of an enemy display provider.</param>
    /// <returns>A diagnostic for an unowned pair, or null when the pair is valid or unrelated.</returns>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        string type = operation.TargetMethod.ContainingType.ToDisplayString();
        if (operation.TargetMethod.Name != "TryGetDisplay" ||
            type != typeof(EnemySpritemapCatalog).FullName && type != typeof(EnemyExtendedFrameCatalog).FullName) return null;
        int? Constant(string name) => operation.Arguments.Single(argument => argument.Parameter?.Name == name)
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (Constant("bank") is not int bank || Constant("nativePointer") is not int pointer) return null;
        bool owned = type == typeof(EnemySpritemapCatalog).FullName
            ? EnemySpritemapDefinitions.Frames.ToArray().Any(frame => frame.Bank == bank && frame.Pointer == pointer)
            : EnemyExtendedFrameDefinitions.Frames.ToArray().Any(frame => frame.Bank == bank && frame.Pointer == pointer) ||
                CommonEnemyEmptyExtendedFrameDefinitions.HasFrame((byte)bank, (ushort)pointer);
        return owned ? null : $"Known display identity ${bank:X2}:{pointer:X4} is outside the reviewed installed frame domain.";
    }
}
