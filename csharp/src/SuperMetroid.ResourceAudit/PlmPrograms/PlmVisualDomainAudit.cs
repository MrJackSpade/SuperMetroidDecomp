using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.ResourceAudit.PlmPrograms;

/// <summary>Checks known tuple components without pretending unknown caller values have been proven safe.</summary>
internal static class PlmVisualDomainAudit
{
    /// <summary>Reports a named draw, run, or word index outside the selected compiled PLM visual shape; computed
    /// draws and dynamic indexes remain unknown.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.Name != "GetWord") return null;
        RoomPlmShotBlockDrawDefinitions.DrawList[]? frames =
            PlmVisualDomainDefinitions.Get(operation.TargetMethod.ContainingType.ToDisplayString());
        if (frames is null) return null;
        int? Constant(string name) => operation.Arguments.SingleOrDefault(arg => arg.Parameter?.Name == name)
            ?.Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (NamedDraw(operation.Arguments.SingleOrDefault(arg => arg.Parameter?.Name == "draw")?.Value) is { } draw)
        {
            frames = frames.Where(frame => frame.Draw == draw).ToArray();
            if (frames.Length == 0) return $"Draw {draw} is not owned by this reviewed PLM artwork domain.";
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
        if (word is int wordIndex && !frames.Any(frame => frame.Runs.Span
            .WhereToArray((_, index) => run is null || index == run.Value)
            .Any(row => (uint)wordIndex < row.Count)))
            return $"Constant word/block index={wordIndex} is outside the selected compiled PLM run shape.";
        return null;
    }

    /// <summary>
    /// The draw a call names statically: a static draw property, or a static draw factory called with constants, both
    /// evaluated against Core. Null when the caller computes the draw, which leaves every draw of the domain possible.
    /// </summary>
    private static PlmDraw? NamedDraw(IOperation? argument) => argument switch
    {
        IConversionOperation conversion => NamedDraw(conversion.Operand),
        IPropertyReferenceOperation { Property.IsStatic: true } property =>
            CoreType(property.Property.ContainingType).GetProperty(property.Property.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(null) as PlmDraw,
        IInvocationOperation { TargetMethod.IsStatic: true } invocation
            when invocation.Arguments.All(arg => arg.Value.ConstantValue.HasValue) => Invoke(invocation) as PlmDraw,
        _ => null,
    };

    /// <summary>Invokes a static Core factory with its constant arguments.</summary>
    /// <exception cref="InvalidOperationException">The factory cannot be found in Core.</exception>
    private static object? Invoke(IInvocationOperation invocation)
    {
        IMethodSymbol target = invocation.TargetMethod;
        Type owner = CoreType(target.ContainingType);
        MethodInfo method = owner.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == target.Name && candidate.GetParameters().Length == target.Parameters.Length &&
                candidate.IsGenericMethodDefinition == target.IsGenericMethod);
        if (method.IsGenericMethodDefinition)
            method = method.MakeGenericMethod([.. target.TypeArguments.Select(CoreType)]);
        object?[] values = [.. invocation.Arguments.Select(arg =>
        {
            Type parameter = method.GetParameters()[arg.Parameter!.Ordinal].ParameterType;
            object constant = arg.Value.ConstantValue.Value!;
            return parameter.IsEnum ? Enum.ToObject(parameter, constant) : Convert.ChangeType(constant, parameter, null);
        })];
        return method.Invoke(null, values);
    }

    /// <summary>The Core type a symbol names, nested types included.</summary>
    /// <exception cref="InvalidOperationException">Core declares no such type.</exception>
    private static Type CoreType(ITypeSymbol symbol)
    {
        string name = symbol.Name;
        for (INamedTypeSymbol? outer = symbol.ContainingType; outer is not null; outer = outer.ContainingType)
            name = outer.Name + "+" + name;
        string full = symbol.ContainingNamespace.IsGlobalNamespace ? name : symbol.ContainingNamespace.ToDisplayString() + "." + name;
        return typeof(PlmDraw).Assembly.GetType(full)
            ?? throw new InvalidOperationException($"Core declares no type {full}.");
    }
}
