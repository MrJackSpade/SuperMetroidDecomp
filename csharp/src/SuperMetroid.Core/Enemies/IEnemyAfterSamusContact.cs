using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with contact work of its own that the shared pass runs after the ordinary Samus contact pass.</summary>
internal interface IEnemyAfterSamusContact : IEnemyEntity
{
    /// <summary>Runs after the ordinary Samus contact pass.</summary>
    /// <param name="samus">Samus.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    void ResolveAfterSamusContact(SamusState samus, PseudoScrewCharge pseudoScrew);
}
