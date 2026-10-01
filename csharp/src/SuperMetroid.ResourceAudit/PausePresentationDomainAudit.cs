using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Preserves the reviewed correlated pause selectors; a short category cannot borrow another category's capacity.</summary>
internal static class PausePresentationDomainAudit
{
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
                if (!PauseSelectorDefinitions.Anchors().Any(anchor =>
                    (!category.HasValue || category == anchor.Category) && (!item.HasValue || item == anchor.Item)))
                    return "Constant category/item does not select any reviewed pause selector anchor.";
            }
        }
        if (type == typeof(PauseEquipmentLabelPresentation).FullName && method == "ApplyLabel")
        {
            int? category = Constant("category"), item = Constant("item"), words = Constant("wordCount");
            var candidates = PauseEquipmentLabelDefinitions.Keys.SelectMany((keys, categoryIndex) =>
                keys.Select((key, itemIndex) => (Category: categoryIndex, Item: itemIndex, Key: key)));
            if (!candidates.Any(label => (!category.HasValue || category == label.Category) &&
                (!item.HasValue || item == label.Item) && (!words.HasValue ||
                    words >= 0 && words <= PauseEquipmentLabelDefinitions.EquipmentWords &&
                    (words <= PauseEquipmentLabelDefinitions.WordCount(label.Category) ||
                        label.Key == PauseEquipmentLabelDefinitions.PlasmaKey))))
                return "Constant category/item/wordCount has no reviewed pause label; only Plasma owns the five-to-nine-word native overrun.";
        }
        return null;
    }
}
