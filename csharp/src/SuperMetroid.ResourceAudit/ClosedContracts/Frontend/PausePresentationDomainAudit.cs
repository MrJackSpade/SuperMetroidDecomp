using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Preserves the reviewed correlated pause selectors; a short category cannot borrow another category's capacity.</summary>
internal static class PausePresentationDomainAudit
{
    /// <summary>Rejects constant pause selectors outside reviewed phase or anchor domains.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        string type = operation.TargetMethod.ContainingType.ToDisplayString(), method = operation.TargetMethod.Name;
        int? Constant(string name) => operation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == name)
            ?.Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;

        if (type == typeof(PauseSelectorPresentation).FullName)
        {
            if (method is "NormalizePhase" or "Duration" or "Draw" && Constant("phase") is < 0)
                return "Constant phase is negative; reviewed pause phases normalize only nonnegative indices.";
            if (method is "Anchor" or "Draw")
            {
                int? category = Constant("category"), item = Constant("item");
                if (!PauseSelectorAnchors.Anchors().Any(anchor =>
                    (!category.HasValue || category == (int)anchor.Category) && (!item.HasValue || item == anchor.Item)))
                    return "Constant category/item does not select any reviewed pause selector anchor.";
            }
        }
        return null;
    }
}
