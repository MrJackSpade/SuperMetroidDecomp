namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>How the room lists the Mama Turtle's family (#2556): her four babies right after her.</summary>
internal static class MamaTurtlePartListingDefinitions
{
    /// <summary>The Mama Turtle, then her four babies.</summary>
    internal static EnemyComposite<EnemyDefinitionId> Listing { get; } = new([EnemyDefinitionId.MamaTurtle, .. DefinitionRun.Repeated(EnemyDefinitionId.BabyTurtle, 4)]);
}
