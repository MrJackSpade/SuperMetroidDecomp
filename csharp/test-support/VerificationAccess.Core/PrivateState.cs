using System.Collections.Concurrent;
using System.Reflection;

namespace SuperMetroid.Core;

/// <summary>
/// Reaches state only its owner's own updates write or read (#2107, #2129, #2570): a check, or a native snapshot's import,
/// sets a property through its private setter, reads a private field or calls a private method, by name, so production
/// keeps no member wider than its own code needs for verification's sake. The generated <c>*PrivateAccess</c> classes name
/// each private member a check reaches as the owner's own code names it, and reach it here.
/// </summary>
internal static class PrivateState
{
    /// <summary>Every member, public or not, static or instance: the owner's own, and those it inherits that it can see.</summary>
    private const BindingFlags Declared = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>The members found so far, by owner, kind, name and parameter types.</summary>
    private static readonly ConcurrentDictionary<MemberKey, MemberInfo> Found = new(MemberKeyComparer.Instance);

    /// <summary>No parameter types: a field's, a property's or a generic method's key.</summary>
    private static readonly Type[] NoTypes = [];

    /// <summary>Sets <paramref name="property"/> of <paramref name="owner"/> to <paramref name="value"/> through its setter.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no settable property of that name.</exception>
    internal static void Set<TOwner>(TOwner owner, string property, object? value) where TOwner : class =>
        Setter(typeof(TOwner), property).Invoke(Required(owner), BindingFlags.DoNotWrapExceptions, binder: null, [value], culture: null);

    /// <summary>The value of <paramref name="owner"/>'s instance property <paramref name="property"/>, read through its getter.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no readable property of that name.</exception>
    /// <exception cref="InvalidCastException">The property does not hold a <typeparamref name="TValue"/>.</exception>
    internal static TValue Get<TOwner, TValue>(TOwner owner, string property) where TOwner : class =>
        (TValue)Getter(typeof(TOwner), property).Invoke(Required(owner), BindingFlags.DoNotWrapExceptions, binder: null, [], culture: null)!;

