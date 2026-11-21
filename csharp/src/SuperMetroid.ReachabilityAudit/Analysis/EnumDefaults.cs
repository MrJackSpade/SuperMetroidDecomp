using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// The enum members a value holds when nothing gave it one. A field, an automatic property, an array element and an
/// optional parameter of an enum type take the type's default, zero, and so does a <c>default</c>, a zero or a
/// <c>new E()</c> converted to it: at run time that value is the enum's zero member, though no code names it. Each such
/// place reaches the zero member, so the prune keeps the name of the value every fresh instance holds: f4af8a1aca removed
/// <c>SamusPresentationMode.Gameplay</c> and 0ea41f339b <c>LiquidMotionStage.AwaitingFirstPass</c>, which the
/// presentation's mode and the room's liquid motion started from unnamed, and a debugger state holding either could no
/// longer be restored (#2608).
/// </summary>
/// <remarks>
/// A field or automatic property takes its default unless an initializer gives it a value, and in a struct always, since
/// <c>default</c> and a new array of the struct hold every instance field at zero whatever its initializer; so does a
/// struct's primary constructor parameter. An optional parameter's default is the expression it declares, which the walk
/// reads like any other, or zero for one marked <c>[Optional]</c> alone. An array takes the default in every element when
/// it is created without an initializer, stackalloc included, and in the elements <c>Array.Clear</c>,
/// <c>Array.Resize</c> or <c>Span&lt;T&gt;.Clear</c> clear or add. A nullable enum's default is null, which is no member.
/// </remarks>
internal static class EnumDefaults
{
    /// <summary>The members of <paramref name="type"/>, an enum, whose value is zero; none for any other type.</summary>
    public static IEnumerable<IFieldSymbol> ZeroMembers(ITypeSymbol? type) => type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType
        ? enumType.GetMembers().OfType<IFieldSymbol>().Where(member => member.HasConstantValue && IsZero(member.ConstantValue))
        : [];

    /// <summary>
    /// The storage a declaration gives a default and the zero members that default is, or null when it holds no enum
    /// default: a field or automatic property (its backing field is the compiler's) without an initializer or of a
    /// struct instance, a struct's primary constructor parameter, and a parameter marked <c>[Optional]</c> without a
    /// default value.
    /// </summary>
    public static (ISymbol Storage, List<IFieldSymbol> Members)? HeldByDeclaration(SemanticModel model, SyntaxNode node) => node switch
    {
        VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax } declarator
            when model.GetDeclaredSymbol(declarator) is IFieldSymbol { IsConst: false } field
                 && (declarator.Initializer is null || HeldByStructInstance(field)) => Held(field, field.Type),
        PropertyDeclarationSyntax declaration
            when model.GetDeclaredSymbol(declaration) is IPropertySymbol property && HasBackingField(property)
                 && (declaration.Initializer is null || HeldByStructInstance(property)) => Held(property, property.Type),
        ParameterSyntax { Parent.Parent: TypeDeclarationSyntax owner } parameter
            when model.GetDeclaredSymbol(owner) is INamedTypeSymbol { IsValueType: true }
                 && model.GetDeclaredSymbol(parameter) is IParameterSymbol primary => Held(primary, primary.Type),
        ParameterSyntax parameter
            when model.GetDeclaredSymbol(parameter) is IParameterSymbol { IsOptional: true, HasExplicitDefaultValue: false } optional
            => Held(optional, optional.Type),
        _ => null,
    };

    /// <summary>
    /// The zero members an expression gives or leaves a value of an enum type: a constant zero converted to the enum (a
    /// <c>default</c>, a <c>0</c>, a cast of zero) that names no member, a <c>new E()</c>, an array of the enum created
    /// without an initializer, and a clear or resize of one.
    /// </summary>
    public static IEnumerable<IFieldSymbol> HeldByExpression(SemanticModel model, ExpressionSyntax expression, SymbolInfo info)
    {
        var held = new List<IFieldSymbol>();
        TypeInfo typeInfo = model.GetTypeInfo(expression);
        if (info.Symbol is not IFieldSymbol && model.GetConstantValue(expression) is { HasValue: true, Value: var value } && IsZero(value))
        {
            held.AddRange(ZeroMembers(typeInfo.Type));
            held.AddRange(ZeroMembers(typeInfo.ConvertedType));
        }
        switch (expression)
        {
            case BaseObjectCreationExpressionSyntax:
                held.AddRange(ZeroMembers(typeInfo.Type));
                break;
            case ArrayCreationExpressionSyntax { Initializer: null }:
                held.AddRange(ZeroMembers((typeInfo.Type as IArrayTypeSymbol)?.ElementType));
                break;
            case StackAllocArrayCreationExpressionSyntax { Initializer: null, Type: ArrayTypeSyntax array }:
                held.AddRange(ZeroMembers(model.GetTypeInfo(array.ElementType).Type));
                break;
            case InvocationExpressionSyntax invocation when info.Symbol is IMethodSymbol method:
                held.AddRange(ClearedElements(model, invocation, method).SelectMany(ZeroMembers));
                break;
            default:
                break;
        }
        return held;
    }

    /// <summary>The element types whose elements a call sets to their default: <c>Array.Clear</c> and
    /// <c>Array.Resize</c> (the elements it adds) on an array, and <c>Clear</c> on a span.</summary>
    private static List<ITypeSymbol> ClearedElements(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        bool arrayClear = method.ContainingType.SpecialType == SpecialType.System_Array && method.Name is "Clear" or "Resize";
        bool spanClear = method.Name == "Clear" && method.ContainingType is { Name: "Span", ContainingNamespace.Name: "System" };
        if (!arrayClear && !spanClear)
            return [];
        var types = new List<ITypeSymbol>(method.TypeArguments);
        if (spanClear)
            types.AddRange(method.ContainingType.TypeArguments);
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            if (model.GetTypeInfo(argument.Expression).Type is IArrayTypeSymbol array)
                types.Add(array.ElementType);
        }
        return types;
    }

    /// <summary>Whether a constant is an integral zero, the only constant an enum's value can be converted from.</summary>
    private static bool IsZero(object? value) => value switch
    {
        sbyte or byte or short or ushort or int or uint or long or ulong =>
            Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture) == 0m,
        _ => false,
    };

    /// <summary>Whether an instance member of a struct holds its default in <c>default</c> and a new array of the struct.</summary>
    private static bool HeldByStructInstance(ISymbol member) => !member.IsStatic && member.ContainingType.IsValueType;

    /// <summary>Whether the compiler gives the property a backing field: an automatic property, or one using <c>field</c>.</summary>
    private static bool HasBackingField(IPropertySymbol property) =>
        property.ContainingType.GetMembers().OfType<IFieldSymbol>().Any(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, property));

    /// <summary>The storage and its zero members, or null when its type is no enum with one.</summary>
    private static (ISymbol Storage, List<IFieldSymbol> Members)? Held(ISymbol storage, ITypeSymbol type) =>
        ZeroMembers(type).ToList() is { Count: > 0 } members ? (storage, members) : null;
}
