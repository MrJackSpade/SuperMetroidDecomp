using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidNailSibling(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        typeof(RoomEnemySystem).GetField("_kraidState", flags)!.SetValue(enemies, state);
        enemies.Slots[0].EnemyDefinitionPointer = RoomEnemySystem.KraidDefinition;
        ushort random = 0;
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => random));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => throw new InvalidOperationException("Nail initialization must not advance RNG.")));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeKraidNailFlight", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidPartState>>(enemies);
        foreach (int slot in new[] { 6, 7 })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        foreach (ushort siblingFlag in new ushort[] { 0, 1, 2, 65535 })
        {
            var nail = enemies.Slots[slot];
            var sibling = enemies.Slots[13 - slot];
            var part = state.Parts[slot];
            random = (ushort)raw;
            sibling.VariableE = (ushort)raw;
            nail.VariableE = unchecked((ushort)~raw);
            state.Parts[sibling.SlotIndex].AlternateSpawnFlag = siblingFlag;
            part.AlternateSpawnFlag = siblingFlag == 1 ? (ushort)0 : (ushort)1;
            bool diagonal = (random & 1) == 0 || siblingFlag == 1;
            int table = (short)sibling.VariableE < 0 ? 0xa7be3e : 0xa7be46;
            int pointer = 0xa70000 | Word(table + (random & 6));
            AssertEqual((Word(pointer), Word(pointer + 2), Word(pointer + 4), Word(pointer + 6)),
                KraidNailLaunchDefinitions.FromSiblingVelocity(sibling.VariableE), "All native indirect launch records match compiled words");
            nail.VariableB = nail.VariableD = ushort.MaxValue;
            initialize(nail, part);
            AssertEqual((ushort)0, nail.VariableB, "Launch clears old X fraction");
            AssertEqual((ushort)0, nail.VariableD, "Launch clears old Y fraction");
            AssertEqual(diagonal ? Word(pointer + 6) : (ushort)0, nail.VariableE, "Nail launch Y velocity comes from sibling sign");
            AssertEqual(diagonal ? Word(pointer + 2) : (ushort)1, nail.VariableC, "Nail launch X velocity and horizontal override");
            AssertEqual((ushort)(diagonal ? 0 : 1), part.AlternateSpawnFlag, "Nail spawn mode consults sibling flag, writes own flag");
            AssertEqual(siblingFlag, state.Parts[sibling.SlotIndex].AlternateSpawnFlag, "Sibling flag remains unchanged");
            AssertEqual((ushort)raw, sibling.VariableE, "Sibling velocity remains unchanged");
            AssertEqual((ushort)(diagonal ? KraidAiFunction.FingernailFire : KraidAiFunction.FingernailWaitForLint), nail.VariableA, "Sibling choice selects actual launch phase");
        }
        Console.WriteLine("Kraid nail sibling: both slots, all RNG/sign words and four sibling flags match native launch handoff without ROM access.");
    }
}
