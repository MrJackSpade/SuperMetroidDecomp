using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Fx;

/// <summary>The conveyor blocks among the special-air reactions, and how far they carry Samus each frame.</summary>
internal static class ConveyorBlockDefinitions
{
    /// <summary>The whole pixels a rightward conveyor carries Samus each frame.</summary>
    public const int RightwardCarry = 2;

    /// <summary>The whole pixels a leftward conveyor carries Samus each frame.</summary>
    public const int LeftwardCarry = -2;

    /// <summary>The conveyor each conveyor reaction of a special-air block selects; the other reactions carry nothing.</summary>
    public static readonly IReadOnlyDictionary<SpecialAirReaction, ConveyorBlock> Conveyors = new Dictionary<SpecialAirReaction, ConveyorBlock>
    {
        [SpecialAirReaction.RightwardConveyorOnFoot] = new(HorizontalDirection.Right, ConveyorCarryCondition.OnFootWithPower),
        [SpecialAirReaction.LeftwardConveyorOnFoot] = new(HorizontalDirection.Left, ConveyorCarryCondition.OnFootWithPower),
        [SpecialAirReaction.RightwardConveyor] = new(HorizontalDirection.Right, ConveyorCarryCondition.Always),
        [SpecialAirReaction.LeftwardConveyor] = new(HorizontalDirection.Left, ConveyorCarryCondition.Always),
    };
}
