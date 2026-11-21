using System.Text.Json.Serialization;

namespace SuperMetroid.DebugRunner;

/// <summary>
/// The source-generated JSON metadata of every type SuperMetroid.DebugRunner's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="DebugRunnerJson"/> binds a call's options to it.
/// </summary>
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Audio.AudioBank), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Audio_AudioBank")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Audio.MusicTrackShape), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Audio_MusicTrackShape")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Audio.SeamProfile), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Audio_SeamProfile")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Audio.SoundEffectStateScan), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Audio_SoundEffectStateScan")]
[JsonSerializable(typeof(global::SuperMetroid.Core.Audio.SoundEffectLibrary), TypeInfoPropertyName = "SuperMetroid_Core_Audio_SoundEffectLibrary")]
[JsonSerializable(typeof(global::SuperMetroid.DebugRunner.Assets.AssetCommands.AssetRecord), TypeInfoPropertyName = "SuperMetroid_DebugRunner_Assets_AssetCommands_AssetRecord")]
[JsonSerializable(typeof(global::SuperMetroid.DebugRunner.Tools.AudioClipInventory.InventoryDocument), TypeInfoPropertyName = "SuperMetroid_DebugRunner_Tools_AudioClipInventory_InventoryDocument")]
[JsonSerializable(typeof(global::SuperMetroid.DebugRunner.Tools.AudioClipInventory.MusicRow), TypeInfoPropertyName = "SuperMetroid_DebugRunner_Tools_AudioClipInventory_MusicRow")]
[JsonSerializable(typeof(global::SuperMetroid.DebugRunner.Tools.AudioClipInventory.PhaseTiming), TypeInfoPropertyName = "SuperMetroid_DebugRunner_Tools_AudioClipInventory_PhaseTiming")]
[JsonSerializable(typeof(global::SuperMetroid.DebugRunner.Tools.AudioClipInventory.SoundEffectRow), TypeInfoPropertyName = "SuperMetroid_DebugRunner_Tools_AudioClipInventory_SoundEffectRow")]
[JsonSerializable(typeof(global::System.Collections.Generic.IReadOnlyList<byte>), TypeInfoPropertyName = "System_Collections_Generic_IReadOnlyList_byte")]
[JsonSerializable(typeof(global::System.Collections.Generic.IReadOnlyList<global::SuperMetroid.AssetExtraction.Audio.SoundEffectStateScan>), TypeInfoPropertyName = "System_Collections_Generic_IReadOnlyList_SuperMetroid_AssetExtraction_Audio_SoundEffectStateScan")]
[JsonSerializable(typeof(global::System.Collections.Generic.IReadOnlyList<global::SuperMetroid.DebugRunner.Tools.AudioClipInventory.MusicRow>), TypeInfoPropertyName = "System_Collections_Generic_IReadOnlyList_SuperMetroid_DebugRunner_Tools_AudioClipInventory_MusicRow")]
[JsonSerializable(typeof(global::System.Collections.Generic.IReadOnlyList<global::SuperMetroid.DebugRunner.Tools.AudioClipInventory.PhaseTiming>), TypeInfoPropertyName = "System_Collections_Generic_IReadOnlyList_SuperMetroid_DebugRunner_Tools_AudioClipInventory_PhaseTiming")]
[JsonSerializable(typeof(global::System.Collections.Generic.IReadOnlyList<global::SuperMetroid.DebugRunner.Tools.AudioClipInventory.SoundEffectRow>), TypeInfoPropertyName = "System_Collections_Generic_IReadOnlyList_SuperMetroid_DebugRunner_Tools_AudioClipInventory_SoundEffectRow")]
[JsonSerializable(typeof(global::System.Collections.Generic.IReadOnlyList<long>), TypeInfoPropertyName = "System_Collections_Generic_IReadOnlyList_long")]
[JsonSerializable(typeof(global::System.Collections.Generic.IReadOnlyList<string>), TypeInfoPropertyName = "System_Collections_Generic_IReadOnlyList_string")]
[JsonSerializable(typeof(global::System.Collections.Generic.List<global::SuperMetroid.DebugRunner.Assets.AssetCommands.AssetRecord>), TypeInfoPropertyName = "System_Collections_Generic_List_SuperMetroid_DebugRunner_Assets_AssetCommands_AssetRecord")]
[JsonSerializable(typeof(global::System.Collections.Generic.SortedDictionary<string, object>), TypeInfoPropertyName = "System_Collections_Generic_SortedDictionary_string_object")]
[JsonSerializable(typeof(object), TypeInfoPropertyName = "object")]
internal sealed partial class DebugRunnerJsonContext : JsonSerializerContext;
