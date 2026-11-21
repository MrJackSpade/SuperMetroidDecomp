using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #2602: the game's code uses no reflection (owner, 2026-11-21: "except the analyzer, no reflection in the game specific
/// code"). Every project the analyzer runs on is covered (the game, its tools, test-support and every verification
/// project) except the code-analysis tooling that needs reflection by its nature: the analyzer and its code fixes, the
/// architecture and reachability audits. Nothing excuses a site.
/// </summary>
/// <remarks>
/// Reflection is any use of a <c>System.Reflection</c> type or member, or of an API whose signature carries one
/// (<c>Type.GetProperty</c>, <c>Delegate.Method</c>, <c>Assembly</c> loading and type scanning); any method of
/// <see cref="Type"/> and any of its properties that yields another type or member (<c>Type.GetType</c> by name,
/// <c>MakeGenericType</c>, <c>BaseType</c>); <c>Activator</c>; an uninitialized object; <c>Delegate.DynamicInvoke</c>;
/// <c>TypeDescriptor</c>; <c>dynamic</c>; an <c>[UnsafeAccessor]</c>; and System.Text.Json's reflection-based
/// serialization: a <c>JsonSerializer</c> call, or an <c>HttpClient</c> JSON call, given no <c>JsonTypeInfo</c> or
/// <c>JsonSerializerContext</c>, a generic <c>JsonValue.Create</c> given none, and the reflection resolver
/// <c>DefaultJsonTypeInfoResolver</c>. A type's name (<c>GetType().Name</c>), <c>typeof</c>, <c>nameof</c>,
/// <see cref="Enum"/>'s values and an applied attribute are not reflection.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GameReflectionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A use of reflection in the game's code.</summary>
    public const string ReflectionId = "SME6489";

    /// <summary>The assemblies of the code-analysis tooling, which needs reflection by its nature.</summary>
    private static readonly ImmutableHashSet<string> AnalysisTooling = ImmutableHashSet.Create(StringComparer.Ordinal,
        "SuperMetroid.EnsureAnalyzer", "SuperMetroid.EnsureAnalyzer.CodeFixes", "SuperMetroid.ArchitectureAudit",
        "SuperMetroid.ReachabilityAudit");

    /// <summary>The methods of <see cref="Type"/> that only compare or print it.</summary>
    private static readonly ImmutableHashSet<string> TypeIdentityMethods = ImmutableHashSet.Create(StringComparer.Ordinal,
        "Equals", "GetHashCode", "ToString", "op_Equality", "op_Inequality", "GetTypeFromHandle");

    private static readonly DiagnosticDescriptor Rule = new(
        ReflectionId,
        "Reflection in the game's code",
        "{0} uses reflection ({1}); name the member it reaches, generate the code that walks it, or serialize through a source-generated JsonSerializerContext",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The game's code reaches only what it names (#2602): no System.Reflection, no by-name or attribute-driven " +
            "discovery, no UnsafeAccessor, no dynamic, and System.Text.Json only through source-generated contexts. A check " +
            "needs a production member the game uses: the member is internal and the check calls it.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (AnalysisTooling.Contains(start.Compilation.AssemblyName ?? ""))
                return;
            start.RegisterOperationAction(AnalyzeMemberUse, OperationKind.Invocation, OperationKind.PropertyReference,
                OperationKind.FieldReference, OperationKind.MethodReference, OperationKind.EventReference, OperationKind.ObjectCreation);
            start.RegisterOperationAction(AnalyzeDynamic, OperationKind.DynamicInvocation, OperationKind.DynamicMemberReference,
                OperationKind.DynamicObjectCreation, OperationKind.DynamicIndexerAccess);
            start.RegisterSymbolAction(AnalyzeUnsafeAccessor, SymbolKind.Method);
        });
    }

    /// <summary>Reports a member use that reflects.</summary>
    private static void AnalyzeMemberUse(OperationAnalysisContext context)
    {
        IOperation operation = context.Operation;
        ISymbol? member = operation switch
        {
            IInvocationOperation invocation => invocation.TargetMethod,
            IMemberReferenceOperation reference => reference.Member,
            IObjectCreationOperation creation => creation.Constructor,
            _ => null,
        };
        if (member is null || IsNameOrAttribute(operation) || Reflects(member) is not { } reason)
            return;
        Report(context, operation, reason);
    }

    /// <summary>Reports a dynamic member access, which the runtime binder resolves by name.</summary>
    private static void AnalyzeDynamic(OperationAnalysisContext context) => Report(context, context.Operation, "dynamic binding by name");

    /// <summary>Reports an <c>[UnsafeAccessor]</c> declaration.</summary>
    private static void AnalyzeUnsafeAccessor(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (!method.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() ==
                "System.Runtime.CompilerServices.UnsafeAccessorAttribute"))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0],
            method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), "an UnsafeAccessor"));
    }

    private static void Report(OperationAnalysisContext context, IOperation operation, string reason) =>
        context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(),
            context.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), reason));

    /// <summary>Why a use of <paramref name="member"/> reflects, or null when it does not.</summary>
    private static string? Reflects(ISymbol member)
    {
        INamedTypeSymbol owner = member.ContainingType;
        string ownerName = owner.ToDisplayString();
        // A type's name is declared on MemberInfo; naming a type reaches nothing.
        if (member is IPropertySymbol { Name: "Name" } && ownerName == "System.Reflection.MemberInfo")
            return null;
        if (IsReflectionType(owner))
            return $"{owner.Name}.{member.Name}";
        if (Signature(member).FirstOrDefault(IsReflectionType) is { } carried)
            return $"{owner.Name}.{member.Name}, which carries {Element(carried).Name}";
        switch (ownerName)
        {
            case "System.Type":
                if (member is IMethodSymbol method && !TypeIdentityMethods.Contains(method.Name))
                    return $"Type.{member.Name}";
                if (member is IPropertySymbol property && YieldsTypeOrMember(property.Type))
                    return $"Type.{member.Name}";
                return null;
            case "System.Activator":
                return $"Activator.{member.Name}";
            case "System.Delegate" or "System.MulticastDelegate" when member.Name == "DynamicInvoke":
                return "Delegate.DynamicInvoke";
            case "System.Runtime.CompilerServices.RuntimeHelpers" when member.Name is "GetUninitializedObject" or "RunClassConstructor":
                return $"RuntimeHelpers.{member.Name}";
            case "System.Runtime.Serialization.FormatterServices":
            case "System.ComponentModel.TypeDescriptor":
            case "System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver":
                return $"{owner.Name}.{member.Name}";
            case "System.Text.Json.JsonSerializer":
            case "System.Net.Http.Json.HttpClientJsonExtensions":
            case "System.Net.Http.Json.HttpContentJsonExtensions":
            case "System.Net.Http.Json.JsonContent":
                return TakesGeneratedMetadata(member) ? null : $"{owner.Name}.{member.Name} without a source-generated JsonTypeInfo";
            case "System.Text.Json.Nodes.JsonValue" when member is IMethodSymbol { Name: "Create", IsGenericMethod: true }:
                return TakesGeneratedMetadata(member) ? null : "JsonValue.Create without a source-generated JsonTypeInfo";
            default:
                return null;
        }
    }

    /// <summary>Whether a serializer member is given its metadata by a source-generated context.</summary>
    private static bool TakesGeneratedMetadata(ISymbol member) => member is IMethodSymbol method &&
        method.Parameters.Any(parameter => parameter.Type.OriginalDefinition.ToDisplayString() is
            "System.Text.Json.Serialization.Metadata.JsonTypeInfo" or "System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>" or
            "System.Text.Json.Serialization.JsonSerializerContext");

    /// <summary>The types a member's signature carries: its type, return type and parameter types.</summary>
    private static IEnumerable<ITypeSymbol> Signature(ISymbol member) => member switch
    {
        IMethodSymbol method => [method.ReturnType, .. method.Parameters.Select(parameter => parameter.Type)],
        IPropertySymbol property => [property.Type, .. property.Parameters.Select(parameter => parameter.Type)],
        IFieldSymbol field => [field.Type],
        IEventSymbol @event => [@event.Type],
        _ => [],
    };

    /// <summary>Whether a property's value is a type or a member: what walks from one type to others.</summary>
    private static bool YieldsTypeOrMember(ITypeSymbol type) =>
        Element(type) is INamedTypeSymbol { Name: "Type" or "RuntimeTypeHandle", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } ||
        IsReflectionType(type);

    /// <summary>Whether <paramref name="type"/>, or the element type of the array it is, belongs to System.Reflection.</summary>
    private static bool IsReflectionType(ITypeSymbol type)
    {
        ITypeSymbol element = Element(type);
        for (INamespaceSymbol? space = element.ContainingNamespace; space is { IsGlobalNamespace: false }; space = space.ContainingNamespace)
        {
            if (space.Name == "Reflection" && space.ContainingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true })
                return true;
        }
        return element is INamedTypeSymbol { IsGenericType: true } generic && generic.TypeArguments.Any(IsReflectionType);
    }

    private static ITypeSymbol Element(ITypeSymbol type)
    {
        while (type is IArrayTypeSymbol array)
            type = array.ElementType;
        return type;
    }

    /// <summary>Whether the operation names a member without reaching it, in <c>nameof</c>, or applies an attribute.</summary>
    private static bool IsNameOrAttribute(IOperation operation)
    {
        for (IOperation? parent = operation.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent.Kind is OperationKind.NameOf or OperationKind.Attribute)
                return true;
        }
        return false;
    }
}
