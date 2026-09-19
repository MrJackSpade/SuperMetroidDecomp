using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroMotherBrainDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        IntroMotherBrainActorDefinition[] actors =
        [
            IntroMotherBrainDefinitions.MotherBrain,
            IntroMotherBrainDefinitions.BigExplosionActor,
            IntroMotherBrainDefinitions.SmallExplosionActor,
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            IntroMotherBrainActorDefinition actual = actors[index];
            int address = IntroMotherBrainDefinitions.NativeBank | actual.Pointer;
            AssertEqual(ReadIntroMotherBrainWord(retail, address), actual.Initialization,
                $"intro Mother Brain actor {index} initialization callback");
            AssertEqual(ReadIntroMotherBrainWord(retail, address + 2), actual.PreInstruction,
                $"intro Mother Brain actor {index} pre-instruction callback");
            AssertEqual(ReadIntroMotherBrainWord(retail, address + 4), actual.InstructionList,
                $"intro Mother Brain actor {index} instruction list");
        }

        for (int index = 0; index < IntroMotherBrainDefinitions.BigExplosionCount; index++)
        {
            IntroMotherBrainExplosionPlacement actual = IntroMotherBrainDefinitions.BigExplosion(index);
            AssertEqual(unchecked((short)ReadIntroMotherBrainWord(retail,
                    IntroMotherBrainDefinitions.BigXOffsetReferenceAddress + index * 2)),
                actual.XOffset, $"intro Mother Brain big explosion {index} X offset");
            AssertEqual(unchecked((short)ReadIntroMotherBrainWord(retail,
                    IntroMotherBrainDefinitions.BigYOffsetReferenceAddress + index * 2)),
                actual.YOffset, $"intro Mother Brain big explosion {index} Y offset");
            AssertEqual(ReadIntroMotherBrainWord(retail,
                    IntroMotherBrainDefinitions.BigTimerReferenceAddress + index * 2),
                actual.StartTimer, $"intro Mother Brain big explosion {index} timer");
        }

        for (int index = 0; index < IntroMotherBrainDefinitions.SmallExplosionCount; index++)
        {
            IntroMotherBrainExplosionPlacement actual = IntroMotherBrainDefinitions.SmallExplosion(index);
            AssertEqual(unchecked((short)ReadIntroMotherBrainWord(retail,
                    IntroMotherBrainDefinitions.SmallXOffsetReferenceAddress + index * 2)),
                actual.XOffset, $"intro Mother Brain small explosion {index} X offset");
            AssertEqual(unchecked((short)ReadIntroMotherBrainWord(retail,
                    IntroMotherBrainDefinitions.SmallYOffsetReferenceAddress + index * 2)),
                actual.YOffset, $"intro Mother Brain small explosion {index} Y offset");
            AssertEqual(ReadIntroMotherBrainWord(retail,
                    IntroMotherBrainDefinitions.SmallTimerReferenceAddress + index * 2),
                actual.StartTimer, $"intro Mother Brain small explosion {index} timer");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => IntroMotherBrainDefinitions.BigExplosion(
                IntroMotherBrainDefinitions.BigExplosionCount),
            "intro Mother Brain big explosion definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => IntroMotherBrainDefinitions.SmallExplosion(
                IntroMotherBrainDefinitions.SmallExplosionCount),
            "intro Mother Brain small explosion definition boundary");

        var guarded = new IntroMotherBrainDefinitionReadGuard(retail);
        var motherBrain = new IntroMotherBrainSpriteState();
        AssertEqual(IntroMotherBrainDefinitions.MotherBrainOrigin.X,
            IntroMotherBrainSpriteState.XPosition, "intro Mother Brain compiled X origin");
        AssertEqual(IntroMotherBrainDefinitions.MotherBrainOrigin.Y,
            IntroMotherBrainSpriteState.YPosition, "intro Mother Brain compiled Y origin");
        motherBrain.Step(guarded);

        var explosions = new IntroMotherBrainExplosionSystem();
        explosions.SpawnFourthHitExplosions();
        AssertEqual(8, explosions.ActiveCount, "intro Mother Brain allocates all explosion actors");
        for (int frame = 0; frame < 192; frame++)
            explosions.Step(guarded, introCrossfadeTimer: 1);
        AssertEqual(8, explosions.ActiveCount,
            "intro Mother Brain explosion actors survive until page-two crossfade completion");
        explosions.Step(guarded, introCrossfadeTimer: 0);
        AssertEqual(0, explosions.ActiveCount,
            "intro Mother Brain explosion actors delete at page-two crossfade completion");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro Mother Brain actors never reread compiled definitions or placement tables");

        Console.WriteLine(
            "  Intro Mother Brain definitions: nine actor words and 24 placement words match; complete explosion lifetime is ROM-table independent.");
    }

    private static ushort ReadIntroMotherBrainWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    private sealed class IntroMotherBrainDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            bool forbidden =
                address is >= 0x8bce55 and < 0x8bce5b or
                >= 0x8bcf15 and < 0x8bcf21 or
                >= 0x8bb9b6 and < 0x8bb9d4 or
                >= 0x8bb9fd and < 0x8bba0f;
            if (forbidden)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Intro Mother Brain reread compiled byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
