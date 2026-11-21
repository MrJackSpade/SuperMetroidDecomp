using System.Text.Json.Serialization;

namespace SuperMetroid.Core;

/// <summary>
/// The source-generated JSON metadata of every type SuperMetroid.Core's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="CoreJson"/> binds a call's options to it.
/// </summary>
[JsonSerializable(typeof(global::SuperMetroid.Core.Input.ControllerButton), TypeInfoPropertyName = "SuperMetroid_Core_Input_ControllerButton")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Map.ExploredAreaJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Map_ExploredAreaJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Map.ExploredAreaJsonDocument[]), TypeInfoPropertyName = "SuperMetroid_Core_Map_ExploredAreaJsonDocumentA")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Map.MapCoordinateJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Map_MapCoordinateJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Map.MapCoordinateJsonDocument[]), TypeInfoPropertyName = "SuperMetroid_Core_Map_MapCoordinateJsonDocumentA")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Progression.AreaId), TypeInfoPropertyName = "SuperMetroid_Core_Progression_AreaId")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Progression.AreaId[]), TypeInfoPropertyName = "SuperMetroid_Core_Progression_AreaIdA")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Progression.DefeatedBosses), TypeInfoPropertyName = "SuperMetroid_Core_Progression_DefeatedBosses")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Progression.ReserveTankMode), TypeInfoPropertyName = "SuperMetroid_Core_Progression_ReserveTankMode")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Progression.SamusBeamFlags), TypeInfoPropertyName = "SuperMetroid_Core_Progression_SamusBeamFlags")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Progression.SamusEquipmentFlags), TypeInfoPropertyName = "SuperMetroid_Core_Progression_SamusEquipmentFlags")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Progression.SaveStartupRoute), TypeInfoPropertyName = "SuperMetroid_Core_Progression_SaveStartupRoute")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Samus.HudItem), TypeInfoPropertyName = "SuperMetroid_Core_Samus_HudItem")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.GameSaveJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Saves_GameSaveJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.GameSaveSlotJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Saves_GameSaveSlotJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.SaveAreaBossesJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Saves_SaveAreaBossesJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.SaveAreaBossesJsonDocument[]), TypeInfoPropertyName = "SuperMetroid_Core_Saves_SaveAreaBossesJsonDocumentA")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.SaveControllerJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Saves_SaveControllerJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.SaveFileHistory), TypeInfoPropertyName = "SuperMetroid_Core_Saves_SaveFileHistory")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.SaveGameTimeJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Saves_SaveGameTimeJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.SaveInventoryJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Saves_SaveInventoryJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.SaveProgressionJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Saves_SaveProgressionJsonDocument")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Saves.SaveResourcesJsonDocument), TypeInfoPropertyName = "SuperMetroid_Core_Saves_SaveResourcesJsonDocument")]
[JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, global::SuperMetroid.Core.Saves.GameSaveSlotJsonDocument>), TypeInfoPropertyName = "System_Collections_Generic_Dictionary_string_SuperMetroid_Core_Saves_GameSaveSlotJsonDocument")]
[JsonSerializable(typeof(string[]), TypeInfoPropertyName = "stringA")]
internal sealed partial class CoreJsonContext : JsonSerializerContext;
