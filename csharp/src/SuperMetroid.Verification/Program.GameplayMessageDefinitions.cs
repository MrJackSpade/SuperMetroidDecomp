using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGameplayMessageDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int address) => unchecked((ushort)(
            retail.ReadByte(address) | retail.ReadByte(address + 1) << 8));

        for (int index = 1;
             index <= GameplayMessageDefinitions.NativeDefinitionCount;
             index++)
        {
            int address = GameplayMessageRomData.Assets.DefinitionTable +
                (index - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
            GameplayMessageDefinition definition =
                GameplayMessageDefinitions.AtNativeIndex(index);
            AssertEqual(Word(address), definition.ModifyFunction,
                $"message definition {index:X2} modify callback");
            AssertEqual(Word(address + 2), definition.DrawFunction,
                $"message definition {index:X2} draw callback");
            AssertEqual(Word(address + 4), definition.ContentPointer,
                $"message definition {index:X2} content pointer");
        }

        var guard = new GameplayMessageDefinitionReadGuard(retail);
        for (int raw = (byte)GameplayMessageId.EnergyTank;
             raw <= (byte)GameplayMessageId.GravitySuit;
             raw++)
        {
            var state = new GameplayMessageBoxState();
            state.Begin(guard, (GameplayMessageId)raw);
            AssertTrue(state.TilemapRowCount >= GameplayMessageRomData.Layout.MinimumRows,
                $"message {raw:X2} builds from compiled definition metadata");
        }
        var gunship = new GameplayMessageBoxState();
        gunship.Begin(guard, GameplayMessageId.GunshipSaveConfirmation);
        AssertTrue(gunship.TilemapRowCount >= GameplayMessageRomData.Layout.MinimumRows,
            "gunship message builds from compiled definition metadata");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production message builder avoids native definition records");
        AssertThrows<ArgumentOutOfRangeException>(
            () => GameplayMessageDefinitions.AtNativeIndex(0),
            "message definition rejects index zero");
        AssertThrows<ArgumentOutOfRangeException>(
            () => GameplayMessageDefinitions.AtNativeIndex(
                GameplayMessageDefinitions.NativeDefinitionCount + 1),
            "message definition rejects index after terminator");

        Console.WriteLine(
            "  Gameplay-message definitions: all 29 callback/content records match the cartridge; every supported production message builds with the native definition table forbidden.");
    }

    private sealed class GameplayMessageDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int start = GameplayMessageRomData.Assets.DefinitionTable;
            int end = start + GameplayMessageDefinitions.NativeDefinitionCount *
                GameplayMessageRomData.Layout.DefinitionBytes;
            if (address >= start && address < end)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Gameplay message reread compiled definition byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
