using System.Text.Json.Serialization;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// The source-generated JSON metadata of every type SuperMetroid.ResourceAudit's documents and reports serialize (#2602), and of
/// every type they reach, each under its own name; <see cref="ResourceAuditJson"/> binds a call's options to it.
/// </summary>
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Artwork.PaletteRgb5), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Artwork_PaletteRgb5")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Artwork.PaletteRgb5[]), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Artwork_PaletteRgb5A")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Artwork.PaletteRgb5[][]), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Artwork_PaletteRgb5A_A")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Artwork.SpriteVisualPart), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Artwork_SpriteVisualPart")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Artwork.SpriteVisualPart[]), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Artwork_SpriteVisualPartA")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorDocument), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Bosses_MotherBrain_MotherBrainRoomColorDocument")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Enemies.EnemyExtendedFrameDocument), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Enemies_EnemyExtendedFrameDocument")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Enemies.EnemyExtendedVisualComponent), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Enemies_EnemyExtendedVisualComponent")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Enemies.EnemyExtendedVisualComponent[]), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Enemies_EnemyExtendedVisualComponentA")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Enemies.EnemyProjectilePartDocument), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Enemies_EnemyProjectilePartDocument")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Enemies.EnemyProjectilePartDocument[]), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Enemies_EnemyProjectilePartDocumentA")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Enemies.EnemyProjectileSpritemapDocument), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Enemies_EnemyProjectileSpritemapDocument")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.Frontend.Intro.Artwork.IntroDiscoveryActorSpriteDocument), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_Frontend_Intro_Artwork_IntroDiscoveryActorSpriteDocument")]
[JsonSerializable(typeof(global::SuperMetroid.AssetExtraction.World.Artwork.EnemySpritemapDocument), TypeInfoPropertyName = "SuperMetroid_AssetExtraction_World_Artwork_EnemySpritemapDocument")]
[JsonSerializable(typeof(global::SuperMetroid.ResourceAudit.Reports.AuditClassification), TypeInfoPropertyName = "SuperMetroid_ResourceAudit_Reports_AuditClassification")]
[JsonSerializable(typeof(global::SuperMetroid.ResourceAudit.Reports.AuditCompiledDefinition), TypeInfoPropertyName = "SuperMetroid_ResourceAudit_Reports_AuditCompiledDefinition")]
[JsonSerializable(typeof(global::SuperMetroid.ResourceAudit.Reports.AuditConsumer), TypeInfoPropertyName = "SuperMetroid_ResourceAudit_Reports_AuditConsumer")]
[JsonSerializable(typeof(global::SuperMetroid.ResourceAudit.Reports.AuditCoverage), TypeInfoPropertyName = "SuperMetroid_ResourceAudit_Reports_AuditCoverage")]
[JsonSerializable(typeof(global::SuperMetroid.ResourceAudit.Reports.AuditFinding), TypeInfoPropertyName = "SuperMetroid_ResourceAudit_Reports_AuditFinding")]
[JsonSerializable(typeof(global::SuperMetroid.ResourceAudit.Reports.AuditReport), TypeInfoPropertyName = "SuperMetroid_ResourceAudit_Reports_AuditReport")]
[JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, global::SuperMetroid.AssetExtraction.Artwork.SpriteVisualPart[]>), TypeInfoPropertyName = "System_Collections_Generic_Dictionary_string_SuperMetroid_AssetExtraction_Artwork_SpriteVisualPartA")]
[JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, global::SuperMetroid.AssetExtraction.Enemies.EnemyExtendedVisualComponent[]>), TypeInfoPropertyName = "System_Collections_Generic_Dictionary_string_SuperMetroid_AssetExtraction_Enemies_EnemyExtendedVisualComponentA")]
[JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, global::SuperMetroid.AssetExtraction.Enemies.EnemyProjectilePartDocument[]>), TypeInfoPropertyName = "System_Collections_Generic_Dictionary_string_SuperMetroid_AssetExtraction_Enemies_EnemyProjectilePartDocumentA")]
[JsonSerializable(typeof(global::System.Collections.Generic.Dictionary<string, string>), TypeInfoPropertyName = "System_Collections_Generic_Dictionary_string_string")]
[JsonSerializable(typeof(global::System.Collections.Generic.List<global::SuperMetroid.ResourceAudit.Reports.AuditClassification>), TypeInfoPropertyName = "System_Collections_Generic_List_SuperMetroid_ResourceAudit_Reports_AuditClassification")]
[JsonSerializable(typeof(global::System.Collections.Generic.List<global::SuperMetroid.ResourceAudit.Reports.AuditCompiledDefinition>), TypeInfoPropertyName = "System_Collections_Generic_List_SuperMetroid_ResourceAudit_Reports_AuditCompiledDefinition")]
[JsonSerializable(typeof(global::System.Collections.Generic.List<global::SuperMetroid.ResourceAudit.Reports.AuditConsumer>), TypeInfoPropertyName = "System_Collections_Generic_List_SuperMetroid_ResourceAudit_Reports_AuditConsumer")]
[JsonSerializable(typeof(global::System.Collections.Generic.List<global::SuperMetroid.ResourceAudit.Reports.AuditCoverage>), TypeInfoPropertyName = "System_Collections_Generic_List_SuperMetroid_ResourceAudit_Reports_AuditCoverage")]
[JsonSerializable(typeof(global::System.Collections.Generic.List<global::SuperMetroid.ResourceAudit.Reports.AuditFinding>), TypeInfoPropertyName = "System_Collections_Generic_List_SuperMetroid_ResourceAudit_Reports_AuditFinding")]
internal sealed partial class ResourceAuditJsonContext : JsonSerializerContext;
