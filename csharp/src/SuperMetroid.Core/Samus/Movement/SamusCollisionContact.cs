namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The body a block reaction acts on, with the contact-damage mode its sand reaction reads.</summary>
/// <param name="Body">The live or probe kinematics the reaction moves.</param>
/// <param name="ContactDamage">The contact-damage mode in force: the probe's captured mode, or the owning Samus's live one.</param>
public readonly record struct SamusCollisionContact(SamusKinematicsState Body, SamusContactDamageIndex ContactDamage);
