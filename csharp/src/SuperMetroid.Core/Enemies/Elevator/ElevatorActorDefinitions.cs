using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Authored definitions of the elevator actor.</summary>
public static partial class ElevatorActorDefinitions
{
    /// <summary>How far above the platform Samus's center rides.</summary>
    public const int SamusYOffset = 26;

    /// <summary>How far a travelling elevator moves each update: one and a half pixels.</summary>
    internal static readonly SignedSixteenSixteen TravelSpeed = SignedSixteenSixteen.FromPixelFraction(3, 2);

    /// <summary>The newly pressed input that begins each direction's travel.</summary>
    internal static readonly Dictionary<VerticalDirection, GameplayAction> RequiredDirectionInputs = new()
    {
        [VerticalDirection.Down] = GameplayAction.Down,
        [VerticalDirection.Up] = GameplayAction.Up,
    };
}
