using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Accounts for the identified closed-provider boundaries using reviewed loader
/// invariants, not possible gameplay paths. Unknown operations, changed source,
/// invalid constant indices and unresolved string names remain failures.
/// </summary>
internal sealed class ClosedPresentationAudit
{
    private readonly Dictionary<string, (ClosedPresentationContract Contract, bool Valid)> contracts = [];
    private int references;

    internal ClosedPresentationAudit(Compilation compilation, ResourceIndex exports)
    {
        var sources = compilation.SyntaxTrees.ToDictionary(tree => tree.FilePath, tree => tree.GetText().ToString(),
            StringComparer.Ordinal);
        foreach (ClosedPresentationContract contract in ClosedPresentationContractDefinitions.All)
        {
            bool valid = contract.Sources.All(source => sources.TryGetValue(source.Path, out string? text) &&
                MatchesReviewedSource(text, source));
            // This existing definition uses a readonly array reference, not an
            // immutable array. Its reviewed reads are wholly inside these files;
            // a new external use could mutate its keys and must revoke the proof.
            if (valid && contract.Type == typeof(GameplayHudPresentation).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "IconNames")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IFieldSymbol field &&
                            field.ContainingType.ToDisplayString() == typeof(GameplayHudDefinitions).FullName));
            contracts.Add(contract.Type, (contract, valid));
            if (valid)
                foreach (string method in contract.Methods)
                    exports.Add("closed-presentation-contract", contract.Type + "." + method);
        }
        NamedPresentationAudit.Install(exports);
    }

    internal static bool MatchesReviewedSource(string text, ReviewedSource source) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal))))
            == source.Sha256;

    internal bool TryInspect(InvocationExpressionSyntax call, SemanticModel semantic, IMethodSymbol method,
        ResourceIndex exports, AuditReport report, string location, string arguments)
    {
        if (!contracts.TryGetValue(method.ContainingType.ToDisplayString(), out var reviewed) ||
            !reviewed.Contract.Methods.Contains(method.Name, StringComparer.Ordinal)) return false;
        ClosedPresentationContract contract = reviewed.Contract;
        string owner = method.ContainingType.Name + "." + method.Name;
        void Gap(string reason)
        {
            report.Gap(method.ContainingType.Name, owner, location, reason);
            report.Consumers.Add(new(method.ContainingType.Name, owner, location, arguments, "unresolved"));
        }
        if (!reviewed.Valid)
        {
            Gap($"Reviewed provider proof {contract.Rule} is stale or its source is unavailable; re-inspect the loader and selector contract.");
            return true;
        }
        if (semantic.GetOperation(call) is not IInvocationOperation operation)
        {
            Gap("Closed-provider operation could not be bound.");
            return true;
        }
        string? invalid = InvalidConstantIndex(operation);
        if (invalid is not null)
        {
            Gap(invalid);
            return true;
        }
        NamedSelectionResult? named = NamedPresentationAudit.Inspect(operation, owner, location, exports, report);
        if (named == NamedSelectionResult.Unresolved)
        {
            Gap("Named selection has no compiler-resolved finite constant set; complete array coverage does not prove arbitrary strings.");
            return true;
        }
        if (named == NamedSelectionResult.Missing)
        {
            report.Consumers.Add(new(method.ContainingType.Name, owner, location, arguments, "missing"));
            return true;
        }
        report.Require("closed-presentation-contract", owner, contract.Type + "." + method.Name, location, exports);
        references++;
        report.Consumers.Add(new(method.ContainingType.Name, owner, location, arguments, "closed-provider"));
        report.Classifications.Add(new(contract.Rule, owner, location, contract.Reason,
            contract.Sources.Select(source => source.Path + "#SHA256=" + source.Sha256).ToArray()));
        return true;
    }

    private static string? InvalidConstantIndex(IInvocationOperation operation)
    {
        foreach (IArgumentOperation argument in operation.Arguments)
        {
            var owner = (Type: operation.TargetMethod.ContainingType.Name,
                Method: operation.TargetMethod.Name, Parameter: argument.Parameter!.Name);
            (int Min, int Max)? range = owner switch
            {
                ("GameplayHudPresentation", "TryApplyIcon", "itemIndex") => (0, 4),
                ("GameplayHudPresentation", "ApplyAmmo", "itemIndex") => (0, 2),
                ("GameplayHudPresentation", "MinimapCellIndex", "outputX") => (0, 4),
                ("GameplayHudPresentation", "MinimapCellIndex", "outputY") => (0, 2),
                ("FileSelectPresentation", "WriteDigit", "digit") => (0, 9),
                ("FileSelectPresentation", "Slot" or "WriteSlotLetter" or "DrawHelmet", "slot") => (0, 2),
                ("FileSelectPresentation", "DrawCursor", "frame") => (0, 3),
                ("FileSelectPresentation", "DrawHelmet", "frame") => (0, 7),
                ("FileSelectPresentation", "CursorPosition", "selected") => (0, 5),
                ("MotherBrainRoomColorPresentation", "ApplyRecoveryLights", "frame") => (0, 6),
                ("GameOptionsPresentation", "ApplyControllerLabel", "action" or "button") => (0, 6),
                ("GameOptionsPresentation", "DrawCursor", "frame") => (0, 3),
                ("GameOverPresentation", "DrawBaby", "frame") => (0, 2),
                ("GameOverPresentation", "DrawCursor", "frame") => (0, 3),
                ("GameOverPresentation", "ApplyBabyPalette", "palette") => (0, 3),
                ("PauseReserveUiPresentation", "ApplyDigit", "position") => (0, 2),
                ("PauseReserveUiPresentation", "ApplyDigit", "value") => (0, 9),
                _ => null,
            };
            if (range is null || argument.Value.ConstantValue is not { HasValue: true, Value: not null } value) continue;
            int number = Convert.ToInt32(value.Value);
            if (number < range.Value.Min || number > range.Value.Max)
                return $"Constant {owner.Parameter}={number} is outside the reviewed valid domain {range.Value.Min}..{range.Value.Max}.";
        }
        if (operation.TargetMethod.ContainingType.Name == "MotherBrainRoomColorPresentation" &&
            operation.TargetMethod.Name == "ApplyFlash")
        {
            var argument = operation.Arguments.Single(arg => arg.Parameter?.Name == "timedEntryPointer");
            if (argument.Value.ConstantValue is { HasValue: true, Value: not null } value)
            {
                int offset = Convert.ToInt32(value.Value) - SuperMetroid.Core.Game.MotherBrainRoomPaletteProgramDefinitions.FlashStart;
                int stride = SuperMetroid.Core.Game.MotherBrainRoomColorRomData.TimedEntryByteCount;
                if (offset < 0 || offset % stride != 0 ||
                    offset / stride >= SuperMetroid.Core.Game.MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
                    return "Constant flash entry is not one of the reviewed aligned installed rows.";
            }
        }
        return null;
    }

    internal void Complete(ResourceIndex exports, AuditReport report) => report.Coverage.Add(new(
        "closed-presentation-contract", references, exports.Count("closed-presentation-contract")));
}
