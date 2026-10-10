namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    // These collision checks belong to actor AI rather than generic interactive hitboxes.
    // The body's permanent intangible flag and lint's no-op touch callback do not
    // suppress these private native paths.
    /// <summary>Applies Kraid's actor-specific body contour ejection and normal touch damage while the body is alive.</summary>
    /// <param name="body">The Kraid body slot supplying the current position and function.</param>
    /// <param name="state">The Kraid state supplying the minimum Samus ejection height.</param>
    /// <param name="samus">The player state repositioned and contacted by the body collision.</param>
    private void ResolveKraidBodyContact(RoomEnemySlot body, KraidEnemyState state, SamusState samus)
    {
        if (unchecked((short)(body.VariableA - (ushort)KraidAiFunction.DeathInitialize)) >= 0)
            return;
        short relativeY = unchecked((short)(samus.YPosition - body.YPosition));
        if (unchecked((short)(body.XPosition + KraidBodyContour.LeftEdge(relativeY) -
            samus.XPosition - samus.Kinematics.XRadius)) >= 0)
            return;
        if (unchecked((short)(samus.XPosition - KraidContactDefinitions.MinimumBodyEjectionX)) >= 0)
            samus.XPosition = unchecked((ushort)(samus.XPosition - KraidContactDefinitions.BodyEjection));
        ushort nextY = unchecked((ushort)(samus.YPosition - KraidContactDefinitions.BodyEjection));
        samus.YPosition = unchecked((short)(nextY - state.MinimumYPositionForEjection)) < 0
            ? state.MinimumYPositionForEjection : nextY;
        samus.Kinematics.ExtraXDisplacement = KraidContactDefinitions.ExtraX;
        samus.Kinematics.ExtraYDisplacement = KraidContactDefinitions.ExtraY;
        if (samus.InvincibilityTimer == 0)
            ResolveNormalEnemyTouch(body, samus, 0);
    }

    /// <summary>Checks Samus against Kraid's lint leading edge, pushes her away on overlap, and disables repeat contact.</summary>
    /// <param name="lint">The lint actor slot supplying its edge position and collision flags.</param>
    /// <param name="samus">The player state tested for overlap and pushed on contact.</param>
    private void ResolveKraidLintContact(RoomEnemySlot lint, SamusState samus)
    {
        if (lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) || samus.InvincibilityTimer != 0)
            return;
        ushort edge = unchecked((ushort)(lint.XPosition + KraidContactDefinitions.LintLeadingEdge));
        if (unchecked((short)(samus.XPosition + samus.Kinematics.XRadius - edge)) < 0 ||
            unchecked((short)(samus.XPosition - samus.Kinematics.XRadius - edge)) >= 0 ||
            unchecked((short)(samus.YPosition + samus.Kinematics.YRadius -
                lint.YPosition - KraidContactDefinitions.LintTop)) < 0 ||
            unchecked((short)(samus.YPosition - samus.Kinematics.YRadius -
                lint.YPosition - KraidContactDefinitions.LintBottom)) >= 0)
            return;
        ushort push = unchecked((ushort)(~(samus.Kinematics.XRadius + KraidContactDefinitions.LintPush) +
            samus.Kinematics.ExtraXDisplacement));
        samus.Kinematics.ExtraXDisplacement = unchecked((short)(push - KraidContactDefinitions.LintPush)) < 0
            ? push : KraidContactDefinitions.LintPush;
        ResolveNormalEnemyTouch(lint, samus, 0);
        lint.Properties = lint.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }
}
