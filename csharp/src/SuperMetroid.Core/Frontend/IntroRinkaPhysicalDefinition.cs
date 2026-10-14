namespace SuperMetroid.Core.Frontend;

/// <summary>One intro Rinka's final origin and signed whole-pixel X velocity component.</summary>
internal readonly record struct IntroRinkaPhysicalDefinition(
    Position16<ScreenPixels> Position,
    short XWholeVelocity);