    /// <summary>Calls <paramref name="owner"/>'s instance method <paramref name="method"/> with <paramref name="arguments"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance method of that name.</exception>
    internal static void Call<TOwner>(TOwner owner, string method, params object?[] arguments) where TOwner : class =>
        (typeof(TOwner).GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(TOwner).Name, method))
        .Invoke(Required(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>Replaces the value of <paramref name="owner"/>'s instance field <paramref name="field"/>, read-only or not.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    internal static void SetField<TOwner>(TOwner owner, string field, object? value) where TOwner : class =>
        FieldOf(typeof(TOwner), field).SetValue(Required(owner), value);

    /// <summary>The value of <paramref name="owner"/>'s instance field <paramref name="field"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    /// <exception cref="InvalidCastException">The field does not hold a <typeparamref name="TValue"/>.</exception>
    internal static TValue Field<TOwner, TValue>(TOwner owner, string field) where TOwner : class =>
        (TValue)FieldOf(typeof(TOwner), field).GetValue(Required(owner))!;

    /// <summary>The value of the instance field <paramref name="field"/> of the value <paramref name="owner"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    /// <exception cref="InvalidCastException">The field does not hold a <typeparamref name="TValue"/>.</exception>
    internal static TValue ValueField<TOwner, TValue>(TOwner owner, string field) where TOwner : struct =>
        (TValue)FieldOf(typeof(TOwner), field).GetValue(owner)!;

    /// <summary>Replaces the value of the instance field <paramref name="field"/> of the value <paramref name="owner"/> in place.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    internal static void SetValueField<TOwner>(ref TOwner owner, string field, object? value) where TOwner : struct
    {
        object boxed = owner;
        FieldOf(typeof(TOwner), field).SetValue(boxed, value);
        owner = (TOwner)boxed;
    }

    /// <summary>The value of the instance property <paramref name="property"/> of the value <paramref name="owner"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no readable property of that name.</exception>
    internal static TValue ValueGet<TOwner, TValue>(TOwner owner, string property) where TOwner : struct =>
        (TValue)Getter(typeof(TOwner), property).Invoke(owner, BindingFlags.DoNotWrapExceptions, binder: null, [], culture: null)!;

    /// <summary>Sets the instance property <paramref name="property"/> of the value <paramref name="owner"/> in place.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no settable property of that name.</exception>
    internal static void ValueSet<TOwner>(ref TOwner owner, string property, object? value) where TOwner : struct
    {
        object boxed = owner;
        Setter(typeof(TOwner), property).Invoke(boxed, BindingFlags.DoNotWrapExceptions, binder: null, [value], culture: null);
        owner = (TOwner)boxed;
    }

    /// <summary>
    /// <paramref name="owner"/> with its instance property <paramref name="property"/> set: a value created in an object
    /// initializer or <c>with</c> expression, which a private setter cannot appear in.
    /// </summary>
    /// <exception cref="MissingMemberException">The owner's type has no settable property of that name.</exception>
    internal static TOwner With<TOwner>(TOwner owner, string property, object? value)
    {
        object boxed = owner ?? throw new ArgumentNullException(nameof(owner));
        Setter(typeof(TOwner), property).Invoke(boxed, BindingFlags.DoNotWrapExceptions, binder: null, [value], culture: null);
        return (TOwner)boxed;
    }
    /// <summary>The value of <paramref name="owner"/>'s static field <paramref name="field"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no static field of that name.</exception>
    internal static TValue StaticField<TValue>(Type owner, string field) =>
        (TValue)FieldOf(owner, field).GetValue(null)!;

    /// <summary>Replaces the value of <paramref name="owner"/>'s static field <paramref name="field"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no static field of that name.</exception>
    internal static void SetStaticField(Type owner, string field, object? value) =>
        FieldOf(owner, field).SetValue(null, value);

    /// <summary>The value of <paramref name="owner"/>'s static property <paramref name="property"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no readable static property of that name.</exception>
    internal static TValue StaticGet<TValue>(Type owner, string property) =>
        (TValue)Getter(owner, property).Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, [], culture: null)!;

    /// <summary>Sets <paramref name="owner"/>'s static property <paramref name="property"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no settable static property of that name.</exception>
    internal static void StaticSet(Type owner, string property, object? value) =>
        Setter(owner, property).Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, [value], culture: null);

    /// <summary>
    /// Calls <paramref name="owner"/>'s method <paramref name="method"/> taking <paramref name="parameters"/> with
    /// <paramref name="arguments"/>, which a <c>ref</c> or <c>out</c> parameter writes back into, and returns its result.
    /// </summary>
    /// <exception cref="MissingMemberException">The owner's type has no method of that name taking those parameters.</exception>
    internal static TResult Invoke<TOwner, TResult>(TOwner owner, string method, Type[] parameters, params object?[] arguments) where TOwner : class =>
        (TResult)MethodOf(typeof(TOwner), method, parameters).Invoke(Required(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null)!;

    /// <summary>Calls <paramref name="owner"/>'s method <paramref name="method"/> taking <paramref name="parameters"/>, which returns nothing.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no method of that name taking those parameters.</exception>
    internal static void Invoke<TOwner>(TOwner owner, string method, Type[] parameters, params object?[] arguments) where TOwner : class =>
        MethodOf(typeof(TOwner), method, parameters).Invoke(Required(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>Calls the value <paramref name="owner"/>'s method <paramref name="method"/> in place and returns its result.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no method of that name taking those parameters.</exception>
    internal static TResult InvokeValue<TOwner, TResult>(ref TOwner owner, string method, Type[] parameters, params object?[] arguments) where TOwner : struct
    {
        object boxed = owner;
        object? result = MethodOf(typeof(TOwner), method, parameters).Invoke(boxed, BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);
        owner = (TOwner)boxed;
        return (TResult)result!;
    }

    /// <summary>Calls the value <paramref name="owner"/>'s method <paramref name="method"/>, which returns nothing, in place.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no method of that name taking those parameters.</exception>
    internal static void InvokeValue<TOwner>(ref TOwner owner, string method, Type[] parameters, params object?[] arguments) where TOwner : struct
    {
        object boxed = owner;
        MethodOf(typeof(TOwner), method, parameters).Invoke(boxed, BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);
        owner = (TOwner)boxed;
    }

    /// <summary>Calls <paramref name="owner"/>'s static method <paramref name="method"/> and returns its result.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no method of that name taking those parameters.</exception>
    internal static TResult InvokeStatic<TResult>(Type owner, string method, Type[] parameters, params object?[] arguments) =>
        (TResult)MethodOf(owner, method, parameters).Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null)!;

    /// <summary>Calls <paramref name="owner"/>'s static method <paramref name="method"/>, which returns nothing.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no method of that name taking those parameters.</exception>
    internal static void InvokeStatic(Type owner, string method, Type[] parameters, params object?[] arguments) =>
        MethodOf(owner, method, parameters).Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>A new <typeparamref name="TOwner"/> from its constructor taking <paramref name="parameters"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no constructor taking those parameters.</exception>
    internal static TOwner New<TOwner>(Type[] parameters, params object?[] arguments) =>
        (TOwner)((ConstructorInfo)Found.GetOrAdd(new MemberKey(typeof(TOwner), ".ctor", "", parameters, 0, 0), _ =>
            typeof(TOwner).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, parameters)
                ?? throw new MissingMemberException(typeof(TOwner).Name, ".ctor")))
        .Invoke(BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>
    /// Calls <paramref name="owner"/>'s generic method <paramref name="method"/>, made for
    /// <paramref name="typeArguments"/>, and returns its result.
    /// </summary>
    /// <exception cref="MissingMemberException">The owner's type has no single such method of that many parameters.</exception>
    internal static TResult InvokeGeneric<TOwner, TResult>(TOwner owner, string method, Type[] typeArguments, params object?[] arguments) where TOwner : class =>
        (TResult)GenericMethodOf(typeof(TOwner), method, typeArguments, arguments.Length)
            .Invoke(Required(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null)!;

    /// <summary>Calls <paramref name="owner"/>'s generic method <paramref name="method"/>, made for <paramref name="typeArguments"/>, which returns nothing.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no single such method of that many parameters.</exception>
    internal static void InvokeGeneric<TOwner>(TOwner owner, string method, Type[] typeArguments, params object?[] arguments) where TOwner : class =>
        GenericMethodOf(typeof(TOwner), method, typeArguments, arguments.Length)
            .Invoke(Required(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>Calls <paramref name="owner"/>'s static generic method <paramref name="method"/>, made for <paramref name="typeArguments"/>, and returns its result.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no single such method of that many parameters.</exception>
    internal static TResult InvokeStaticGeneric<TResult>(Type owner, string method, Type[] typeArguments, params object?[] arguments) =>
        (TResult)GenericMethodOf(owner, method, typeArguments, arguments.Length)
            .Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null)!;

    /// <summary>Calls <paramref name="owner"/>'s static generic method <paramref name="method"/>, made for <paramref name="typeArguments"/>, which returns nothing.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no single such method of that many parameters.</exception>
    internal static void InvokeStaticGeneric(Type owner, string method, Type[] typeArguments, params object?[] arguments) =>
        GenericMethodOf(owner, method, typeArguments, arguments.Length)
            .Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>
    /// <paramref name="owner"/>'s method <paramref name="method"/> taking <paramref name="parameters"/> as a
    /// <typeparamref name="TDelegate"/>, an instance method's receiver first: the call for a method whose signature holds
    /// a <c>ref struct</c>, which reflection cannot pass as an argument.
    /// </summary>
    /// <exception cref="MissingMemberException">The owner's type has no method of that name taking those parameters.</exception>
    internal static TDelegate MethodDelegate<TDelegate>(Type owner, string method, Type[] parameters) where TDelegate : Delegate =>
        MethodOf(owner, method, parameters).CreateDelegate<TDelegate>();

    /// <summary>
    /// The getter of <paramref name="owner"/>'s property <paramref name="property"/> as a <typeparamref name="TDelegate"/>,
    /// an instance property's receiver first: the read of a <c>ref struct</c> property, which reflection cannot return.
    /// </summary>
    /// <exception cref="MissingMemberException">The owner's type has no readable property of that name.</exception>
    internal static TDelegate GetterDelegate<TDelegate>(Type owner, string property) where TDelegate : Delegate =>
        Getter(owner, property).CreateDelegate<TDelegate>();

    /// <summary>The owner a member is reached on, which must be one: PrivateState depends on nothing but the runtime, so a
    /// fixture repository's checks can use it as the repository's do.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="owner"/> is null.</exception>
    private static TOwner Required<TOwner>(TOwner owner) where TOwner : class
    {
        ArgumentNullException.ThrowIfNull(owner);
        return owner;
    }

    /// <summary>The field <paramref name="name"/> <paramref name="owner"/> declares.</summary>
    private static FieldInfo FieldOf(Type owner, string name) =>
        (FieldInfo)Found.GetOrAdd(new MemberKey(owner, "field", name, NoTypes, 0, 0), static key =>
            key.Owner.GetField(key.Name, Declared) ?? throw new MissingMemberException(key.Owner.Name, key.Name));

    /// <summary>The getter of the property <paramref name="name"/> <paramref name="owner"/> declares.</summary>
    private static MethodInfo Getter(Type owner, string name) =>
        (MethodInfo)Found.GetOrAdd(new MemberKey(owner, "get", name, NoTypes, 0, 0), static key =>
            key.Owner.GetProperty(key.Name, Declared)?.GetMethod ?? throw new MissingMemberException(key.Owner.Name, key.Name));

    /// <summary>The setter, or init accessor, of the property <paramref name="name"/> <paramref name="owner"/> declares.</summary>
    private static MethodInfo Setter(Type owner, string name) =>
        (MethodInfo)Found.GetOrAdd(new MemberKey(owner, "set", name, NoTypes, 0, 0), static key =>
            key.Owner.GetProperty(key.Name, Declared)?.SetMethod ?? throw new MissingMemberException(key.Owner.Name, key.Name));

    /// <summary>The method <paramref name="name"/> <paramref name="owner"/> declares taking <paramref name="parameters"/>.</summary>
    private static MethodInfo MethodOf(Type owner, string name, Type[] parameters) =>
        (MethodInfo)Found.GetOrAdd(new MemberKey(owner, "method", name, parameters, 0, 0), static key =>
            key.Owner.GetMethod(key.Name, Declared, key.Parameters) ?? throw new MissingMemberException(key.Owner.Name, key.Name));

    /// <summary>
    /// The generic method <paramref name="name"/> <paramref name="owner"/> declares with as many type parameters as
    /// <paramref name="typeArguments"/> and <paramref name="parameterCount"/> parameters, made for those type arguments.
    /// </summary>
    private static MethodInfo GenericMethodOf(Type owner, string name, Type[] typeArguments, int parameterCount) =>
        ((MethodInfo)Found.GetOrAdd(new MemberKey(owner, "generic", name, NoTypes, typeArguments.Length, parameterCount), _ =>
        {
            MethodInfo[] candidates = [.. owner.GetMethods(Declared).Where(candidate => candidate.Name == name && candidate.IsGenericMethodDefinition &&
                candidate.GetGenericArguments().Length == typeArguments.Length && candidate.GetParameters().Length == parameterCount)];
            return candidates.Length == 1 ? candidates[0] : throw new MissingMemberException(owner.Name, name);
        })).MakeGenericMethod(typeArguments);

    /// <summary>A member found: its owner, its kind, its name, and the parameter types or counts that tell overloads apart.</summary>
    /// <param name="Owner">The type declaring or inheriting the member.</param>
    /// <param name="Kind">A field, an accessor, a method, a generic method or a constructor.</param>
    /// <param name="Name">The member's name; empty for a constructor.</param>
    /// <param name="Parameters">A method's or constructor's parameter types.</param>
    /// <param name="TypeArguments">A generic method's type parameter count.</param>
    /// <param name="ParameterCount">A generic method's parameter count.</param>
    private readonly record struct MemberKey(Type Owner, string Kind, string Name, Type[] Parameters, int TypeArguments, int ParameterCount);

    /// <summary>Tells keys apart by their parts, the parameter types one by one.</summary>
    private sealed class MemberKeyComparer : IEqualityComparer<MemberKey>
    {
        /// <summary>The one comparer.</summary>
        internal static MemberKeyComparer Instance { get; } = new();

        /// <inheritdoc/>
        public bool Equals(MemberKey x, MemberKey y) =>
            x.Owner == y.Owner && string.Equals(x.Kind, y.Kind, StringComparison.Ordinal) && string.Equals(x.Name, y.Name, StringComparison.Ordinal) &&
            x.TypeArguments == y.TypeArguments && x.ParameterCount == y.ParameterCount && x.Parameters.AsSpan().SequenceEqual(y.Parameters);

        /// <inheritdoc/>
        public int GetHashCode(MemberKey key) =>
            HashCode.Combine(key.Owner, StringComparer.Ordinal.GetHashCode(key.Kind), StringComparer.Ordinal.GetHashCode(key.Name),
                key.Parameters.Length, key.TypeArguments, key.ParameterCount);
    }
}
