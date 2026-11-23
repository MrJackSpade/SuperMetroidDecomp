using Engine.Shared;

namespace Engine.Tests.Shared;

/// <summary>Checks of the collision rectangle: its edges, overlap and placement.</summary>
public sealed class PixelRectChecks
{
    [Check]
    public void TheRightAndBottomEdgesLieJustPastTheLastPixel()
    {
        var rect = new PixelRect(0, 0, 16, 8);
        Expect.Equal(16, rect.Right);
        Expect.Equal(8, rect.Bottom);
        Expect.True(rect.Contains(15, 7));
        Expect.False(rect.Contains(16, 7));
    }

    [Check]
    public void RectanglesThatShareAPixelOverlap()
    {
        var first = new PixelRect(0, 0, 16, 16);
        Expect.True(first.Overlaps(new PixelRect(15, 15, 4, 4)));
    }

    [Check]
    public void RectanglesThatOnlyTouchDoNotOverlapButDoTouch()
    {
        var first = new PixelRect(0, 0, 16, 16);
        var beside = new PixelRect(16, 0, 16, 16);
        Expect.False(first.Overlaps(beside));
        Expect.True(first.OverlapsOrTouches(beside));
        Expect.False(first.OverlapsOrTouches(new PixelRect(17, 0, 16, 16)));
    }

    [Check]
    public void AnEmptyRectangleOverlapsNothing()
    {
        var empty = new PixelRect(4, 4, 0, 8);
        Expect.False(empty.Overlaps(new PixelRect(0, 0, 16, 16)));
        Expect.False(empty.OverlapsOrTouches(new PixelRect(0, 0, 16, 16)));
    }

    [Check]
    public void ARectangleIsPlacedOnItsOwnersWholePixels()
    {
        PixelRect body = PixelRect.Around(0, 0, 5, 10);
        var owner = new PixelPosition(Pixels.FromParts(100, 40000), Pixels.FromParts(50, 1));
        Expect.Equal(new PixelRect(95, 40, 10, 20), body.At(owner));
    }

    [Check]
    public void ANegativeSizeIsRefused()
    {
        Expect.Throws<ArgumentOutOfRangeException>(() => new PixelRect(0, 0, -1, 4));
    }
}
