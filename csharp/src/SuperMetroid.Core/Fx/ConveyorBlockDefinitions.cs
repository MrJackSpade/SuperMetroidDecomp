using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Fx;

/// <summary>The conveyor blocks among the common special-air reactions, and how far they carry Samus each frame.</summary>
internal static class ConveyorBlockDefinitions
{
    /// <summary>The whole pixels a rightward conveyor carries Samus each frame.</summary>
    public const int RightwardCarry = 2;

    /// <summary>The whole pixels a leftward conveyor carries Samus each frame.</summary>
    public const int LeftwardCarry = -2;

    /// <summary>The conveyor each conveyor reaction of a special-air block selects; the other reactions carry nothing.</summary>
    public static readonly IReadOnlyDictionary<BlockReaction, ConveyorBlock> Conveyors = new Dictionary<BlockReaction, ConveyorBlock>
    {
        [BlockReaction.Common(8)] = new(HorizontalDirection.Right, ConveyorCarryCondition.OnFootWithPower),
        [BlockReaction.Common(9)] = new(HorizontalDirection.Left, ConveyorCarryCondition.OnFootWithPower),
        [BlockReaction.Common(10)] = new(HorizontalDirection.Right, ConveyorCarryCondition.Always),
        [BlockReaction.Common(11)] = new(HorizontalDirection.Left, ConveyorCarryCondition.Always),
    };
}
