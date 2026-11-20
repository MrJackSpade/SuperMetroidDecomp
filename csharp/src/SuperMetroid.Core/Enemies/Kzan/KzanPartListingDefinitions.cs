namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>How the room lists a Kzan's parts (#2556): its bottom right after its top, in the Wasteland and the Wrecked Ship's main shaft alike.</summary>
internal static class KzanPartListingDefinitions
{
    /// <summary>The top, then the bottom.</summary>
    internal static EnemyComposite<EnemyDefinitionId> Listing { get; } = new([EnemyDefinitionId.KzanTop, EnemyDefinitionId.KzanBottom]);
}
