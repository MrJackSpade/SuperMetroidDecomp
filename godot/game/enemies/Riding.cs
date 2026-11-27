namespace Game.Enemies;

/// <summary>How Samus is taken to stand on the enemies she can ride.</summary>
public static class Riding
{
    /// <summary>
    /// How many pixels over an enemy's top her feet may be while she still stands on it; her middle is reckoned as
    /// many pixels lower when it is asked whether it lies above the enemy's. Every enemy that asks whether she stands
    /// on it reckons her so.
    /// </summary>
    public const int FeetAbove = 3;
}
