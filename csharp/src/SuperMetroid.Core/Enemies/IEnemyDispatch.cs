using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The coordinator services a species uses to bring an enemy it allocated to life, or to run another
/// enemy's AI on its behalf. Everything else a species needs is injected as its own collaborator.
/// </summary>
internal interface IEnemyDispatch
{
    /// <summary>Installs a population record and definition into a free slot, as native <c>SpawnEnemy</c> does.</summary>
    void InitializeSlotFromDefinition(RoomEnemySlot slot, RoomEnemyPopulationRecord population, RoomEnemyDefinition definition);

    /// <summary>
    /// Runs the slot's initialization AI. A population load supplies its live room, Samus, controller and
    /// camera; a species spawning on another enemy's behalf runs it without that context.
    /// </summary>
    void RunInitializationAi(RoomEnemySlot slot, RoomLevelData? level = null, SamusState? samus = null,
        ushort controllerInput = 0, ushort cameraX = 0);

    /// <summary>Runs the slot's main AI inside the supplied frame.</summary>
    void RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte nmiFrameCounter8);

    /// <summary>The registered AI routines and touch, shot, bomb and power-bomb reactions.</summary>
    EnemyAiTable Ai { get; }

    /// <summary>Native indexes of the enemies admitted to interaction this frame, in processing order.</summary>
    IReadOnlyList<ushort> InteractiveEnemyIndexes { get; }
}
