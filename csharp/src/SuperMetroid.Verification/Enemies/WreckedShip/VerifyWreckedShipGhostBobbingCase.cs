namespace SuperMetroid.Verification.Enemies.WreckedShip;

/// <summary>The context, y, origin y, whole, expected y and expected velocity of one verify wrecked ship ghost bobbing case.</summary>
/// <param name="Context">The context.</param>
/// <param name="Y">The y.</param>
/// <param name="OriginY">The origin y.</param>
/// <param name="Whole">The whole.</param>
/// <param name="ExpectedY">The expected y.</param>
/// <param name="ExpectedVelocity">The expected velocity.</param>
internal readonly record struct VerifyWreckedShipGhostBobbingCase(string Context, ushort Y, ushort OriginY, short Whole, ushort ExpectedY, int ExpectedVelocity);
