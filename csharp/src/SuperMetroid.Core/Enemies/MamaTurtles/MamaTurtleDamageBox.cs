using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The box around an awake Mama Turtle that hurts Samus (#2341): her body grown sideways by
/// <see cref="MamaTurtleMotionDefinitions.DamageBoxReach"/> and shrunk at the top and bottom by
/// <see cref="MamaTurtleMotionDefinitions.DamageBoxInset"/>.
/// </summary>
internal static class MamaTurtleDamageBox
{
    /// <summary>
    /// Whether Samus's body touches the box: sideways and at the box's top she touches it from the pixel she meets its
    /// edge; at its bottom her top must reach more than a pixel inside it.
    /// </summary>
    internal static bool Touches(RoomEnemyPlace mama, SamusState samus)
    {
        int left = mama.Position.X.Whole - mama.Radius.X - MamaTurtleMotionDefinitions.DamageBoxReach;
        int right = mama.Position.X.Whole + mama.Radius.X + MamaTurtleMotionDefinitions.DamageBoxReach;
        int top = mama.Position.Y.Whole - mama.Radius.Y + MamaTurtleMotionDefinitions.DamageBoxInset;
        int bottom = mama.Position.Y.Whole + mama.Radius.Y - MamaTurtleMotionDefinitions.DamageBoxInset;
        int samusLeft = samus.Position.X.Whole - samus.Kinematics.Radius.X;
        int samusRight = samus.Position.X.Whole + samus.Kinematics.Radius.X;
        int samusTop = samus.Position.Y.Whole - samus.Kinematics.Radius.Y;
        int samusBottom = samus.Position.Y.Whole + samus.Kinematics.Radius.Y;
        return samusLeft <= right && samusRight >= left && samusTop + 1 < bottom && samusBottom >= top;
    }
}
