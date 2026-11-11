namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far off its target a charging Bull reckons its heading (#1607), in 256ths of a turn: the turn from the heading on
/// to Samus's bearing, counted the short way round when the bearing lies half a turn or more on from the heading in the
/// angles' order, but the long way round when it lies behind the heading by more than half a turn. A Bull heading at a
/// large angle with Samus's bearing just past zero therefore reckons itself far off target and starts braking, where the
/// true turn between them is small.
/// </summary>
/// <remarks>
/// Consulted on every accelerating update of a Bull's charge, after it moves, to decide whether it starts braking.
/// Evidence: the original subtracts the captured heading from the bearing as whole angle values and takes a negative
/// difference's magnitude as it stands, folding only a positive difference of a half turn or more back the short way.
/// </remarks>
[CompatibilityService("a charging Bull reckons the turn to Samus the long way round when her bearing lies more than half a turn behind its heading")]
public static class BullOffTargetTurn
{
    /// <summary>The turn, in 256ths of a turn, a Bull heading at <paramref name="heading"/> reckons to <paramref name="bearing"/>.</summary>
    public static int Of(TrigonometryAngle heading, TrigonometryAngle bearing)
    {
        int turn = bearing.RawValue - heading.RawValue;
        return turn >= TrigonometryAngle.HalfTurn.RawValue ? RotationAngle.StepsPerTurn - turn : Math.Abs(turn);
    }
}
