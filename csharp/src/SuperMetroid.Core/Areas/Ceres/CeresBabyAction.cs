using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres Baby Metroid's behaviour while Ridley holds, drops or retrieves it (#2571): each action moves the Baby itself
/// each update. The Baby starts in his arms (#2228); Ridley lets it fall through <see cref="CeresBaby.Drop"/>, and its own
/// update then sets up its fall and falls on.
/// </summary>
public sealed class CeresBabyAction
{
    /// <summary>The action's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Moves the Baby one update, given Ridley's body, which carries it.</summary>
    private readonly Action<RoomEnemyPlace, CeresBaby> _move;

    /// <summary>An action named <paramref name="name"/> that moves the Baby by <paramref name="move"/>.</summary>
    /// <param name="name">The action's name, for diagnostics.</param>
    /// <param name="move">Moves the Baby one update, given Ridley's body.</param>
    private CeresBabyAction(string name, Action<RoomEnemyPlace, CeresBaby> move)
    {
        _name = name;
        _move = move;
    }

    /// <summary>The Baby stays at Ridley's arm anchor.</summary>
    public static CeresBabyAction CarriedInArms { get; } = new(nameof(CarriedInArms), (place, baby) =>
        // Until the fake retreat, Ridley carries the Baby at his left-hand anchor, so it is where he holds it when he lets
        // it fall.
        baby.Position = baby.Position.WithWhole(place.Position.Whole + CeresRidleyArenaDefinitions.BabyArmsAnchor));

    /// <summary>The Baby follows Ridley's grasp point.</summary>
    public static CeresBabyAction CarriedInFeet { get; } = new(nameof(CarriedInFeet), (place, baby) =>
        // Once the hand rectangles overlap, the Baby follows the grasp point for the real retreat and disappears with
        // Ridley at the escape handoff.
        baby.Position = baby.Position.WithWhole(place.Position.Whole + CeresRidleyArenaDefinitions.BabyFeetAnchor));

    /// <summary>
    /// Ridley has just let the Baby go: its update starts its fall from rest at a whole pixel and falls its first step in
    /// the same update.
    /// </summary>
    public static CeresBabyAction Dropping { get; } = new(nameof(Dropping), (place, baby) =>
    {
        baby.Position = baby.Position.WithFractionY(PixelFraction.None);
        baby.VerticalVelocity = SignedEightEight.Zero;
        baby.Action = Dropped;
        Dropped.Move(place, baby);
    });

    /// <summary>The Baby falls under gravity until it lands.</summary>
    public static CeresBabyAction Dropped { get; } = new(nameof(Dropped), (_, baby) =>
    {
        // The falling speed only grows from zero and the fall ends at the Baby's floor, so it stays a few pixels an update.
        baby.VerticalVelocity += CeresRidleyArenaDefinitions.BabyFallGravity;
        baby.Position = baby.Position.WithY(baby.Position.Y.MovedBy(baby.VerticalVelocity));
        if (baby.Position.Y.Whole >= CeresRidleyArenaDefinitions.BabyFloorY)
        {
            baby.Position = baby.Position.WithWholeY(CeresRidleyArenaDefinitions.BabyFloorY);
            baby.Action = Idle;
        }
    });

    /// <summary>The idle action after landing: the Baby stays where it landed.</summary>
    public static CeresBabyAction Idle { get; } = new(nameof(Idle), (_, _) => { });

    /// <summary>Moves the Baby one update.</summary>
    /// <param name="place">Ridley's body, which carries the Baby.</param>
    /// <param name="baby">The Baby.</param>
    internal void Move(RoomEnemyPlace place, CeresBaby baby) => _move(place, baby);

    /// <summary>Names the action.</summary>
    public override string ToString() => _name;
}
