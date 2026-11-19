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
    private static readonly ConcurrentDictionary<string, MemberInfo> Found = new(StringComparer.Ordinal);

    /// <summary>Sets <paramref name="property"/> of <paramref name="owner"/> to <paramref name="value"/> through its setter.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no settable property of that name.</exception>
    internal static void Set<TOwner>(TOwner owner, string property, object? value) where TOwner : class =>
        Setter(typeof(TOwner), property).Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, [value], culture: null);

    /// <summary>The value of <paramref name="owner"/>'s instance property <paramref name="property"/>, read through its getter.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no readable property of that name.</exception>
    /// <exception cref="InvalidCastException">The property does not hold a <typeparamref name="TValue"/>.</exception>
    internal static TValue Get<TOwner, TValue>(TOwner owner, string property) where TOwner : class =>
        (TValue)Getter(typeof(TOwner), property).Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, [], culture: null)!;

    /// <summary>Calls <paramref name="owner"/>'s instance method <paramref name="method"/> with <paramref name="arguments"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance method of that name.</exception>
    internal static void Call<TOwner>(TOwner owner, string method, params object?[] arguments) where TOwner : class =>
        (typeof(TOwner).GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(TOwner).Name, method))
        .Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>Replaces the value of <paramref name="owner"/>'s instance field <paramref name="field"/>, read-only or not.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    internal static void SetField<TOwner>(TOwner owner, string field, object? value) where TOwner : class =>
        FieldOf(typeof(TOwner), field).SetValue(Ensure.NotNull(owner), value);

    /// <summary>The value of <paramref name="owner"/>'s instance field <paramref name="field"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    /// <exception cref="InvalidCastException">The field does not hold a <typeparamref name="TValue"/>.</exception>
    internal static TValue Field<TOwner, TValue>(TOwner owner, string field) where TOwner : class =>
        (TValue)FieldOf(typeof(TOwner), field).GetValue(Ensure.NotNull(owner))!;

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
        (TResult)MethodOf(typeof(TOwner), method, parameters).Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null)!;

    /// <summary>Calls <paramref name="owner"/>'s method <paramref name="method"/> taking <paramref name="parameters"/>, which returns nothing.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no method of that name taking those parameters.</exception>
    internal static void Invoke<TOwner>(TOwner owner, string method, Type[] parameters, params object?[] arguments) where TOwner : class =>
        MethodOf(typeof(TOwner), method, parameters).Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

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
        (TOwner)((ConstructorInfo)Found.GetOrAdd($"{typeof(TOwner).AssemblyQualifiedName}|.ctor|{Signature(parameters)}", _ =>
            typeof(TOwner).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, parameters)
                ?? throw new MissingMemberException(typeof(TOwner).Name, ".ctor")))
        .Invoke(BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>The field <paramref name="name"/> <paramref name="owner"/> declares.</summary>
    private static FieldInfo FieldOf(Type owner, string name) =>
        (FieldInfo)Found.GetOrAdd($"{owner.AssemblyQualifiedName}|field|{name}", _ =>
            owner.GetField(name, Declared) ?? throw new MissingMemberException(owner.Name, name));

    /// <summary>The getter of the property <paramref name="name"/> <paramref name="owner"/> declares.</summary>
    private static MethodInfo Getter(Type owner, string name) =>
        (MethodInfo)Found.GetOrAdd($"{owner.AssemblyQualifiedName}|get|{name}", _ =>
            owner.GetProperty(name, Declared)?.GetMethod ?? throw new MissingMemberException(owner.Name, name));

    /// <summary>The setter, or init accessor, of the property <paramref name="name"/> <paramref name="owner"/> declares.</summary>
    private static MethodInfo Setter(Type owner, string name) =>
        (MethodInfo)Found.GetOrAdd($"{owner.AssemblyQualifiedName}|set|{name}", _ =>
            owner.GetProperty(name, Declared)?.SetMethod ?? throw new MissingMemberException(owner.Name, name));

    /// <summary>The method <paramref name="name"/> <paramref name="owner"/> declares taking <paramref name="parameters"/>.</summary>
    private static MethodInfo MethodOf(Type owner, string name, Type[] parameters) =>
        (MethodInfo)Found.GetOrAdd($"{owner.AssemblyQualifiedName}|method|{name}|{Signature(parameters)}", _ =>
            owner.GetMethod(name, Declared, parameters) ?? throw new MissingMemberException(owner.Name, name));

    /// <summary>A parameter list's key.</summary>
    private static string Signature(Type[] parameters) => string.Join(",", parameters.Select(parameter => parameter.AssemblyQualifiedName));
}
