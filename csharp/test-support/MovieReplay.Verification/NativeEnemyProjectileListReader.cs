using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.VerificationImporters;
using static SuperMetroid.TestSupport.MovieReplay.NativeEnemyProjectileListDefinitions;

namespace SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>A native bank-$86 projectile list with the projectile interpreter's timer gate.</summary>
internal sealed class NativeEnemyProjectileListReader(ushort cursor, ushort timer)
{
    /// <summary>The next native instruction.</summary>
    private ushort cursor = cursor;
    /// <summary>Updates remaining before the next native instruction is consumed.</summary>
    internal ushort Timer { get; private set; } = timer;
    /// <summary>The spritemap operand address of the latest native frame.</summary>
    internal ushort Operand { get; private set; }
    /// <summary>The pre-instruction the list installed, if any.</summary>
    internal ushort? FrameBehavior { get; private set; }
    /// <summary>Whether the list cleared its pre-instruction.</summary>
    internal bool FrameBehaviorCleared { get; private set; }
    /// <summary>Whether the list cleared its palette selection.</summary>
    internal bool PaletteCleared { get; private set; }
    /// <summary>The long routine of each external call the latest update made.</summary>
    internal List<int> Calls { get; } = [];
    /// <summary>Whether the list has deleted its projectile.</summary>
    internal bool Deleted { get; private set; }
    /// <summary>The projectile's general-timer word, which the counter commands count.</summary>
    internal ushort Counter { get; private set; }
    /// <summary>The owner callbacks the latest update executed.</summary>
    internal List<NativeEnemyProjectileInstruction> Callbacks { get; } = [];

    /// <summary>Runs one update.</summary>
    internal void Step(IImportCartridgeSource cartridge)
    {
        Calls.Clear();
        Callbacks.Clear();
        ushort old = Timer;
        Timer = unchecked((ushort)(Timer - 1));
        if (old != 1)
            return;
        for (int command = 0; command < 16; command++)
        {
            ushort word = cartridge.ReadCartridgeWord(ProjectileBank | cursor);
            switch (Classify(word))
            {
                case WordKind.Frame:
                    Timer = word;
                    Operand = unchecked((ushort)(cursor + 2));
                    cursor = unchecked((ushort)(cursor + 4));
                    return;
                case WordKind.FrameBehavior:
                    FrameBehavior = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case WordKind.ClearFrameBehavior:
                    FrameBehaviorCleared = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case WordKind.UsePaletteZero:
                    PaletteCleared = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case WordKind.CallExternal:
                    Calls.Add(cartridge.ReadCartridgeLong(ProjectileBank | (cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 5));
                    continue;
                case WordKind.GoTo:
                    cursor = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
                    continue;
                case WordKind.Sleep:
                    return;
                case WordKind.Delete:
                    Deleted = true;
                    return;
                case WordKind.SetCounter:
                    Counter = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case WordKind.RepeatWhileCounting:
                    Counter = unchecked((ushort)(Counter - 1));
                    cursor = Counter != 0
                        ? cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2))
                        : unchecked((ushort)(cursor + 4));
                    continue;
                case WordKind.Callback:
                    Callbacks.Add((NativeEnemyProjectileInstruction)word);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case WordKind.Scatter:
                    cursor = unchecked((ushort)(cursor + 6));
                    continue;
                case WordKind.CallbackWithWord:
                    Callbacks.Add((NativeEnemyProjectileInstruction)word);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(WordKind)}.");
            }
        }
        throw new InvalidDataException("A native projectile list ran sixteen commands without a frame.");
    }
}
