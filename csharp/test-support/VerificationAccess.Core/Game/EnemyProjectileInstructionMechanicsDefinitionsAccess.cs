using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="EnemyProjectileInstructionMechanicsDefinitions"/> members production does not use.</summary>
internal static class EnemyProjectileInstructionMechanicsDefinitionsAccess
{
    /// <summary>Exposes miscellaneous dust program count and classifies projectile visual operands for audits.</summary>
    extension(EnemyProjectileInstructionMechanicsDefinitions)
    {
        /// <summary>Native $86:E42C selector domain, including its final two out-of-order roots.</summary>
        internal static int MiscDustProgramCount => 30;

        /// <summary>Recognizes presentation operands among the bank-$86 projectile instruction programs.</summary>
        internal static bool IsVisualOperand(ushort address)
        {
            int offset = address - EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingInitial;
            if (offset is >= 6 and < 48 && offset % 8 == 6) return true;
            offset = address - EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch;
            if (offset is >= 6 and < 28 && offset % 4 == 2) return true;
            offset = address - EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial;
            if (offset is >= 2 and < 20 && offset % 4 == 2 || offset == 28) return true;
            for (int index = 0; index < EnemyProjectileInstructionMechanicsDefinitions.TimedProgramCount; index++)
            {
                var program = ((EnemyProjectileTimedProgramDefinition)(EnemyProjectileInstructionMechanicsDefinitions.TimedProgram((int)(index))));
                offset = address - program.InitialPointer - (program.PrefixInstruction.HasValue ? 2 : 0);
                if (offset is >= 2 && offset < program.FrameCount * 4 && offset % 4 == 2) return true;
            }
            return false;
        }
    }
}
