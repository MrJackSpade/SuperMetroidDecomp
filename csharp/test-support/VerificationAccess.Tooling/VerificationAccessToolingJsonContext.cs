using System.Text.Json.Serialization;

namespace SuperMetroid.TestSupport.VerificationAccess.Tooling;

/// <summary>
/// The source-generated JSON metadata of every type test-support/VerificationAccess.Tooling's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="VerificationAccessToolingJson"/> binds a call's options to it.
/// </summary>
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Artwork.SpriteVisualPart), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Artwork_SpriteVisualPart")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Artwork.SpriteVisualPart[]), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Artwork_SpriteVisualPartA")]
[JsonSerializable(typeof(int[]), TypeInfoPropertyName = "intA")]
internal sealed partial class VerificationAccessToolingJsonContext : JsonSerializerContext;
