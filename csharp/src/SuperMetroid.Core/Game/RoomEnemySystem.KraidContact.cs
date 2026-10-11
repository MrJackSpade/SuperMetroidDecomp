namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    // These collision checks belong to actor AI rather than generic interactive hitboxes.
    // The body's permanent intangible flag and lint's no-op touch callback do not
    // suppress these private native paths.
    private void ResolveKraidBodyContact(RoomEnemySlot body, KraidEnemyState state, SamusState samus)
    {
        if (unchecked((short)(body.VariableA - (ushort)KraidAiFunction.DeathInitialize)) >= 0)
            return;
        short relativeY = unchecked((short)(samus.Position.Y.Whole - body.Position.Y.Whole));
        if (unchecked((short)(body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) -
            samus.Position.X.Whole - samus.Kinematics.Radius.X)) >= 0)
            return;
        if (unchecked((short)(samus.Position.X.Whole - KraidContactDefinitions.MinimumBodyEjectionX)) >= 0)
            samus.Position -= new Offset16<RoomPixels>(unchecked((short)KraidContactDefinitions.BodyEjection), 0);
        ushort nextY = unchecked((ushort)(samus.Position.Y.Whole - KraidContactDefinitions.BodyEjection));
        samus.Position = samus.Position.WithWholeY(unchecked((short)(nextY - state.MinimumYPositionForEjection)) < 0
            ? state.MinimumYPositionForEjection : nextY);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(KraidContactDefinitions.ExtraX, KraidContactDefinitions.ExtraY));
        if (samus.InvincibilityTimer == 0)
            ResolveNormalEnemyTouch(body, samus);
    }

    private void ResolveKraidLintContact(RoomEnemySlot lint, SamusState samus)
    {
        if (lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) || samus.InvincibilityTimer != 0)
            return;
        ushort edge = unchecked((ushort)(lint.Position.X.Whole + KraidContactDefinitions.LintLeadingEdge));
        if (unchecked((short)(samus.Position.X.Whole + samus.Kinematics.Radius.X - edge)) < 0 ||
            unchecked((short)(samus.Position.X.Whole - samus.Kinematics.Radius.X - edge)) >= 0 ||
            unchecked((short)(samus.Position.Y.Whole + samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole - KraidContactDefinitions.LintTop)) < 0 ||
            unchecked((short)(samus.Position.Y.Whole - samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole - KraidContactDefinitions.LintBottom)) >= 0)
            return;
        ushort push = unchecked((ushort)(~(samus.Kinematics.Radius.X + KraidContactDefinitions.LintPush) +
            samus.Kinematics.ExtraDisplacement.X.WholePixels));
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(unchecked((short)(
            unchecked((short)(push - KraidContactDefinitions.LintPush)) < 0 ? push : KraidContactDefinitions.LintPush)));
        ResolveNormalEnemyTouch(lint, samus);
        lint.Properties = lint.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }
}
