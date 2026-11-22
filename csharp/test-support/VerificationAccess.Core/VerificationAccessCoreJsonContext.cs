using System.Text.Json.Serialization;

namespace SuperMetroid.TestSupport.VerificationAccess.Core;

/// <summary>
/// The source-generated JSON metadata of every type test-support/VerificationAccess.Core's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="VerificationAccessCoreJson"/> binds a call's options to it.
/// </summary>
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Fx.RoomFxLayer3TilemapDocument), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Fx_RoomFxLayer3TilemapDocument")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Rooms.RoomBackgroundTilemapCell), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Rooms_RoomBackgroundTilemapCell")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Rooms.RoomBackgroundTilemapCell[]), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Rooms_RoomBackgroundTilemapCellA")]
[JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, global::SuperMetroid.AssetExtraction.Rooms.RoomBackgroundTilemapCell[]>), TypeInfoPropertyName = "System_Collections_Generic_Dictionary_string_SuperMetroid_AssetExtraction_Rooms_RoomBackgroundTilemapCellA")]
internal sealed partial class VerificationAccessCoreJsonContext : JsonSerializerContext;
