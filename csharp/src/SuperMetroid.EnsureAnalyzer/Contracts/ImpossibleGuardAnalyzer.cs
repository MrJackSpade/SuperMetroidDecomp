using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Core checks no state its types and data already rule out (SME6481, #2553). In <c>SuperMetroid.Core</c> it reports:
/// <list type="bullet">
/// <item><c>Enum.IsDefined</c> on an enum value outside a type marked <c>[EncodingOwner]</c>: outside its codecs Core makes
/// no value that names no member (SME6450), so the test cannot fail.</item>
/// <item>A lookup that can miss (<c>TryGetValue</c>, <c>ContainsKey</c>, <c>GetValueOrDefault</c>, <c>Ensure.Listed</c>)
/// on a field or property whose initializer is a dictionary naming every member of its enum key: the catalog lists them
/// all, so it is an <c>IdentityTable</c> read by member.</item>
/// <item>A downcast whose failure throws (<c>x is T t ? t : throw</c>, <c>x as T ?? throw</c>,
/// <c>if (x is not T t) throw</c>) from a closed hierarchy, one whose every constructor is private or private protected:
/// the value is typed as the case the code needs, or the hierarchy answers the question itself.</item>
/// </list>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ImpossibleGuardAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core checks a state its types or data rule out.</summary>
    public const string ImpossibleGuardId = "SME6481";

    private static readonly DiagnosticDescriptor Rule = new(
        ImpossibleGuardId,
        "Core checks a state its types or data rule out",
        "{0}",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A value that must exist is held as the type that cannot express its absence, and a closed choice is " +
            "exhausted by its type or its catalog (#2553). Enum.IsDefined on a Core value tests what SME6450 already rules " +
            "out; a lookup that can miss on a catalog naming every member re-checks what the catalog guarantees (an " +
            "IdentityTable is read by member); a downcast that throws on a closed hierarchy re-checks what the type should " +
            "say. Make the state unrepresentable, then delete the check.");

    /// <summary>The lookups that answer whether a key is present.</summary>
    private static readonly ImmutableHashSet<string> MissableLookups = ImmutableHashSet.Create("TryGetValue", "ContainsKey", "GetValueOrDefault");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            start.RegisterOperationAction(AnalyzeConditional, OperationKind.Conditional);
            start.RegisterOperationAction(AnalyzeCoalesce, OperationKind.Coalesce);
        });
    }

    /// <summary>Reports <c>Enum.IsDefined</c> on an enum value and a missable lookup on a catalog naming every member.</summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        if (method.Name == "IsDefined" && method.ContainingType?.SpecialType == SpecialType.System_Enum)
        {
            if (invocation.Arguments.Length > 0 && Unconverted(invocation.Arguments[invocation.Arguments.Length - 1].Value).Type is { TypeKind: TypeKind.Enum } tested &&
                !InEncodingOwner(context.ContainingSymbol))
            {
                Report(context, invocation.Syntax,
                    $"'{invocation.Syntax}' tests whether a {tested.Name} is a member; outside its codec Core makes no value " +
                    "that names no member, so the test cannot fail");
            }
            return;
        }

        IOperation? table = null;
        IOperation? key = null;
        if (MissableLookups.Contains(method.Name) && invocation.Instance is { } instance)
        {
            table = instance;
            key = invocation.Arguments.Length > 0 ? invocation.Arguments[0].Value : null;
        }
        else if (method.Name == "Listed" && method.ContainingType?.Name == "Ensure" && invocation.Arguments.Length >= 2)
        {
            table = invocation.Arguments[0].Value;
            key = invocation.Arguments[1].Value;
        }
        if (table is null || key is null || Unconverted(key).Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } keyType)
            return;
        ISymbol? catalog = Unconverted(table) switch
        {
            IFieldReferenceOperation field => field.Field,
            IPropertyReferenceOperation property => property.Property,
            _ => null,
        };
        if (catalog is null || !NamesEveryMember(context.Compilation, catalog, keyType))
            return;
        Report(context, invocation.Syntax,
            $"'{invocation.Syntax}' asks whether '{catalog.Name}' holds a {keyType.Name}, but it names every member; hold it " +
            "as an IdentityTable and read the member");
    }

    /// <summary>Reports <c>x is T t ? t : throw</c> and <c>if (x is not T t) throw</c> on a closed hierarchy.</summary>
    private static void AnalyzeConditional(OperationAnalysisContext context)
    {
        var conditional = (IConditionalOperation)context.Operation;
        if (conditional.Condition is not IIsPatternOperation test || test.Pattern is not { } pattern)
            return;
        bool negated = pattern is INegatedPatternOperation;
        IPatternOperation matched = pattern is INegatedPatternOperation negation ? negation.Pattern : pattern;
        if (NarrowedType(matched) is not { } narrowed)
            return;
        IOperation? failure = negated ? conditional.WhenTrue : conditional.WhenFalse;
        if (failure is null || !Throws(failure))
            return;
        ReportDowncast(context, test.Value, narrowed, conditional.Syntax);
    }

    /// <summary>Reports <c>x as T ?? throw</c> on a closed hierarchy.</summary>
    private static void AnalyzeCoalesce(OperationAnalysisContext context)
    {
        var coalesce = (ICoalesceOperation)context.Operation;
        if (Unconverted(coalesce.Value) is not IConversionOperation { IsTryCast: true } cast || cast.Type is not { } narrowed ||
            !Throws(coalesce.WhenNull))
            return;
        ReportDowncast(context, cast.Operand, narrowed, coalesce.Syntax);
    }

    /// <summary>Reports a downcast from a closed hierarchy's base to one of its cases.</summary>
    private static void ReportDowncast(OperationAnalysisContext context, IOperation value, ITypeSymbol narrowed, SyntaxNode site)
    {
        if (Unconverted(value).Type is not INamedTypeSymbol { TypeKind: TypeKind.Class } source ||
            SymbolEqualityComparer.Default.Equals(source, narrowed) || !IsClosed(source) || !DerivesFrom(narrowed, source))
            return;
        Report(context, site,
            $"'{site}' throws unless the {source.Name} is a {narrowed.Name}, a case its closed hierarchy can hold; type the value " +
            "as the case the code needs, or have the hierarchy answer the question itself");
    }

    /// <summary>The type a declaration or type pattern narrows to, or null.</summary>
    private static ITypeSymbol? NarrowedType(IPatternOperation pattern) => pattern switch
    {
        IDeclarationPatternOperation declaration => declaration.MatchedType,
        ITypePatternOperation type => type.MatchedType,
        IRecursivePatternOperation recursive => recursive.MatchedType,
        _ => null,
    };

    /// <summary>Whether an operation is a throw, through the conversion the compiler wraps it in.</summary>
    private static bool Throws(IOperation operation) => Unconverted(operation) switch
    {
        IThrowOperation => true,
        IBlockOperation { Operations.Length: 1 } block => Throws(block.Operations[0]),
        IExpressionStatementOperation { Operation: IThrowOperation } => true,
        _ => false,
    };

    /// <summary>
    /// Whether an abstract class can be derived only by itself or inside its own assembly: it declares a constructor and every
    /// constructor it declares is private or private protected. A record's copy constructor, which the compiler writes, is
    /// not a way to make a new case.
    /// </summary>
    private static bool IsClosed(INamedTypeSymbol type)
    {
        if (!type.IsAbstract)
            return false;
        IMethodSymbol[] written = [.. type.InstanceConstructors.Where(constructor => !IsCopyConstructor(type, constructor))];
        return written.Length > 0 && written.All(constructor => !constructor.IsImplicitlyDeclared &&
            constructor.DeclaredAccessibility is Accessibility.Private or Accessibility.ProtectedAndInternal);
    }

    /// <summary>Whether a constructor is the copy constructor a record's compiler writes.</summary>
    private static bool IsCopyConstructor(INamedTypeSymbol type, IMethodSymbol constructor) =>
        type.IsRecord && constructor.IsImplicitlyDeclared && constructor.Parameters.Length == 1 &&
        SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, type);

    /// <summary>Whether <paramref name="type"/> derives from <paramref name="baseType"/>.</summary>
    private static bool DerivesFrom(ITypeSymbol type, INamedTypeSymbol baseType)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType.OriginalDefinition))
                return true;
        }
        return false;
    }

    /// <summary>Whether a field's or property's initializer is a dictionary whose keys name every member of the enum.</summary>
    private static bool NamesEveryMember(Compilation compilation, ISymbol catalog, INamedTypeSymbol keyType)
    {
        foreach (SyntaxReference reference in catalog.DeclaringSyntaxReferences)
        {
            ExpressionSyntax? value = reference.GetSyntax() switch
            {
                VariableDeclaratorSyntax variable => variable.Initializer?.Value,
                PropertyDeclarationSyntax property => property.Initializer?.Value,
                _ => null,
            };
            if (value is null || !compilation.ContainsSyntaxTree(value.SyntaxTree))
                continue;
            IOperation? initializer = compilation.GetSemanticModel(value.SyntaxTree).GetOperation(value);
            if (Creation(initializer) is not { Initializer: { } entries })
                continue;
            var named = new HashSet<object?>();
            foreach (IOperation entry in entries.Initializers)
            {
                IOperation? keyOperation = entry switch
                {
                    ISimpleAssignmentOperation { Target: IPropertyReferenceOperation { Arguments.Length: 1 } indexer } => indexer.Arguments[0].Value,
                    IInvocationOperation { TargetMethod.Name: "Add", Arguments.Length: 2 } add => add.Arguments[0].Value,
                    _ => null,
                };
                if (keyOperation is not null && Unconverted(keyOperation).ConstantValue is { HasValue: true } constant)
                    named.Add(constant.Value);
            }
            IEnumerable<object?> members = keyType.GetMembers().OfType<IFieldSymbol>()
                .Where(field => field.HasConstantValue).Select(field => field.ConstantValue);
            return members.Any() && members.All(named.Contains);
        }
        return false;
    }

    /// <summary>The dictionary creation an initializer builds, through a freezing or copying call on it.</summary>
    private static IObjectCreationOperation? Creation(IOperation? initializer)
    {
        IOperation? current = initializer is null ? null : Unconverted(initializer);
        while (current is IInvocationOperation invocation)
        {
            current = invocation.Instance ?? (invocation.Arguments.Length > 0 ? invocation.Arguments[0].Value : null);
            current = current is null ? null : Unconverted(current);
        }
        return current as IObjectCreationOperation;
    }

    /// <summary>Whether a symbol lies in a type marked as an encoding's owner.</summary>
    private static bool InEncodingOwner(ISymbol symbol)
    {
        for (INamedTypeSymbol? type = symbol.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == CartridgeEncodingScanner.EncodingOwnerAttribute))
                return true;
        }
        return false;
    }

    /// <summary>The operand beneath any implicit conversions.</summary>
    private static IOperation Unconverted(IOperation operation)
    {
        while (operation is IConversionOperation { IsImplicit: true } conversion)
            operation = conversion.Operand;
        return operation;
    }

    /// <summary>Reports one finding at a site.</summary>
    private static void Report(OperationAnalysisContext context, SyntaxNode site, string message) =>
        context.ReportDiagnostic(Diagnostic.Create(Rule, site.GetLocation(), message));
}
