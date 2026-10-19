using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Keeps rooms, room states and doors identified by their definitions (#1333). Runtime code names a room by its
/// <c>RoomHeaderDefinition</c>, a state by its <c>CartridgeRoomState</c> and a door by its <c>CartridgeDoorHeader</c>;
/// the native bank-$8F and bank-$83 pointers are the import pipeline's knowledge. A field, property, parameter or
/// method named as a room, room-state or door pointer with a number type is reported.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RoomPointerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A member or parameter carrying a room, room-state or door pointer as a number.</summary>
    public const string RoomPointerId = "SME6412";

    /// <summary>Names of room, room-state and door pointers.</summary>
    private static readonly Regex RoomPointerName = new(@"(room|door)\w*pointer", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly DiagnosticDescriptor RoomPointerRule = new(
        RoomPointerId,
        "Room, room-state or door pointer held as a number",
        "'{0}' carries a room, room-state or door pointer as a number; identify it by its definition (RoomHeaderDefinition, CartridgeRoomState or CartridgeDoorHeader)",
        "Architecture",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Rooms, room states and doors are identified by their definitions; native pointers belong to the import pipeline (#1333).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RoomPointerRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    /// <summary>Reports a field named as a room pointer with a number type.</summary>
    private static void AnalyzeField(SymbolAnalysisContext context)
    {
        var field = (IFieldSymbol)context.Symbol;
        if (!field.IsImplicitlyDeclared)
            Report(context, field, field.Name, field.Type);
    }

    /// <summary>Reports a property named as a room pointer with a number type.</summary>
    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var property = (IPropertySymbol)context.Symbol;
        if (!property.IsImplicitlyDeclared)
            Report(context, property, property.Name, property.Type);
    }

    /// <summary>Reports a method returning a room pointer, and each parameter named as one, with a number type.</summary>
    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (method.IsImplicitlyDeclared || method.AssociatedSymbol is not null)
            return;
        if (method.MethodKind == MethodKind.Ordinary)
            Report(context, method, method.Name, method.ReturnType);
        foreach (IParameterSymbol parameter in method.Parameters)
            Report(context, parameter, parameter.Name, parameter.Type);
    }

    /// <summary>Reports <paramref name="symbol"/> when its name is a room pointer's and its type a number.</summary>
    private static void Report(SymbolAnalysisContext context, ISymbol symbol, string name, ITypeSymbol type)
    {
        if (!RoomPointerName.IsMatch(name) || !IsNumber(type) || symbol.Locations.FirstOrDefault(l => l.IsInSource) is not { } location)
            return;
        context.ReportDiagnostic(Diagnostic.Create(RoomPointerRule, location, name));
    }

    /// <summary>Whether <paramref name="type"/> is an integral number type or a nullable one.</summary>
    private static bool IsNumber(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            type = nullable.TypeArguments[0];
        return type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16
            or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64
            or SpecialType.System_UInt64;
    }
}
