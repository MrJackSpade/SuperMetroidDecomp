namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>How the interpreter proceeds after a registered instruction.</summary>
internal enum EnemyProjectileInstructionOutcome
{
    /// <summary>No registered handler applies; the engine's own opcodes decide.</summary>
    NotHandled,
    /// <summary>The instruction advanced the cursor; the interpreter reads the next word.</summary>
    Continue,
    /// <summary>The instruction ended this projectile's instruction pass (for example by deleting it).</summary>
    Stop,
}
