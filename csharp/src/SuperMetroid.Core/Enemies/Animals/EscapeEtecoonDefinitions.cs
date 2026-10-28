namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Fixed initialization definitions for the three escape Etecoon actors.</summary>
internal static class EscapeEtecoonDefinitions
{
    /// <summary>The role each authored even population parameter-one value selects.</summary>
    internal static readonly ParameterVariants<EscapeEtecoonRole> Roles = new(new Dictionary<int, EscapeEtecoonRole>
    {
        [0] = EscapeEtecoonRole.LeftWalker,
        [2] = EscapeEtecoonRole.RightWalker,
        [4] = EscapeEtecoonRole.WaitForEscapeEvent,
    });

    /// <summary>The cartridge clears parameter bit zero before dispatching on the role.</summary>
    internal const int RoleParameterMask = 0xfffe;

    /// <summary>
    /// $B3:E718..E735 InitializationAI_EtecoonEscape: the position, action, program and horizontal speed each of the
    /// left walker, right walker and event-waiting roles starts with.
    /// </summary>
    internal static readonly Dictionary<EscapeEtecoonRole, EscapeEtecoonInitialization> Initializations = new()
    {
        [EscapeEtecoonRole.LeftWalker] = new(new(128, 200), EscapeEtecoonPreInstruction.WalkAndFall,
            EscapeEtecoonProgram.RunningLeftLowTide, new(-512, 0)),
        [EscapeEtecoonRole.RightWalker] = new(new(160, 200), EscapeEtecoonPreInstruction.WalkAndFall,
            EscapeEtecoonProgram.RunningRightLowTide, new(640, 0)),
        [EscapeEtecoonRole.WaitForEscapeEvent] = new(new(232, 200), EscapeEtecoonPreInstruction.WaitForEscapeEvent,
            EscapeEtecoonProgram.Stationary, default),
    };
}
