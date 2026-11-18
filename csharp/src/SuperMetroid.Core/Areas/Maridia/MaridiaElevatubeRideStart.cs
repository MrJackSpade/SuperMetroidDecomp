namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>How the Maridia elevatube's ride starts from one end of the tube (#2485).</summary>
/// <param name="StartingY">The room Y, in whole pixels, the tube's tracked height starts at.</param>
/// <param name="StartingVelocity">The tube's starting vertical velocity in room pixels per update, negative upward.</param>
/// <param name="Acceleration">The tube's vertical acceleration in room pixels per update per update, negative upward.</param>
internal sealed record MaridiaElevatubeRideStart(int StartingY, SignedEightEight StartingVelocity, SignedEightEight Acceleration);
