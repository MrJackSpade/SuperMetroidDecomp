namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>How and where a title demonstration starts Samus (#2500).</summary>
/// <param name="Setup">How the demonstration stages her once its room has loaded.</param>
/// <param name="Position">The room position the demonstration places her at.</param>
public readonly record struct AttractDemoSamusStaging(AttractDemoSamusSetup Setup, Position16<RoomPixels> Position);
