namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The running and direction of one edited flare.</summary>
/// <param name="Running">The running.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct EditedFlare(bool Running, int Direction);
