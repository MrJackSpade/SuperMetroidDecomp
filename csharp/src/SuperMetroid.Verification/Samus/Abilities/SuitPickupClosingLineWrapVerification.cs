using SuperMetroid.Core.Samus.Abilities;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks how the suit pickup's closing line narrows (<see cref="SuitPickupClosingLineWrap"/>, #1692).</summary>
internal static class SuitPickupClosingLineWrapVerification
{
    /// <summary>
    /// The narrowing against the original's arithmetic for every edge column the row holds: the left edge's column plus
    /// eight and the right edge's minus eight, each kept within its byte; a column outside the row is rejected. Then the
    /// dissipation from its authored start: the left edge comes round from 255 to 7 on the first update, the line closes
    /// for fifteen updates, and the update its left edge reaches 112 or more is the one that finishes the pickup.
    /// </summary>
    internal static void VerifySuitPickupClosingLineWrap()
    {
        for (int column = 0; column < SuitPickupClosingLineWrap.RowColumns; column++)
        {
            AssertEqual((column + 8) & 0xFF, SuitPickupClosingLineWrap.NarrowedLeft(column), $"left edge at column {column} narrows as the original's byte addition");
            AssertEqual((column - 8) & 0xFF, SuitPickupClosingLineWrap.NarrowedRight(column), $"right edge at column {column} narrows as the original's byte subtraction");
        }
        foreach (int outside in new[] { -1, SuitPickupClosingLineWrap.RowColumns })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SuitPickupClosingLineWrap.NarrowedLeft(outside), $"a left edge at column {outside} is rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => SuitPickupClosingLineWrap.NarrowedRight(outside), $"a right edge at column {outside} is rejected");
        }

        int left = SuitPickupBeamDefinitions.DissipationStart.Left.X;
        int right = SuitPickupBeamDefinitions.DissipationStart.Right.X;
        AssertEqual(255, left, "the dissipation starts its left edge at the rightmost column");
        AssertEqual(248, right, "the dissipation starts its right edge at column 248");
        int updates = 0;
        do
        {
            left = SuitPickupClosingLineWrap.NarrowedLeft(left);
            right = SuitPickupClosingLineWrap.NarrowedRight(right);
            updates++;
            if (updates == 1)
            {
                AssertEqual(7, left, "the first narrowing carries the left edge round to column 7");
                AssertEqual(240, right, "the first narrowing moves the right edge to column 240");
            }
        }
        while (left < 112);
        AssertEqual(15, updates, "the line closes for fifteen updates before the pickup finishes");
        AssertEqual(119, left, "the finishing update's left edge");
        AssertEqual(128, right, "the finishing update's right edge");

        Console.WriteLine("Suit pickup closing line: every edge narrows as the original's byte arithmetic; fifteen closing updates.");
    }
}
