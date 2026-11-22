using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SuperMetroid.TestSupport.VerificationAccess.Tooling;

/// <summary>
/// The metadata test-support/VerificationAccess.Tooling's JSON calls serialize through (#2602): a call's own options, bound to the source-generated
/// <see cref="VerificationAccessToolingJsonContext"/>, so the options keep their naming, converters and layout and no type is
/// serialized by reflection. A type the context does not register fails as its metadata is asked for.
/// </summary>
internal static class VerificationAccessToolingJson
{
    /// <summary>Each caller's options, bound to the context, made the first time each is used.</summary>
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> Bound = [];

    /// <summary>The default options, bound to the context, for calls that pass none.</summary>
    private static readonly JsonSerializerOptions Default = new() { TypeInfoResolver = VerificationAccessToolingJsonContext.Default };

    /// <summary>The metadata of <typeparamref name="T"/> under <paramref name="options"/>, or the default options when null.</summary>
    /// <exception cref="NotSupportedException">The context does not register <typeparamref name="T"/>.</exception>
    internal static JsonTypeInfo<T> Info<T>(JsonSerializerOptions? options = null) =>
        (JsonTypeInfo<T>)BoundTo(options).GetTypeInfo(typeof(T));

    /// <summary>The metadata of <paramref name="type"/> under <paramref name="options"/>, or the default options when null.</summary>
    /// <exception cref="NotSupportedException">The context does not register <paramref name="type"/>.</exception>
    internal static JsonTypeInfo Info(Type type, JsonSerializerOptions? options) => BoundTo(options).GetTypeInfo(type);

    /// <summary><paramref name="options"/>' copy bound to the context, or the default options when null.</summary>
    private static JsonSerializerOptions BoundTo(JsonSerializerOptions? options) => options is null
        ? Default
        : Bound.GetValue(options, source => new JsonSerializerOptions(source) { TypeInfoResolver = VerificationAccessToolingJsonContext.Default });
}
