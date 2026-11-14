namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' work that steps at the start of each update, before the enemy pass selects the enemies it processes
/// (#1851): its order against the selection is observable, so the processing window runs it first.
/// </summary>
internal interface IStepBeforeEnemySelection
{
    /// <summary>Steps the species' work for this update, before the enemies to process are selected.</summary>
    void StepBeforeEnemySelection();
}
