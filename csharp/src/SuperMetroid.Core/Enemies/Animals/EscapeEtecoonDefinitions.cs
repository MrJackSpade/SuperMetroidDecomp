namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Fixed initialization definitions for the three escape Etecoon actors.</summary>
internal static class EscapeEtecoonDefinitions
{
    /// <summary>The role each authored even population parameter-one value selects.</summary>
    private static readonly ParameterVariants<EscapeEtecoonRole> Roles = new(new Dictionary<int, EscapeEtecoonRole>
    {
        [0] = EscapeEtecoonRole.LeftWalker,
        [2] = EscapeEtecoonRole.RightWalker,
        [4] = EscapeEtecoonRole.WaitForEscapeEvent,
    });

    /// <summary>
    /// $B3:E718..E735 InitializationAI_EtecoonEscape: left walker, right walker
    /// and event-waiting role select position, action, program and horizontal speed.
    /// The cartridge clears parameter bit zero before dispatching.
    /// </summary>
    internal static EscapeEtecoonInitialization Initialization(int parameter1) =>
        (Roles.TryDecode(parameter1 & 0xfffe, out EscapeEtecoonRole role)
            ? role
            : throw new ArgumentOutOfRangeException(nameof(parameter1), parameter1,
                "Escape Etecoon parameter must select one of the three authored roles.")) switch
        {
            EscapeEtecoonRole.LeftWalker => new(new(128, 200), EscapeEtecoonPreInstruction.WalkAndFall,
                EscapeEtecoonProgram.RunningLeftLowTide, new(-512, 0)),
            EscapeEtecoonRole.RightWalker => new(new(160, 200), EscapeEtecoonPreInstruction.WalkAndFall,
                EscapeEtecoonProgram.RunningRightLowTide, new(640, 0)),
            EscapeEtecoonRole.WaitForEscapeEvent => new(new(232, 200), EscapeEtecoonPreInstruction.WaitForEscapeEvent,
                EscapeEtecoonProgram.Stationary, default),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter1), parameter1,
                "Escape Etecoon parameter must select one of the three authored roles."),
        };
}
