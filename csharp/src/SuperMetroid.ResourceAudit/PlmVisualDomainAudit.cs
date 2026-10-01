using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>Checks known tuple components without pretending unknown caller values have been proven safe.</summary>
internal static class PlmVisualDomainAudit
{
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.Name != "GetWord") return null;
        RoomPlmShotBlockDrawDefinitions.DrawList[]? frames =
            PlmVisualDomainDefinitions.Get(operation.TargetMethod.ContainingType.ToDisplayString());
        if (frames is null) return null;
        int? Constant(string name) => operation.Arguments.SingleOrDefault(arg => arg.Parameter?.Name == name)
            ?.Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if ((Constant("drawPointer") ?? Constant("pointer")) is int pointer)
        {
            frames = frames.Where(frame => frame.Pointer == pointer).ToArray();
            if (frames.Length == 0) return $"Constant drawPointer=${pointer:X4} is not owned by this reviewed PLM artwork domain.";
        }
        // A flat/single-word method has no run selector: its contract is run
        // zero. A present but dynamic run parameter must remain unknown.
        int? run = operation.TargetMethod.Parameters.Any(parameter => parameter.Name == "runIndex")
            ? Constant("runIndex") : 0;
        if (run is int runIndex)
        {
            frames = frames.Where(frame => (uint)runIndex < frame.Runs.Length).ToArray();
            if (frames.Length == 0) return $"Constant runIndex={runIndex} is outside the selected compiled PLM draw shape.";
        }
        int? word = Constant("wordIndex") ?? Constant("blockIndex");
        if (word is int wordIndex && !frames.Any(frame => frame.Runs.ToArray()
            .Where((_, index) => run is null || index == run.Value)
            .Any(row => (uint)wordIndex < row.LevelWords.Length)))
            return $"Constant word/block index={wordIndex} is outside the selected compiled PLM run shape.";
        return null;
    }
}
