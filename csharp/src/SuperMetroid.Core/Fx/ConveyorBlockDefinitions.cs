using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Fx;

/// <summary>The conveyor blocks among the special-air reactions, and how far they carry Samus each frame.</summary>
internal static partial class ConveyorBlockDefinitions
{
    /// <summary>How far a rightward conveyor carries Samus each frame: two pixels right.</summary>
    public static readonly SignedSixteenSixteen RightwardCarry = SignedSixteenSixteen.FromWholePixels(2);

    /// <summary>How far a leftward conveyor carries Samus each frame: two pixels left.</summary>
    public static readonly SignedSixteenSixteen LeftwardCarry = SignedSixteenSixteen.FromWholePixels(-2);

    /// <summary>The conveyor each conveyor reaction of a special-air block selects; the other reactions carry nothing.</summary>
    public static readonly IReadOnlyDictionary<SpecialAirReaction, ConveyorBlock> Conveyors = new Dictionary<SpecialAirReaction, ConveyorBlock>
    {
        [SpecialAirReaction.RightwardConveyorOnFoot] = new(HorizontalDirection.Right, ConveyorCarryCondition.OnFootWithPower),
        [SpecialAirReaction.LeftwardConveyorOnFoot] = new(HorizontalDirection.Left, ConveyorCarryCondition.OnFootWithPower),
        [SpecialAirReaction.RightwardConveyor] = new(HorizontalDirection.Right, ConveyorCarryCondition.Always),
        [SpecialAirReaction.LeftwardConveyor] = new(HorizontalDirection.Left, ConveyorCarryCondition.Always),
    };
}
