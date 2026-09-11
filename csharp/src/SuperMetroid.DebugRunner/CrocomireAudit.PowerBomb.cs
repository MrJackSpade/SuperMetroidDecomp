using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class CrocomireAudit
{
    /// <summary>Checks reaction selection and every movement/animation call against a ROM-list schedule.</summary>
    public static int RunPowerBombTrajectory(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        foreach (ushort expectedList in CrocomirePowerBombFixtureData.ReactionLists)
        {
            var room = CartridgeRoomHeader.Load(bus, RoomHeader);
            var loaded = Load(bus, room, CartridgeRoomAssets.Load(bus, room));
            var control = Load(bus, room, CartridgeRoomAssets.Load(bus, room));
            var state = RequireState(loaded);
            bool found = false;
            for (int frame = 0; frame < 360; frame++)
            {
                Step(loaded);
                Step(control);
                if (state.Body.SpritemapPointer >= 0x8000 && SelectList() == expectedList)
                { found = true; break; }
            }
            if (!found) throw new InvalidDataException($"Natural wake animation did not reach reaction variant {expectedList:X4}.");
            ushort startX = state.Body.XPosition, startY = state.Body.YPosition;
            int hits = loaded.Enemies.ResolveOrdinaryPowerBombHits(bus, startX, startY, byte.MaxValue, loaded.Samus);
            if (hits != 2 || state.Body.CurrentInstruction != expectedList || state.FightFunction != CrocomireFightFunction.PowerBombCharge)
                throw new InvalidDataException("Power Bomb collision did not select the native mouth-dependent charge list.");

            // Independent, bounded schedule walker: only timed maps, dust/sound,
            // four-pixel moves, goto, and the charge counter. No production AI
            // stepping or collision helper is used to derive expected positions.
            ushort cursor = expectedList, map = state.Body.SpritemapPointer;
            int timer = 1, x = startX, steps = ReadWord(bus, CrocomirePowerBombFixtureData.StepCount);
            int moves = 0, firstMove = -1;
            bool complete = false;
            for (int frame = 0; frame < 300; frame++)
            {
                if (--timer == 0)
                {
                    for (int instruction = 0; instruction < 32; instruction++)
                    {
                        ushort word = ReadWord(bus, 0xa40000 | cursor);
                        cursor += 2;
                        if (word < 0x8000)
                        { timer = word; map = ReadWord(bus, 0xa40000 | cursor); cursor += 2; break; }
                        if (word == CrocomirePowerBombFixtureData.Goto)
                        { cursor = ReadWord(bus, 0xa40000 | cursor); continue; }
                        if (word == CrocomireCodePointers.Instruction_Crocomire_FightAI)
                        { if (--steps < 2) { complete = true; break; } continue; }
                        if (word is CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels or
                            CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud)
                        { x -= 4; moves++; if (firstMove < 0) firstMove = frame; continue; }
                        if (word is CrocomireCodePointers.Instruction_Crocomire_ShakeScreen or
                            CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX) continue;
                        if (word >= CrocomirePowerBombFixtureData.FirstDust && word <= CrocomirePowerBombFixtureData.LastDust &&
                            (word - CrocomirePowerBombFixtureData.FirstDust) % 5 == 0) continue;
                        throw new InvalidDataException($"Unaccounted Power Bomb schedule opcode {word:X4}.");
                    }
                    if (timer == 0 && !complete) throw new InvalidDataException("Schedule did not reach a timed frame.");
                }
                Step(loaded);
                Step(control);
                if (RequireState(control).Body.XPosition != startX ||
                    RequireState(control).FightFunction == CrocomireFightFunction.PowerBombCharge)
                    throw new InvalidDataException("No-Power-Bomb control advanced or entered the reaction unexpectedly.");
                if (state.Body.XPosition != x || state.Body.YPosition != startY)
                    throw new InvalidDataException($"{expectedList:X4} frame {frame}: expected ({x},{startY}), actual ({state.Body.XPosition},{state.Body.YPosition}).");
                if (complete)
                {
                    // The returned step-forward list immediately evaluates random
                    // attack selection before publishing its next timed frame.
                    if (state.FightFunction is not (CrocomireFightFunction.SteppingForward or CrocomireFightFunction.ProjectileAttack) ||
                        state.StepCounter != 0 || moves != 22)
                        throw new InvalidDataException($"Charge did not complete two native eleven-move loops: moves={moves}, fight={state.FightFunction}, steps={state.StepCounter}, frame={frame}, initialSteps={ReadWord(bus, CrocomirePowerBombFixtureData.StepCount)}.");
                    Console.WriteLine($"PB {expectedList:X4}: first move {firstMove}, handoff {frame}; {moves} exact -4px moves, displacement {x - startX}; every preceding timed spritemap matches.");
                    break;
                }
                if (state.Body.SpritemapPointer != map)
                    throw new InvalidDataException($"{expectedList:X4} frame {frame}: charge animation differs from native list timing.");
            }
            if (!complete) throw new InvalidDataException("Charge schedule did not finish.");

            ushort SelectList()
            {
                int address = 0xa40000 | state.Body.SpritemapPointer;
                int count = ReadWord(bus, address);
                for (int i = 0; i < count; i++)
                {
                    ushort component = ReadWord(bus, address + 6 + i * 8);
                    if (component == CrocomirePowerBombFixtureData.FullyOpenComponent) return CrocomirePowerBombFixtureData.ReactionLists[0];
                    if (component == CrocomirePowerBombFixtureData.PartlyOpenComponent) return CrocomirePowerBombFixtureData.ReactionLists[1];
                }
                return CrocomirePowerBombFixtureData.ReactionLists[2];
            }
        }
        return 0;
    }
}
