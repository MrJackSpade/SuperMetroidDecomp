using SuperMetroid.Core.Hardware;

// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>
/// Rebuilds spatial values (#628) from the retired per-component words an older debugger state
/// still names. The retired words are drained by <see cref="DebuggerRetiredFieldDefinitions"/>;
/// each introduction of the merged field reads them back here. A capture that names only some of
/// a value's components is rejected rather than completed with invented words.
/// </summary>
internal static class DebuggerSpatialFieldMerges
{
    /// <summary>The auto-property backing-field name of <paramref name="property"/>.</summary>
    internal static string Backing(string property) => $"<{property}>k__BackingField";

    /// <summary>A retired word of <paramref name="instance"/>; its absence is a layout mismatch.</summary>
    internal static ushort Word(object instance, string property) =>
        DebuggerRetiredFieldDefinitions.TryGetLegacyWord(instance, Backing(property), out ushort value)
            ? value
            : throw new InvalidDataException(
                $"Legacy {instance.GetType().FullName} lacks {property}, which its merged spatial value requires.");

    /// <summary>A retired signed word of <paramref name="instance"/>; its absence is a layout mismatch.</summary>
    internal static short SignedWord(object instance, string property) =>
        DebuggerRetiredFieldDefinitions.TryGetLegacyValue(instance, Backing(property), out object? value) && value is short word
            ? word
            : throw new InvalidDataException(
                $"Legacy {instance.GetType().FullName} lacks signed word {property}, which its merged spatial value requires.");

    /// <summary>A whole-pixel position from the two retired axis words.</summary>
    internal static Position16<TSpace> Position<TSpace>(object instance, string x, string y)
        where TSpace : ISpatialSpace =>
        new(Word(instance, x), Word(instance, y));

    /// <summary>A signed 8.8 velocity from the two retired signed axis words.</summary>
    internal static Vector88<TSpace> Vector88<TSpace>(object instance, string x, string y)
        where TSpace : ISpatialSpace =>
        new(SignedWord(instance, x), SignedWord(instance, y));

    /// <summary>A 16.16 position from the four retired whole/subpixel words.</summary>
    internal static FixedPosition<TSpace> FixedPosition<TSpace>(
        object instance, string xWhole, string xFraction, string yWhole, string yFraction)
        where TSpace : ISpatialSpace =>
        new(new SnesFixedPosition(Word(instance, xWhole), Word(instance, xFraction)),
            new SnesFixedPosition(Word(instance, yWhole), Word(instance, yFraction)));

    /// <summary>A signed 16.16 displacement from the four retired whole/fraction words.</summary>
    internal static FixedVector<TSpace> FixedVector<TSpace>(
        object instance, string xWhole, string xFraction, string yWhole, string yFraction)
        where TSpace : ISpatialSpace =>
        Core.Spatial.FixedVector<TSpace>.FromWords(
            Word(instance, xWhole), Word(instance, xFraction), Word(instance, yWhole), Word(instance, yFraction));

    /// <summary>An extent from the two retired axis words.</summary>
    internal static Extent16<TSpace> Extent<TSpace>(object instance, string x, string y)
        where TSpace : ISpatialSpace =>
        new(Word(instance, x), Word(instance, y));
}
