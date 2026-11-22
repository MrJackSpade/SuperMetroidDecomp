namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The opening's laboratory scenes: where their pages scroll as the scientists receive and examine the baby.</summary>
internal static partial class IntroLaboratoryDefinitions
{
    /// <summary>The first laboratory page's scroll as the delivery scene starts; it then pans left.</summary>
    internal static Offset16<PlanePixels> DeliveryPageScroll => new(32, 8);

    /// <summary>The second laboratory page's scroll as the examination scene starts; it then moves down.</summary>
    internal static Offset16<PlanePixels> ExaminationPageStartScroll => new(0, -24);

    /// <summary>The vertical scroll at which the examination's page stops moving down.</summary>
    internal const int ExaminationPageEndY = 8;


    /// <summary>How far the delivery scene pans its page each update: one pixel left.</summary>
    internal static Offset16<PlanePixels> DeliveryPagePan { get; } = new(-1, 0);

    /// <summary>How far the delivered baby moves on the screen each update the page pans, keeping its place on the page: one pixel right.</summary>
    internal static Offset16<ScreenPixels> DeliveredBabyCounterPan { get; } = new(1, 0);

    /// <summary>How far the examination scene moves its page down each update: one pixel.</summary>
    internal static Offset16<PlanePixels> ExaminationPageScroll { get; } = new(0, 1);

    /// <summary>How far the examined baby rises on the screen each update the page moves down: one pixel.</summary>
    internal static Offset16<ScreenPixels> ExaminedBabyRise { get; } = new(0, -1);
}
