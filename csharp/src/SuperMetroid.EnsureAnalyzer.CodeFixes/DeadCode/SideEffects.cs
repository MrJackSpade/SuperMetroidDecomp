using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>Which values a dead-code fix may drop without keeping their evaluation.</summary>
internal static class SideEffects
{
    /// <summary>
    /// Whether evaluating <paramref name="operation"/> has no effect the program can observe: a constant, a literal,
    /// <c>default</c>, <c>typeof</c>, <c>sizeof</c>, <c>nameof</c>, <c>this</c>, a local, a parameter, a field read
    /// (of a free receiver), a read of an automatic property declared in source (of a free receiver), whose getter only
    /// returns its stored value, a lambda or method group, <c>new object()</c>, or an implicit built-in conversion of one
    /// of those. Anything else, any other property or a method call included, is kept.
    /// </summary>
    internal static bool IsFree(IOperation? operation) => operation switch
    {
        null => false,
        { ConstantValue.HasValue: true } => true,
        ILiteralOperation or IDefaultValueOperation or ITypeOfOperation or ISizeOfOperation or INameOfOperation => true,
        IInstanceReferenceOperation or ILocalReferenceOperation or IParameterReferenceOperation or IAnonymousFunctionOperation => true,
        IFieldReferenceOperation field => field.Instance is null || IsFree(field.Instance),
        IPropertyReferenceOperation { Arguments.IsEmpty: true } property => IsAutomatic(property.Property) &&
            (property.Instance is null || IsFree(property.Instance)),
        IDelegateCreationOperation creation => creation.Target is IAnonymousFunctionOperation ||
            creation.Target is IMethodReferenceOperation reference && (reference.Instance is null || IsFree(reference.Instance)),
        IConversionOperation conversion => conversion.Conversion.IsImplicit && !conversion.Conversion.IsUserDefined && IsFree(conversion.Operand),
        IObjectCreationOperation creation => creation.Type?.SpecialType == SpecialType.System_Object && creation.Arguments.IsEmpty &&
            creation.Initializer is null,
        _ => false,
    };

    /// <summary>
    /// Whether a property is automatic: declared in source with a get accessor that has no body, so reading it only
    /// returns the value its compiler-generated field stores.
    /// </summary>
    private static bool IsAutomatic(IPropertySymbol property) =>
        property is { IsIndexer: false, GetMethod: not null } &&
        property.DeclaringSyntaxReferences.Length > 0 &&
        property.DeclaringSyntaxReferences.All(reference => reference.GetSyntax() is PropertyDeclarationSyntax
        {
            ExpressionBody: null, AccessorList: { } accessors,
        } && accessors.Accessors.All(accessor => accessor.Body is null && accessor.ExpressionBody is null));
}
