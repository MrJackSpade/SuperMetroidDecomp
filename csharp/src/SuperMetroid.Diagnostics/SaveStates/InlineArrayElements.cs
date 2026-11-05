using SuperMetroid.Core;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The elements of an <see cref="InlineArrayAttribute"/> struct, which declares one field but stores its attribute's
/// length of elements; field reflection alone would see only the first.
/// </summary>
internal static class InlineArrayElements
{
    /// <summary>
    /// The element count of an inline-array struct, or null for any other type. A capture asks this of every struct it
    /// writes, so each type's answer is read from its attributes once.
    /// </summary>
    internal static int? Length(Type type) =>
        type.IsValueType ? lengths.GetOrAdd(type, ReadLength) : null;

    /// <summary>The element count <paramref name="type"/>'s inline-array attribute declares, or null without one.</summary>
    [ReflectionAudit("debugger state capture recognises an inline-array struct by its attribute")]
    private static int? ReadLength(Type type) =>
        type.GetCustomAttribute<InlineArrayAttribute>() is { } inline ? inline.Length : null;

    /// <summary>Each struct type's inline-array length, or null for a struct that is not one, read once.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, int?> lengths = new();

    /// <summary>
    /// <see cref="ReadTyped"/> bound to each buffer type and its element type, bound once as a delegate, so a capture
    /// calls it directly rather than invoking it by reflection per buffer (#1532).
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object, int, object?[]>> readers = new();

    /// <summary><see cref="CreateTyped"/> bound to each buffer type and its element type, bound once as a delegate (#1532).</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object?[], int, object>> creators = new();

    /// <summary>Every element of the boxed inline array <paramref name="buffer"/>, in order.</summary>
    [ReflectionAudit("debugger state capture reads every element of an inline-array struct through its element type")]
    internal static object?[] Read(object buffer)
    {
        Type type = buffer.GetType();
        Func<object, int, object?[]> reader = readers.GetOrAdd(type,
            static buffer => Bind(nameof(ReadTyped), buffer).CreateDelegate<Func<object, int, object?[]>>());
        return reader(buffer, Length(type) ?? throw NotInline(type));
    }

    /// <summary>A boxed inline array of <paramref name="type"/> holding <paramref name="elements"/> in order.</summary>
    [ReflectionAudit("debugger state restore rebuilds an inline-array struct element by element")]
    internal static object Create(Type type, object?[] elements)
    {
        int length = Length(type) ?? throw NotInline(type);
        if (elements.Length != length)
            throw new InvalidDataException($"The saved {type.FullName} holds {elements.Length} elements; this build holds {length}.");
        Func<object?[], int, object> creator = creators.GetOrAdd(type,
            static buffer => Bind(nameof(CreateTyped), buffer).CreateDelegate<Func<object?[], int, object>>());
        return creator(elements, length);
    }

    /// <summary>The typed helper <paramref name="helper"/> bound to <paramref name="type"/> and its element type.</summary>
    [ReflectionAudit("debugger state binds the typed inline-array helpers to the buffer's own element type")]
    private static MethodInfo Bind(string helper, Type type)
    {
        Type element = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Single().FieldType;
        MethodInfo method = typeof(InlineArrayElements).GetMethod(helper, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(InlineArrayElements), helper);
        return method.MakeGenericMethod(type, element);
    }

    /// <summary>Copies each element of a boxed buffer out as an object.</summary>
    private static object?[] ReadTyped<TBuffer, TElement>(object buffer, int length) where TBuffer : struct
    {
        TBuffer value = (TBuffer)buffer;
        ReadOnlySpan<TElement> elements = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TBuffer, TElement>(ref value), length);
        var copies = new object?[length];
        for (int index = 0; index < length; index++)
            copies[index] = elements[index];
        return copies;
    }

    /// <summary>Builds a buffer from objects of its element type.</summary>
    private static object CreateTyped<TBuffer, TElement>(object?[] values, int length) where TBuffer : struct
    {
        TBuffer value = default;
        Span<TElement> elements = MemoryMarshal.CreateSpan(ref Unsafe.As<TBuffer, TElement>(ref value), length);
        for (int index = 0; index < length; index++)
        {
            elements[index] = values[index] is TElement element
                ? element
                : throw new InvalidDataException($"Element {index} of the saved {typeof(TBuffer).FullName} is not a {typeof(TElement).FullName}.");
        }
        return value;
    }

    /// <summary>The failure for a type that is not an inline array.</summary>
    private static InvalidOperationException NotInline(Type type) => new($"{type.FullName} is not an inline array.");
}
