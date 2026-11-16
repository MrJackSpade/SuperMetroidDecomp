namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The opening's laboratory scenes: where their pages scroll as the scientists receive and examine the baby.</summary>
internal static class IntroLaboratoryDefinitions
{
    /// <summary>The first laboratory page's scroll as the delivery scene starts; it then pans left.</summary>
    internal static Offset16<PlanePixels> DeliveryPageScroll => new(32, 8);

    /// <summary>The second laboratory page's scroll as the examination scene starts; it then moves down.</summary>
    internal static Offset16<PlanePixels> ExaminationPageStartScroll => new(0, -24);

    /// <summary>The vertical scroll at which the examination's page stops moving down.</summary>
    internal const int ExaminationPageEndY = 8;
}
