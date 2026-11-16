namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The center Draygon carries Samus around in his spiral, and how a new spiral starts from where the last one left it
/// (#1763). Each spiral lifts its center a quarter pixel an update, and a grab starts the next spiral by setting the
/// center's whole position only: the vertical part of a pixel the previous spiral of this fight lifted it to carries
/// over. The updates on which a re-grab's spiral raises Draygon and the held Samus a pixel therefore depend on where the
/// last spiral stopped: a spiral starting from no part of a pixel, as the first of a fight does, rises a pixel on its
/// first update, while one starting a quarter pixel up waits an update longer.
/// </summary>
/// <remarks>
/// Started on the update Draygon grabs Samus (<see cref="StartSpiral"/>), during the enemy pass, and lifted on each
/// update of the spiral after Draygon and Samus are placed around it (<see cref="Lifted"/>); a value owned by Draygon's
/// state for his fight. Evidence: the original grab sets the spiral center's whole positions and leaves its vertical
/// part of a pixel as the last spiral left it, and the spiral subtracts the lift from both parts together; in the lowc
/// movie a second grab starts a quarter pixel up and Draygon's first rise waits an update, which a center reset to the
/// whole pixel missed (update 76,989). The Draygon grab check (<c>VerifyDraygonGrabSpiralFraction</c>) re-grabs with a
/// part of a pixel left over and finds it kept.
/// </remarks>
[CompatibilityService("a spiral after Draygon re-grabs Samus starts from the part of a pixel the last spiral lifted its center to, which decides the updates the pair rises a pixel on")]
public readonly record struct DraygonRegrabSpiralLift
{
    /// <summary>Stores the center; callers use <see cref="At"/>, <see cref="StartSpiral"/> or <see cref="Lifted"/>.</summary>
    private DraygonRegrabSpiralLift(FixedPosition<RoomPixels> center) => Center = center;

    /// <summary>The spiral's center, its vertical position carrying a part of a pixel; its horizontal position stays whole.</summary>
    public FixedPosition<RoomPixels> Center { get; }

    /// <summary>A center at <paramref name="center"/>, as a check states one.</summary>
    public static DraygonRegrabSpiralLift At(FixedPosition<RoomPixels> center) => new(center);

    /// <summary>
    /// The center a grab starts the next spiral from: at <paramref name="target"/>'s whole position, keeping the vertical
    /// part of a pixel this center was lifted to.
    /// </summary>
    /// <param name="target">The whole position the spiral is centered on.</param>
    public DraygonRegrabSpiralLift StartSpiral(Position16<RoomPixels> target) => new(Center.WithWhole(target));

    /// <summary>The center after one update's <paramref name="lift"/>, which carries into the whole position as it passes a pixel.</summary>
    /// <param name="lift">The vertical step, negative upward.</param>
    public DraygonRegrabSpiralLift Lifted(SignedSixteenSixteen lift) => new(Center.AdvanceY(lift));
}
