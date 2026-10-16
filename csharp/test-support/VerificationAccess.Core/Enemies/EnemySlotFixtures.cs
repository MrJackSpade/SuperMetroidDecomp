using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>The species registry for slots a fixture builds outside any enemy system.</summary>
internal static class EnemySlotFixtures
{
    /// <summary>
    /// A registry whose unconverted species bind to a dispatch that has no enemy system: binding works, and any
    /// participation the fixture's slot is asked for fails loudly, since nothing can run it.
    /// </summary>
    internal static EnemySpeciesRegistry Registry { get; } = new(new StandaloneDispatch(), []);

    /// <summary>The dispatch of a fixture slot outside any enemy system; every use is a fixture defect.</summary>
    private sealed class StandaloneDispatch : IEnemyDispatch
    {
        /// <summary>The failure for any use.</summary>
        private static InvalidOperationException NoSystem() =>
            new("A standalone fixture slot has no enemy system to dispatch through.");

        /// <inheritdoc/>
        public void InitializeSlotFromDefinition(RoomEnemySlot slot, RoomEnemyPopulationRecord population, RoomEnemyDefinition definition) =>
            throw NoSystem();

        /// <inheritdoc/>
        public void RunInitializationAi(RoomEnemySlot slot, RoomLevelData? level = null, SamusState? samus = null,
            SnesButton controllerInput = SnesButton.None, ushort cameraX = 0) => throw NoSystem();

        /// <inheritdoc/>
        public void RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte nmiFrameCounter8) => throw NoSystem();

        /// <inheritdoc/>
        public EnemyAiTable Ai => throw NoSystem();

        /// <inheritdoc/>
        public IReadOnlyList<ushort> InteractiveEnemyIndexes => throw NoSystem();
    }
}
