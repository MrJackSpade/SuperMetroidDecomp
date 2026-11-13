namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The bounds Kraid's mouth pass and body pass test each projectile against, as the previous hit in the same pass left
/// them (#1476). Each pass places Kraid's mouth rectangle once, then tests the projectiles <see cref="KraidShotPassReach"/> names, in its order; a
/// hit leaves its own values over the bounds, and every later projectile in that pass is tested against those leftovers
/// with wrapped signed comparisons:
/// <list type="bullet">
/// <item>a damaging mouth hit leaves the bottom <see cref="KraidMouthHitBound"/> gives and a top far above the room;</item>
/// <item>a mouth hit that does no damage leaves twice the place its dud flash takes (<see cref="SpriteObjectPlaces"/>) as
/// the bottom, or the projectile's X when the flash took no place, the projectile's Y as the top, and a fixed left bound;</item>
/// <item>a body hit leaves the projectile's X as the bottom and its Y as the top;</item>
/// <item>testing a projectile that lies below the bottom against the body's contour leaves the projectile's right edge
/// as the bottom, whether or not it hits.</item>
/// </list>
/// </summary>
/// <remarks>
/// Consulted by Kraid's mouth and body collision passes once per projectile tested, after the pass placed the mouth
/// rectangle and before the next projectile is tested. Evidence: the original passes (<c>$A7:AFAA</c>, <c>$A7:B181</c>)
/// keep the rectangle in shared working values that the hit reaction, the dud flash's sprite allocation and the body
/// contour test overwrite and never reload within the pass; the Kraid mouth damage and body contour checks and the
/// Kraid recorded movie confirm the leftovers.
/// </remarks>
[CompatibilityService("a later projectile in the same Kraid collision pass is tested against the bounds the previous hit left behind")]
public struct KraidPassBounds
{
    /// <summary>The bottom a mouth hit that does no damage leaves per place its dud flash takes.</summary>
    private const int BottomPerDudFlashPlace = 2;

    /// <summary>
    /// The top a damaging mouth hit leaves: 3750 pixels above the room's top, so every later projectile in the pass passes
    /// the top test. Evidence: the common shot reaction leaves its vulnerability lookup, the location of Kraid's
    /// vulnerability table, in the working value the next projectile's signed comparison reads as the top (#1413); the
    /// enemy definitions check confirms the table's location is this coordinate.
    /// </summary>
    public const int TopAfterMouthHit = -3750;

    /// <summary>
    /// The left bound a mouth hit that does no damage leaves: six pixels from the room's left. Evidence: the dud flash's
    /// spawn leaves its object selector in the working value the next projectile's comparison reads as the left bound.
    /// </summary>
    public const int LeftAfterDud = 6;

    /// <summary>The upper vertical bound, as a wrapped room coordinate.</summary>
    private int _top;

    /// <summary>The left horizontal bound, as a wrapped room coordinate.</summary>
    private int _left;

    /// <summary>The lower vertical bound the next projectile is tested against, as a wrapped room coordinate.</summary>
    public int Bottom { readonly get; private set; }

    /// <summary>Places the mouth rectangle at the start of a pass.</summary>
    /// <param name="left">The rectangle's left edge in room pixels.</param>
    /// <param name="top">The rectangle's top edge in room pixels.</param>
    /// <param name="bottom">The rectangle's bottom edge in room pixels.</param>
    public static KraidPassBounds Placed(int left, int top, int bottom) =>
        new() { _left = Wrap(left), _top = Wrap(top), Bottom = Wrap(bottom) };

    /// <summary>Whether a projectile whose top edge is <paramref name="shotTop"/> reaches above the bottom bound.</summary>
    /// <param name="shotTop">The projectile's top edge in room pixels.</param>
    public readonly bool IsAboveBottom(int shotTop) => unchecked((short)(shotTop - 1 - Bottom)) < 0;

    /// <summary>Whether a projectile reaches below the top bound and right of the left bound.</summary>
    /// <param name="shotBottom">The projectile's bottom edge in room pixels.</param>
    /// <param name="shotRight">The projectile's right edge in room pixels.</param>
    public readonly bool ReachesTopAndLeft(int shotBottom, int shotRight) =>
        unchecked((short)(shotBottom - _top)) >= 0 && unchecked((short)(shotRight - _left)) >= 0;

    /// <summary>Whether a projectile overlaps the bounds as the mouth test reads them.</summary>
    /// <param name="shotTop">The projectile's top edge in room pixels.</param>
    /// <param name="shotBottom">The projectile's bottom edge in room pixels.</param>
    /// <param name="shotRight">The projectile's right edge in room pixels.</param>
    public readonly bool Overlaps(int shotTop, int shotBottom, int shotRight) =>
        IsAboveBottom(shotTop) && ReachesTopAndLeft(shotBottom, shotRight);

    /// <summary>Leaves a projectile's right edge as the bottom, as the body contour test does.</summary>
    /// <param name="shotRight">The projectile's right edge in room pixels.</param>
    public void LeaveRightEdgeAsBottom(int shotRight) => Bottom = Wrap(shotRight);

    /// <summary>Leaves the bounds a damaging mouth hit leaves: the given bottom and <see cref="TopAfterMouthHit"/>.</summary>
    /// <param name="bottom">The bottom <see cref="KraidMouthHitBound"/> gives for the hit.</param>
    public void LeaveAfterMouthHit(int bottom)
    {
        Bottom = Wrap(bottom);
        _top = Wrap(TopAfterMouthHit);
    }

    /// <summary>Leaves the bounds a mouth hit that does no damage leaves, with <see cref="LeftAfterDud"/> as the left bound.</summary>
    /// <param name="dudFlashPlace">The place the dud flash the hit spawned took, or null when it took none.</param>
    /// <param name="shot">The projectile's position in room pixels.</param>
    public void LeaveAfterDud(int? dudFlashPlace, Position16<RoomPixels> shot)
    {
        Bottom = dudFlashPlace is { } place ? Ensure.AtLeastZero(place) * BottomPerDudFlashPlace : shot.X;
        _top = shot.Y;
        _left = LeftAfterDud;
    }

    /// <summary>Leaves the bounds a body hit leaves.</summary>
    /// <param name="shot">The projectile's position in room pixels.</param>
    public void LeaveAfterBodyHit(Position16<RoomPixels> shot)
    {
        Bottom = shot.X;
        _top = shot.Y;
    }

    /// <summary>A room coordinate as the bounds hold it, wrapped into the sixteen-bit range the comparisons read.</summary>
    private static int Wrap(int coordinate) => unchecked((ushort)coordinate);
}
