using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Rooms;

/// <summary>
/// Keeps the taking of a room-object place in the room's place list (#1361, #1905): the spawn is the list's operation,
/// with its search order and full-room contract, so a room-object place may be claimed only by
/// <c>RoomObjectPlaceList</c>. An open-coded search for a free place must claim it and is reported here.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RoomObjectSlotAllocationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A room-object place claimed outside the place list's spawn.</summary>
    public const string OpenCodedClaimId = "SME6303";

    /// <summary>The namespace declaring the room-object place and its list.</summary>
    private const string RoomObjectNamespace = "SuperMetroid.Core.Rooms.RoomObjects";

    /// <summary>The room-object place type.</summary>
    private const string SlotType = "RoomObjectPlace";

    /// <summary>The place list, the only type allowed to claim a place.</summary>
    private const string TableType = "RoomObjectPlaceList";

    /// <summary>The place's claiming operation.</summary>
    private const string ClaimMethod = "Claim";

    /// <summary>Warns when a room-object place is claimed outside the place list.</summary>
    private static readonly DiagnosticDescriptor OpenCodedClaimRule = new(
        OpenCodedClaimId,
        "Room-object place claimed outside the place list",
        "{0} claims a room-object place directly; use RoomObjectPlaceList.Spawn",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The spawn is the place list's operation; searches for a free place are not open-coded.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [OpenCodedClaimRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    /// <summary>Reports a call to the place's claiming operation from any type but the place list.</summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        if (method.Name != ClaimMethod || !IsObjectType(method.ContainingType, SlotType))
            return;
        INamedTypeSymbol? caller = context.ContainingSymbol.ContainingType;
        if (IsObjectType(caller, TableType))
            return;
        context.ReportDiagnostic(Diagnostic.Create(OpenCodedClaimRule, invocation.Syntax.GetLocation(),
            caller?.Name ?? context.ContainingSymbol.Name));
    }

    /// <summary>Whether <paramref name="type"/> is the room-object type named <paramref name="name"/>.</summary>
    private static bool IsObjectType(INamedTypeSymbol? type, string name) =>
        type is not null && type.Name == name && type.ContainingNamespace.ToDisplayString() == RoomObjectNamespace;
}
