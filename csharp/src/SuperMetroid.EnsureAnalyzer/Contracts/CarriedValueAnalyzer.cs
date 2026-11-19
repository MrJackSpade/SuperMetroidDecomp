using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports a value carried across rooms or pool places read or left outside a compatibility service (SME6466, #2572).
/// What one room's main leaves the next (the room mains' one shared value, <c>RoomMainCarryover</c>) and what a room's
/// shot and enemy-projectile places leave the next room's (<c>RoomHandOver</c>) are working storage the original shared
/// under several meanings. Each meaning is a compatibility service named for the behaviour it carries (the shaft's turn
/// setting the hall's debris wait, the escape's carried quake, the elevatube's inherited fraction, the left beam's start
/// from its place's last shot, an enemy projectile's start from its place's leftovers), and only those services read or
/// leave the storage. The rule reports every call of, and every property, field or method group naming, a member of
/// either storage type outside a <c>[CompatibilityService]</c> type.
/// </summary>
/// <remarks>
/// Creating the storage and handing it on is not reported: the session's composition creates it and passes it to the
/// services. A use inside a <c>[CompatibilityService]</c> type, read on the use's type and every type containing it, is
/// that service carrying its behaviour. Only Core is read.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CarriedValueAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A value carried across rooms or pool places read or left outside a compatibility service.</summary>
    public const string CarriedValueId = "SME6466";

    /// <summary>The namespace of Core's compatibility module, which declares the carried storage.</summary>
    private const string CompatibilityNamespace = "SuperMetroid.Core.Compatibility";

    /// <summary>The storage types whose members only compatibility services use.</summary>
    private static readonly ImmutableHashSet<string> CarriedStorage = ["RoomMainCarryover", "RoomHandOver"];

    /// <summary>The attribute whose types may use the storage: a compatibility service.</summary>
    private const string CompatibilityServiceAttribute = "CompatibilityServiceAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        CarriedValueId,
        "Carried value used outside a compatibility service",
        "'{0}' reads or leaves a value carried across rooms or pool places outside a compatibility service; use the " +
        "compatibility service named for the behaviour the value carries",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A value one room's main leaves the next, or a room's shot and enemy-projectile places leave the next " +
            "room's, is read and left only by the compatibility service named for the behaviour it carries (#2572): the " +
            "members of RoomMainCarryover and RoomHandOver are used only by [CompatibilityService] types.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterOperationAction(Analyze, OperationKind.Invocation, OperationKind.MethodReference,
                OperationKind.PropertyReference, OperationKind.FieldReference);
        });
    }

    /// <summary>Reports a storage member used outside a compatibility service.</summary>
    private static void Analyze(OperationAnalysisContext context)
    {
        ISymbol? member = context.Operation switch
        {
            IInvocationOperation invocation => invocation.TargetMethod,
            IMethodReferenceOperation reference => reference.Method,
            IPropertyReferenceOperation property => property.Property,
            IFieldReferenceOperation field => field.Field,
            _ => null,
        };
        if (member?.ContainingType is not { } type || !IsCarriedStorage(type))
            return;
        for (INamedTypeSymbol? user = context.ContainingSymbol.ContainingType; user is not null; user = user.ContainingType)
        {
            if (user.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == CompatibilityServiceAttribute))
                return;
        }
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), $"{type.Name}.{member.Name}"));
    }

    /// <summary>Whether <paramref name="type"/> is one of the carried storage types.</summary>
    private static bool IsCarriedStorage(INamedTypeSymbol type) =>
        CarriedStorage.Contains(type.Name) && type.ContainingNamespace?.ToDisplayString() == CompatibilityNamespace;
}
