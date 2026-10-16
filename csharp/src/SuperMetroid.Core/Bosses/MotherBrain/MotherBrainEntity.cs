using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One MotherBrain occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MotherBrainEntity(MotherBrainBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyBg2Frames
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.MotherBrainBody => species.RunMotherBrainBodyInitializationAi,
        EnemyDefinitionId.MotherBrainHead => species.RunMotherBrainHeadInitializationAi,
        EnemyDefinitionId.MotherBrainTubes => species.RunMotherBrainTubesInitializationAi,
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.MotherBrainBody => species.RunMotherBrainBodyMainAi,
        EnemyDefinitionId.MotherBrainHead => species.RunMotherBrainHeadMainAi,
        EnemyDefinitionId.MotherBrainTubes => species.RunMotherBrainTubesMainAi,
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.MotherBrainTubes => species.AdvanceMotherBrainTubesTypedAnimation,
        EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead => species.AdvanceMotherBrainBodyTypedAnimation,
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback) => MotherBrainBehavior.FindMotherBrainHitboxCallback(Slot, probe, out callback);

    /// <inheritdoc/>
    public EnemyBg2FrameLookup? FindBg2Frame(ushort selected, EnemyRoomBinding room)
    {
        if (Slot.EnemyDefinitionPointer != EnemyDefinitionId.MotherBrainBody ||
            Slot.Definition.Bank != MotherBrainBodyVisualDefinitions.Bank || !MotherBrainBodyVisualDefinitions.HasBg2(selected))
            return null;
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes = default;
        bool found = room.InstalledArtwork.MotherBrainBodyBg2Frames?.TryGet(selected, out writes) == true;
        return new();
    }
}
