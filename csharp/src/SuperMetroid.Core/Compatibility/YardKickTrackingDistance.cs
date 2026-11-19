namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The sideways speed a Yard kicked by Samus flies off with (#1962): not her own speed, but how far the camera last
/// measured her moving sideways as it tracked her, plus one pixel, fraction and all. The camera measures the distance
/// between her position at its previous tracking and now, compared by whole pixels for its direction, and adds a pixel,
/// so a Samus standing still kicks a shell one pixel an update and a fractional step can kick it at less than a pixel; a
/// fast grapple swing tracks without measuring and leaves the last measure standing. The kick's upward speed is looked
/// up from the same measure's whole pixels, so both halves of the flight depend on it.
/// </summary>
/// <remarks>
/// <para>
/// Recorded by the update's main scrolling, once the camera has tracked Samus on both axes, whenever the scrolling runs
/// (not while the death sequence owns Samus or an elevator's door transition holds the camera); read when a Yard's touch
/// kicks it, during the enemy pass, which runs before the update's scrolling, so a kick takes the measure of the update
/// before. Created with each gameplay runtime, whose rooms and door transitions keep it, and handed by its room loader to
/// every room's enemies; a runtime starts with no measure, and each new Samus's initialisation records one pixel
/// (<see cref="RecordSamusInitialization"/>), so a kick before the camera has tracked a new Samus flies at one pixel an
/// update. The camera's vertical measure reaches a launched shot through <see cref="ProjectileLaunchInheritance"/>.
/// </para>
/// <para>
/// Evidence: the Yard's kick (<c>$A3:D4C7</c>-<c>$A3:D4E0</c>) stores the camera's horizontal speed (<c>$0DA2</c>,
/// <c>$0DA4</c>), which Samus's horizontal scrolling writes as the distance moved plus one (<c>$90:96C0</c>), as the
/// shell's sideways speed and indexes its upward speed table with the whole pixels; Samus's initialisation
/// (<c>$91:E0E7</c>-<c>$91:E0F2</c>) stores one pixel there, from game-data loading (<c>$82:8057</c>), demo-data loading
/// (<c>$82:8014</c>) and the intro; the Yard kick check compares the
/// kick's speeds with the original's table through the room's contact pass, and the kick distance check follows a
/// runtime's scrolling into a loaded room's kick.
/// </para>
/// </remarks>
[CompatibilityService("a Yard kicked by Samus flies sideways at the distance the camera last measured her moving, plus one pixel, and upward at the speed that distance's whole pixels select")]
public sealed class YardKickTrackingDistance
{
    /// <summary>
    /// How far the camera last measured Samus moving sideways, plus one pixel: the kicked shell's sideways speed in pixels
    /// an update before its facing is applied. One pixel from Samus's initialisation until the camera next tracks her,
    /// and zero before the runtime's first Samus.
    /// </summary>
    public SignedSixteenSixteen HorizontalDistancePlusOne { get; private set; }

    /// <summary>The measure Samus's initialisation leaves before the camera has tracked her: no movement, plus one pixel.</summary>
    private static SignedSixteenSixteen InitialDistancePlusOne => SignedSixteenSixteen.OnePixel;

    /// <summary>Records the camera's horizontal measure of Samus's movement, plus one pixel, as the scrolling leaves it.</summary>
    /// <param name="distancePlusOne">The distance the camera measured, plus one pixel.</param>
    public void Record(SignedSixteenSixteen distancePlusOne) => HorizontalDistancePlusOne = distancePlusOne;

    /// <summary>
    /// Records the measure a new Samus starts with, one pixel, as initialising her does wherever the game initialises
    /// her: loading a saved game, starting at Ceres, arriving on Zebes after Ceres and loading an attract demo.
    /// </summary>
    public void RecordSamusInitialization() => HorizontalDistancePlusOne = InitialDistancePlusOne;
}
