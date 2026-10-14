namespace SuperMetroid.Core.Frontend;

/// <summary>One signed origin-offset and start-delay record for an intro explosion actor.</summary>
internal readonly record struct IntroMotherBrainExplosionPlacement(
    Offset16<ScreenPixels> Offset,
    ushort StartTimer);
