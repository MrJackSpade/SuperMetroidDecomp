using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Rooms;

/// <summary>
/// Keeps PLM slot allocation in the slot table (#1361): the cartridge's three spawn routines are the slot table's named
/// operations, each with its search order and full-table contract, so a PLM slot may be claimed only by
/// <c>PlmSlotTable</c>. An open-coded free-slot search must claim its slot and is reported here.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PlmSlotAllocationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A PLM slot claimed outside the slot table's spawn operations.</summary>
    public const string OpenCodedClaimId = "SME6303";

    /// <summary>The namespace declaring the PLM slot and its table.</summary>
    private const string PlmNamespace = "SuperMetroid.Core.Rooms.Plms";

    /// <summary>The PLM slot type.</summary>
    private const string SlotType = "PlmSlot";

    /// <summary>The slot table, the only type allowed to claim a slot.</summary>
    private const string TableType = "PlmSlotTable";

    /// <summary>The slot's claiming operation.</summary>
    private const string ClaimMethod = "Claim";

    /// <summary>Warns when a PLM slot is claimed outside the slot table.</summary>
    private static readonly DiagnosticDescriptor OpenCodedClaimRule = new(
        OpenCodedClaimId,
        "PLM slot claimed outside the slot table",
        "{0} claims a PLM slot directly; use PlmSlotTable.SpawnHardcoded, SpawnRoomPlm or SpawnAtCurrentBlock",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The cartridge's spawn routines are the slot table's named operations; free-slot searches are not open-coded.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [OpenCodedClaimRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    /// <summary>Reports a call to the slot's claiming operation from any type but the slot table.</summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        if (method.Name != ClaimMethod || !IsPlmType(method.ContainingType, SlotType))
            return;
        INamedTypeSymbol? caller = context.ContainingSymbol.ContainingType;
        if (IsPlmType(caller, TableType))
            return;
        context.ReportDiagnostic(Diagnostic.Create(OpenCodedClaimRule, invocation.Syntax.GetLocation(),
            caller?.Name ?? context.ContainingSymbol.Name));
    }

    /// <summary>Whether <paramref name="type"/> is the PLM type named <paramref name="name"/>.</summary>
    private static bool IsPlmType(INamedTypeSymbol? type, string name) =>
        type is not null && type.Name == name && type.ContainingNamespace.ToDisplayString() == PlmNamespace;
}
