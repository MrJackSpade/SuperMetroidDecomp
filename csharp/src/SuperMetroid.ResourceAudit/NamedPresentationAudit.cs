using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

internal enum NamedSelectionResult { Resolved, Missing, Unresolved }
internal sealed record NamedPresentationSelection(string Type, string Method, string Parameter,
    string Domain, string[] Names);

/// <summary>Finite named sets already enforced by the reviewed production loaders.</summary>
internal static class NamedPresentationSelectionDefinitions
{
    internal static readonly NamedPresentationSelection[] All =
    [
        new(nameof(FileSelectPresentation), "DynamicAnchor", "name", "file-select-dynamic-anchor",
            FileSelectPresentationDefinitions.DynamicAnchorNames.ToArray()),
        new(nameof(FileSelectPresentation), "ApplyPatch", "name", "file-select-patch",
            FileSelectPresentationDefinitions.PatchNames.ToArray()),
        new(nameof(GameOptionsPresentation), "CreatePage", "name", "options-page",
            GameOptionsPresentationDefinitions.PageNames.ToArray()),
        new(nameof(GameOptionsPresentation), "ApplySpecialToggle", "name", "options-toggle",
            GameOptionsPresentationDefinitions.SpecialToggleNames.ToArray()),
        new(nameof(GameOptionsPresentation), "CursorPosition", "page", "options-menu",
            GameOptionsPresentationDefinitions.MenuPageNames.ToArray()),
        new(nameof(GameOptionsPresentation), "DrawHeading", "page", "options-menu",
            GameOptionsPresentationDefinitions.MenuPageNames.ToArray()),
        new(nameof(PauseReserveUiPresentation), "ApplyLabel", "name", "reserve-label",
            ["Mode", "ReserveTank", "Manual", "Auto"]),
        new(nameof(MapScreenPresentation), "LoadTo", "page", "map-screen-page",
            MapScreenDefinitions.Pages().Select(page => page.Id).ToArray()),
    ];
}

/// <summary>
/// Resolves only compiler constants and conditional constant unions. It does not
/// guess values of mutable fields, function results, parameters or unknown strings.
/// </summary>
internal static class NamedPresentationAudit
{
    internal static void Install(ResourceIndex exports)
    {
        foreach (NamedPresentationSelection selection in NamedPresentationSelectionDefinitions.All)
        foreach (string name in selection.Names) exports.Add(selection.Domain, name);
    }

    internal static NamedSelectionResult? Inspect(IInvocationOperation operation, string owner,
        string location, ResourceIndex exports, AuditReport report)
    {
        NamedPresentationSelection? selection = NamedPresentationSelectionDefinitions.All.FirstOrDefault(item =>
            item.Type == operation.TargetMethod.ContainingType.Name && item.Method == operation.TargetMethod.Name);
        if (selection is null) return null;
        IArgumentOperation argument = operation.Arguments.Single(arg => arg.Parameter?.Name == selection.Parameter);
        string[]? values = FiniteConstants(argument.Value);
        if (values is null || values.Length == 0) return NamedSelectionResult.Unresolved;
        foreach (string name in values.Order(StringComparer.Ordinal))
            report.Require(selection.Domain, owner, name, location, exports);
        return values.All(name => exports.Contains(selection.Domain, name))
            ? NamedSelectionResult.Resolved : NamedSelectionResult.Missing;
    }

    private static string[]? FiniteConstants(IOperation operation)
    {
        if (operation.ConstantValue is { HasValue: true, Value: string value }) return [value];
        if (operation is IConversionOperation conversion) return FiniteConstants(conversion.Operand);
        if (operation is IInvocationOperation factory &&
            factory.TargetMethod.ContainingType.ToDisplayString() == typeof(MapScreenDefinitions).FullName &&
            factory.TargetMethod.Name is "WorldBackground" or "RoomFrame")
        {
            IOperation area = factory.Arguments.Single(argument => argument.Parameter?.Name == "area").Value;
            IEnumerable<int> indices = Enumerable.Range(0, MapScreenDefinitions.ZebesAreas);
            if (area.ConstantValue is { HasValue: true, Value: not null } known)
            {
                int index = Convert.ToInt32(known.Value);
                if ((uint)index >= MapScreenDefinitions.ZebesAreas) return null;
                indices = [index];
            }
            // Only these exact, source-guarded factories have bounded outputs.
            // Unknown input may throw, but cannot produce an uninstalled page.
            return indices.Select(index => factory.TargetMethod.Name == "WorldBackground"
                ? MapScreenDefinitions.WorldBackground((AreaId)index)
                : MapScreenDefinitions.RoomFrame((AreaId)index)).ToArray();
        }
        if (operation is not IConditionalOperation { WhenFalse: not null } conditional) return null;
        if (conditional.Condition.ConstantValue is { HasValue: true, Value: bool selected })
            return FiniteConstants(selected ? conditional.WhenTrue : conditional.WhenFalse);
        string[]? yes = FiniteConstants(conditional.WhenTrue), no = FiniteConstants(conditional.WhenFalse);
        return yes is null || no is null ? null : yes.Concat(no).Distinct(StringComparer.Ordinal).ToArray();
    }
}
