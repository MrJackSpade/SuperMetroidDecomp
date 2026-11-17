namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What one way Samus moves does when her input changes nothing and when a bomb jump is published (#2323).</summary>
/// <param name="LookupFailureResponse">What her momentum does when her input finds no new pose.</param>
/// <param name="BombJump">How a published bomb jump is set up.</param>
internal sealed record SamusMovementTypeDefinition(SamusMomentumResponse LookupFailureResponse, SamusBombJumpSetup BombJump);
