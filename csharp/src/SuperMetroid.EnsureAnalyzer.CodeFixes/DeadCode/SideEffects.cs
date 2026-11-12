using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>Which values a dead-code fix may drop without keeping their evaluation.</summary>
internal static class SideEffects
{
    /// <summary>
    /// Whether evaluating <paramref name="operation"/> has no effect the program can observe: a constant, a literal,
    /// <c>default</c>, <c>typeof</c>, <c>sizeof</c>, <c>nameof</c>, <c>this</c>, a local, a parameter, a field read
    /// (of a free receiver), a lambda or method group, <c>new object()</c>, or an implicit built-in conversion of one of
    /// those. Anything else, a property or method call included, is kept.
    /// </summary>
    internal static bool IsFree(IOperation? operation) => operation switch
    {
        null => false,
        { ConstantValue.HasValue: true } => true,
        ILiteralOperation or IDefaultValueOperation or ITypeOfOperation or ISizeOfOperation or INameOfOperation => true,
        IInstanceReferenceOperation or ILocalReferenceOperation or IParameterReferenceOperation or IAnonymousFunctionOperation => true,
        IFieldReferenceOperation field => field.Instance is null || IsFree(field.Instance),
        IDelegateCreationOperation creation => creation.Target is IAnonymousFunctionOperation ||
            creation.Target is IMethodReferenceOperation reference && (reference.Instance is null || IsFree(reference.Instance)),
        IConversionOperation conversion => conversion.Conversion.IsImplicit && !conversion.Conversion.IsUserDefined && IsFree(conversion.Operand),
        IObjectCreationOperation creation => creation.Type?.SpecialType == SpecialType.System_Object && creation.Arguments.IsEmpty &&
            creation.Initializer is null,
        _ => false,
    };
}
