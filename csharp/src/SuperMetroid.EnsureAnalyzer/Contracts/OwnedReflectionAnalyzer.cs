using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #622: code reaches the project's own state through typed members, never through reflection. A test that needs a
/// hidden member widens it to <c>internal</c> (the verification assemblies see internals), injects it, or observes
/// it through public behavior. Reflection stays only where metadata or serialization is itself the subject, inside
/// a declaration marked <c>[ReflectionAudit("reason")]</c>.
/// </summary>
/// <remarks>
/// A type is owned when this compilation declares it or its assembly name starts with <c>SuperMetroid</c>. A
/// lookup is reported when its receiver is <c>typeof(owned)</c> or <c>owned.GetType()</c>, or when it asks for
/// non-public members of any type, since a receiver held in a <see cref="Type"/> variable may be owned.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OwnedReflectionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A reflective member lookup, constructor bypass or string type lookup that reaches owned or non-public state.</summary>
    public const string ReflectionId = "SME6373";

    /// <summary>An <c>[UnsafeAccessor]</c> declaration that reaches into an owned type.</summary>
    public const string UnsafeAccessorId = "SME6371";

    /// <summary>A <c>[ReflectionAudit]</c> exemption that does not say why reflection is the subject.</summary>
    public const string UnexplainedAuditId = "SME6372";

    /// <summary>The exemption attribute, matched by name so each assembly may declare its own.</summary>
    public const string AuditAttributeName = "ReflectionAuditAttribute";

    private const string OwnedAssemblyPrefix = "SuperMetroid";

    private static readonly ImmutableHashSet<string> TypeLookups = ImmutableHashSet.Create(StringComparer.Ordinal,
        "GetField", "GetFields", "GetProperty", "GetProperties", "GetMethod", "GetMethods", "GetConstructor",
        "GetConstructors", "GetNestedType", "GetNestedTypes", "GetMember", "GetMembers", "GetEvent", "GetEvents",
        "GetDefaultMembers", "FindMembers", "InvokeMember", "GetDeclaredField", "GetDeclaredProperty",
        "GetDeclaredMethod", "GetDeclaredMethods", "GetDeclaredNestedType", "GetDeclaredEvent",
        "GetRuntimeField", "GetRuntimeFields", "GetRuntimeProperty", "GetRuntimeProperties", "GetRuntimeMethod",
        "GetRuntimeMethods", "GetRuntimeEvent", "GetRuntimeEvents");

    private static readonly ImmutableHashSet<string> DeclaredMemberProperties = ImmutableHashSet.Create(StringComparer.Ordinal,
        "DeclaredFields", "DeclaredProperties", "DeclaredMethods", "DeclaredMembers", "DeclaredConstructors",
        "DeclaredNestedTypes", "DeclaredEvents");

    private static readonly DiagnosticDescriptor ReflectionRule = new(
        ReflectionId,
        "Reflection reaches owned or non-public state",
        "{0} reaches {1} through reflection; use the typed member (widen it to internal for the verification assemblies), inject it, or observe public behavior",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Verification arranges, executes and observes the project's own state through typed members (#622). " +
            "Only a declaration whose subject is metadata or serialization may reflect, marked [ReflectionAudit(\"reason\")].");

    private static readonly DiagnosticDescriptor UnsafeAccessorRule = new(
        UnsafeAccessorId,
        "UnsafeAccessor reaches into an owned type",
        "{0} declares an UnsafeAccessor into owned type {1}; use the typed member instead",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "UnsafeAccessor is a reflection-free bypass of the same visibility #622 forbids reflecting past.");

    private static readonly DiagnosticDescriptor UnexplainedAuditRule = new(
        UnexplainedAuditId,
        "ReflectionAudit without a reason",
        "[ReflectionAudit] on {0} must state why metadata or serialization is the subject",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A reflection exemption is narrow and documented (#622).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ReflectionRule, UnsafeAccessorRule, UnexplainedAuditRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method, SymbolKind.NamedType, SymbolKind.Property);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        string? reached = Reached(invocation, method, context.Compilation);
        if (reached is null || IsAudited(context.ContainingSymbol))
            return;
        context.ReportDiagnostic(Diagnostic.Create(ReflectionRule, invocation.Syntax.GetLocation(),
            context.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), reached));
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context)
    {
        var reference = (IPropertyReferenceOperation)context.Operation;
        if (!DeclaredMemberProperties.Contains(reference.Property.Name) || !IsReflectionType(reference.Property.ContainingType))
            return;
        ITypeSymbol? receiver = ReflectedType(reference.Instance);
        if (receiver is null || !IsOwned(receiver, context.Compilation) || IsAudited(context.ContainingSymbol))
            return;
        context.ReportDiagnostic(Diagnostic.Create(ReflectionRule, reference.Syntax.GetLocation(),
            context.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
            $"the declared members of {Display(receiver)}"));
    }

    /// <summary>What a reflective invocation reaches, or null when it is not this rule's concern.</summary>
    private static string? Reached(IInvocationOperation invocation, IMethodSymbol method, Compilation compilation)
    {
        string name = method.Name;
        INamedTypeSymbol owner = method.ContainingType;
        string ownerName = owner.ToDisplayString();

        if (TypeLookups.Contains(name) && (IsReflectionType(owner) || ownerName == "System.Reflection.RuntimeReflectionExtensions"))
        {
            IOperation? receiver = method.IsExtensionMethod || method.IsStatic
                ? invocation.Arguments.FirstOrDefault()?.Value
                : invocation.Instance;
            ITypeSymbol? reflected = ReflectedType(receiver);
            if (reflected is not null && IsOwned(reflected, compilation))
                return $"{Display(reflected)}.{name}";
            if (RequestsNonPublic(invocation))
                return $"non-public members via {name}";
            return null;
        }

        if (name == "GetUninitializedObject" &&
            ownerName is "System.Runtime.CompilerServices.RuntimeHelpers" or "System.Runtime.Serialization.FormatterServices")
        {
            ITypeSymbol? reflected = ReflectedType(invocation.Arguments.FirstOrDefault()?.Value);
            return reflected is null || IsOwned(reflected, compilation)
                ? $"an unconstructed {(reflected is null ? "object" : Display(reflected))}"
                : null;
        }

        if (name == "CreateInstance" && ownerName == "System.Activator" && RequestsNonPublic(invocation))
            return "a non-public constructor via Activator.CreateInstance";

        if (name == "GetType" && invocation.Arguments.Length >= 1 &&
            (ownerName == "System.Reflection.Assembly" || (ownerName == "System.Type" && method.IsStatic)) &&
            invocation.Arguments[0].Value.ConstantValue is { HasValue: true, Value: string typeName } &&
            typeName.StartsWith(OwnedAssemblyPrefix + ".", StringComparison.Ordinal))
            return $"owned type {typeName} by name";

        return null;
    }

    private static bool IsReflectionType(INamedTypeSymbol? type)
    {
        for (; type is not null; type = type.BaseType)
        {
            string name = type.ToDisplayString();
            if (name is "System.Type" or "System.Reflection.TypeInfo")
                return true;
        }
        return false;
    }

    /// <summary>The type a <see cref="Type"/>-valued expression statically denotes: <c>typeof(T)</c> or <c>x.GetType()</c>.</summary>
    private static ITypeSymbol? ReflectedType(IOperation? operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;
        switch (operation)
        {
            case ITypeOfOperation typeOf:
                return typeOf.TypeOperand;
            case IInvocationOperation { TargetMethod.Name: "GetType", Instance: { } instance, Arguments.Length: 0 }:
            {
                IOperation value = instance;
                while (value is IConversionOperation inner)
                    value = inner.Operand;
                return value.Type is { SpecialType: not SpecialType.System_Object } type ? type : null;
            }
            case IInvocationOperation { TargetMethod.Name: "GetTypeInfo", Arguments.Length: 1 } getTypeInfo:
                return ReflectedType(getTypeInfo.Arguments[0].Value);
            default:
                return null;
        }
    }

    private static bool RequestsNonPublic(IInvocationOperation invocation)
    {
        foreach (IArgumentOperation argument in invocation.Arguments)
        {
            ITypeSymbol? type = argument.Parameter?.Type;
            if (type?.ToDisplayString() == "System.Reflection.BindingFlags")
            {
                // A non-constant mask cannot be shown to stay public.
                if (argument.Value.ConstantValue is not { HasValue: true, Value: int flags })
                    return argument.ArgumentKind != ArgumentKind.DefaultValue;
                if ((flags & 32) != 0)
                    return true;
            }
            else if (argument.Parameter?.Name == "nonPublic" && type?.SpecialType == SpecialType.System_Boolean &&
                     argument.Value.ConstantValue is not { HasValue: true, Value: false })
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsOwned(ITypeSymbol type, Compilation compilation)
    {
        while (true)
        {
            switch (type)
            {
                case IArrayTypeSymbol array:
                    type = array.ElementType;
                    continue;
                case IPointerTypeSymbol pointer:
                    type = pointer.PointedAtType;
                    continue;
                default:
                    break;
            }
            break;
        }
        if (type is ITypeParameterSymbol)
            return false;
        IAssemblySymbol? assembly = type.OriginalDefinition.ContainingAssembly;
        return assembly is not null &&
            (SymbolEqualityComparer.Default.Equals(assembly, compilation.Assembly) ||
             assembly.Name.StartsWith(OwnedAssemblyPrefix, StringComparison.Ordinal));
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;
        foreach (AttributeData attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name == AuditAttributeName &&
                (attribute.ConstructorArguments.Length == 0 ||
                 attribute.ConstructorArguments[0].Value is not string reason || string.IsNullOrWhiteSpace(reason)))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnexplainedAuditRule,
                    attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? symbol.Locations[0],
                    symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }

        if (symbol is not IMethodSymbol method ||
            !method.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() ==
                "System.Runtime.CompilerServices.UnsafeAccessorAttribute"))
            return;
        ITypeSymbol? target = method.Parameters.FirstOrDefault()?.Type ?? method.ReturnType;
        if (target is not null && IsOwned(target, context.Compilation) && !IsAudited(method))
        {
            context.ReportDiagnostic(Diagnostic.Create(UnsafeAccessorRule, method.Locations[0],
                method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), Display(target)));
        }
    }

    /// <summary>Whether the code sits inside a declaration (or a type enclosing it) marked with an explained audit.</summary>
    private static bool IsAudited(ISymbol? symbol)
    {
        for (; symbol is not null; symbol = symbol.ContainingSymbol)
        {
            if (symbol is INamespaceSymbol)
                return false;
            // An accessor's container is the type; the audit sits on its property or event.
            if (HasExplainedAudit(symbol) || (symbol is IMethodSymbol { AssociatedSymbol: { } associated } && HasExplainedAudit(associated)))
                return true;
        }
        return false;
    }

    private static bool HasExplainedAudit(ISymbol symbol) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == AuditAttributeName &&
            attribute.ConstructorArguments.Length == 1 &&
            attribute.ConstructorArguments[0].Value is string reason && !string.IsNullOrWhiteSpace(reason));

    private static string Display(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
}
