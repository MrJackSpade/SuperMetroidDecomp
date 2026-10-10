using System.Collections;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Hardware;

// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>
/// Restores fields whose 15-bit color words became <see cref="Bgr555"/> (#627). A saved field
/// qualifies only when its type is exactly the current field type with every <see cref="Bgr555"/>
/// replaced by <see cref="ushort"/> (scalars, nullables, arrays, tuples and generic collections);
/// every word decodes through <see cref="Bgr555.FromWord"/>, so a set bit 15 is rejected.
/// </summary>
internal static class DebuggerColorWordShapes
{
    /// <summary>True when <paramref name="savedType"/> is the word-shaped predecessor of <paramref name="fieldType"/>.</summary>
    internal static bool Applies(Type fieldType, Type savedType) =>
        fieldType != savedType && WordShape(fieldType) == savedType;

    /// <summary>Converts a word-shaped payload to <paramref name="target"/>.</summary>
    internal static object Convert(object value, Type target)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.GetType() == target)
            return value;
        Type? nullable = Nullable.GetUnderlyingType(target);
        if (target == typeof(Bgr555) || nullable == typeof(Bgr555))
            return value is ushort word
                ? Bgr555.FromWord(word)
                : throw new InvalidDataException($"Saved {value.GetType().FullName} is not a color word.");
        if (target.IsArray)
            return ConvertArray((Array)value, target);
        if (value is ITuple tuple && target.IsGenericType)
        {
            Type[] items = target.GetGenericArguments();
            var converted = new object?[tuple.Length];
            for (int index = 0; index < tuple.Length; index++)
                converted[index] = ConvertItem(tuple[index], items[index]);
            return Activator.CreateInstance(target, converted)!;
        }
        if (value is IDictionary dictionary && target.IsGenericType)
        {
            Type[] arguments = target.GetGenericArguments();
            var result = (IDictionary)Activator.CreateInstance(target)!;
            foreach (DictionaryEntry entry in dictionary)
                result.Add(ConvertItem(entry.Key, arguments[0])!, ConvertItem(entry.Value, arguments[1]));
            return result;
        }
        if (value is IList list && target.IsGenericType)
        {
            Type element = target.GetGenericArguments()[0];
            var result = (IList)Activator.CreateInstance(target)!;
            foreach (object? item in list)
                result.Add(ConvertItem(item, element));
            return result;
        }
        throw new InvalidDataException($"Saved {value.GetType().FullName} has no color-word conversion to {target.FullName}.");
    }

    private static object? ConvertItem(object? value, Type target) =>
        value is null ? null : Convert(value, target);

    private static Array ConvertArray(Array source, Type target)
    {
        Type element = target.GetElementType()!;
        if (source.Rank != 1)
            throw new InvalidDataException($"Saved multi-dimensional {source.GetType().FullName} has no color-word conversion.");
        Array result = Array.CreateInstance(element, source.Length);
        for (int index = 0; index < source.Length; index++)
            result.SetValue(ConvertItem(source.GetValue(index), element), index);
        return result;
    }

    private static Type WordShape(Type type)
    {
        if (type == typeof(Bgr555))
            return typeof(ushort);
        if (type.IsArray)
        {
            Type element = WordShape(type.GetElementType()!);
            return type.GetArrayRank() == 1 && type == type.GetElementType()!.MakeArrayType()
                ? element.MakeArrayType()
                : element.MakeArrayType(type.GetArrayRank());
        }
        if (type.IsGenericType)
        {
            Type[] arguments = type.GetGenericArguments();
            Type[] shaped = Array.ConvertAll(arguments, WordShape);
            return shaped.SequenceEqual(arguments) ? type : type.GetGenericTypeDefinition().MakeGenericType(shaped);
        }
        return type;
    }
}
