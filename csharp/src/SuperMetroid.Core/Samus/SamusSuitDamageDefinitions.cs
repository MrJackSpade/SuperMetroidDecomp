namespace SuperMetroid.Core.Samus;

/// <summary>How Samus's suits divide the damage she takes (<see cref="SamusSuitDamage"/>).</summary>
internal static class SamusSuitDamageDefinitions
{
    /// <summary>The Gravity Suit divides the damage Samus takes by this: she takes a quarter of it.</summary>
    internal const int GravityDivisor = 4;

    /// <summary>The Varia Suit, without the Gravity Suit, divides the damage Samus takes by this: she takes half of it.</summary>
    internal const int VariaDivisor = 2;
}
