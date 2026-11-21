using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// The metadata the save codec's JSON reads and writes through (#2602): the codec's own options, bound to the
/// source-generated <see cref="SaveJsonContext"/>, so the options keep their naming, converters and layout. A type the
/// context does not register fails as its metadata is asked for.
/// </summary>
internal static partial class SaveJson
{
    /// <summary>Each set of options, bound to the context, made the first time it is used.</summary>
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> Bound = [];

    /// <summary>The default options, bound to the context, for reads and writes that pass none.</summary>
    private static readonly JsonSerializerOptions Default = new() { TypeInfoResolver = SaveJsonContext.Default };

    /// <summary>The metadata of <typeparamref name="T"/> under <paramref name="options"/>, or the default options when null.</summary>
    /// <exception cref="NotSupportedException">The context does not register <typeparamref name="T"/>.</exception>
    internal static JsonTypeInfo<T> Info<T>(JsonSerializerOptions? options = null) =>
        (JsonTypeInfo<T>)BoundTo(options).GetTypeInfo(typeof(T));

    /// <summary><paramref name="options"/>' copy bound to the context, or the default options when null.</summary>
    private static JsonSerializerOptions BoundTo(JsonSerializerOptions? options) => options is null
        ? Default
        : Bound.GetValue(options, source => new JsonSerializerOptions(source) { TypeInfoResolver = SaveJsonContext.Default });
}