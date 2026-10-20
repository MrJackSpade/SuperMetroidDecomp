using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies;

/// <summary>One typed enemy-projectile family's oracle parameters.</summary>
internal sealed class NativeEnemyProjectileProgramFamily<TProgram, TFrame, TCue>(
    (TProgram Program, ushort Entry)[] entries,
    AnimationProgramSet<TProgram, TFrame, TCue> programs,
    Func<TFrame, ushort> presentationKey,
    NativeEnemyProjectileCueRules<TCue> rules,
    Func<TFrame, ushort?>? reflectedPresentationKey) : NativeEnemyProjectileFamily
    where TProgram : struct, Enum
    where TFrame : struct
    where TCue : notnull
{
    /// <inheritdoc/>
    internal override int CompareAll(IImportCartridgeSource cartridge) =>
        NativeEnemyProjectileProgramOracle.CompareAll(cartridge, entries, programs, presentationKey, rules, reflectedPresentationKey);

    /// <inheritdoc/>
    internal override Func<EnemyProjectileAnimation, ushort?> NativePointers(IImportCartridgeSource cartridge)
    {
        Dictionary<TProgram, ushort[]> addresses = StepAddresses(cartridge);
        return animation => animation is EnemyProjectileProgramAnimation<TProgram, TFrame, TCue> typed
            ? addresses[typed.Cursor.Program][typed.Cursor.Position]
            : null;
    }

    /// <inheritdoc/>
    internal override bool TryResumeAt(IImportCartridgeSource cartridge, EnemyProjectileAnimation animation, ushort nativePointer)
    {
        if (animation is not EnemyProjectileProgramAnimation<TProgram, TFrame, TCue> typed)
            return false;
        // A program entry also ends the program that runs on into it; the entry is where the native list stands.
        (TProgram Program, int Position)[] matches = [.. StepAddresses(cartridge)
            .SelectMany(program => program.Value.Select((address, position) => (program.Key, address, position)))
            .Where(step => step.address == nativePointer)
            .Select(step => (step.Key, step.position))];
        (TProgram Program, int Position)[] entryMatches = [.. matches.Where(step => step.Position == 0)];
        (TProgram Program, int Position) resumed = entryMatches.Length == 1 ? entryMatches[0]
            : matches.Length == 1 ? matches[0]
            : throw new InvalidDataException($"Native $86:{nativePointer:X4} matches {matches.Length} typed {typeof(TProgram).Name} steps.");
        typed.Cursor = new AnimationProgramCursor<TProgram>(resumed.Program, resumed.Position);
        return true;
    }

    /// <summary>Every program's step addresses, walked once per cartridge.</summary>
    private Dictionary<TProgram, ushort[]> StepAddresses(IImportCartridgeSource cartridge)
    {
        if (!ReferenceEquals(walkedCartridge, cartridge) || walkedAddresses is null)
        {
            walkedAddresses = NativeEnemyProjectileProgramOracle.StepAddresses(cartridge, entries, programs, presentationKey, rules, reflectedPresentationKey);
            walkedCartridge = cartridge;
        }
        return walkedAddresses;
    }

    /// <summary>The cartridge <see cref="walkedAddresses"/> came from.</summary>
    private IImportCartridgeSource? walkedCartridge;

    /// <summary>The step addresses of the last walk.</summary>
    private Dictionary<TProgram, ushort[]>? walkedAddresses;
}
