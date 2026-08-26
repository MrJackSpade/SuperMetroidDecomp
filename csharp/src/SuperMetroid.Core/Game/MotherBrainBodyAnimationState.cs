using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The bank-$A9 enemy-instruction state needed by Mother Brain's forward and backward walks.
/// </summary>
/// <remarks>
/// Mother Brain's body movement is not performed by her AI function. The AI installs one of
/// the ordinary enemy instruction lists at `$A9:9730-$9972`; bank `$A0:C26A` later advances
/// that bytecode and its bank-$A9 opcodes mutate body position, pose, BG2 alignment, footsteps,
/// and the displayed extended spritemap. Keeping that second processing stage explicit is
/// essential: replacing a requested walk with a direct X adjustment would skip roughly ninety
/// visible animation frames and would change when the AI observes pose zero again.
///
/// This class deliberately admits only the command family used by the translated walk lists.
/// Encountering any other command throws with its ROM address, making future animation work an
/// inspectable extension rather than silently treating unknown bytecode as a frame record.
/// </remarks>
public sealed class MotherBrainBodyAnimationState
{
    private const int InstructionBank = 0xa90000;

    /// <summary>Mother Brain body enemy-slot X position.</summary>
    public ushort XPosition { get; set; }

    /// <summary>Mother Brain body enemy-slot Y position.</summary>
    public ushort YPosition { get; set; }

    /// <summary>Body pose word: zero standing and one walking for the admitted programs.</summary>
    public ushort Pose { get; set; }

    /// <summary>Phase/form word consulted by the native footstep sound gate.</summary>
    public ushort Form { get; set; }

    /// <summary>Current bank-$A9 enemy instruction pointer.</summary>
    public ushort InstructionPointer { get; private set; }

    /// <summary>Post-decremented enemy instruction timer.</summary>
    public ushort InstructionTimer { get; private set; }

    /// <summary>Current extended spritemap pointer selected by a timed instruction record.</summary>
    public ushort SpritemapPointer { get; private set; }

    /// <summary>Mother Brain body BG2 X scroll maintained by `$A9:9579`.</summary>
    public ushort Bg2XScroll { get; private set; }

    /// <summary>Mother Brain body BG2 Y scroll maintained opposite to body Y movement.</summary>
    public ushort Bg2YScroll { get; private set; }

    /// <summary>True after common enemy instruction `$812F` pins the program counter.</summary>
    public bool Sleeping { get; private set; }

    /// <summary>
    /// Equivalent of `$A9:C42D`: install a body list, make it eligible on the next enemy
    /// instruction-processing stage, and clear the prior sleep state.
    /// </summary>
    public void SetInstructionList(ushort pointer)
    {
        InstructionPointer = pointer;
        InstructionTimer = 1;
        Sleeping = false;
    }

    /// <summary>
    /// Executes one `$A0:C26A` instruction-processing call using Mother Brain's bank `$A9`.
    /// </summary>
    public MotherBrainBodyAnimationStepResult Step(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);

        ushort xBefore = XPosition;
        ushort yBefore = YPosition;
        ushort poseBefore = Pose;
        ushort pointerBefore = InstructionPointer;
        ushort timerBefore = InstructionTimer;
        bool footstep = false;
        bool footstepSound = false;
        ushort earthquakeType = 0;
        ushort earthquakeTimer = 0;
        bool loadedFrame = false;

        // A sleeping list has returned null from the common `$812F` command. The native
        // timer is zero at that point and would merely wrap if called again; no bytecode runs.
        if (Sleeping)
            return CreateResult();

        // `$A0:C271` compares the OLD timer with one and decrements regardless. This is not
        // equivalent to pre-decrementing and checking zero when the word has wrapped.
        ushort oldTimer = InstructionTimer;
        InstructionTimer = unchecked((ushort)(InstructionTimer - 1));
        if (oldTimer != 1)
            return CreateResult();

        // Commands have bit 15 set and execute without consuming a display frame. Continue
        // until a duration/spritemap pair or a command that stops processing is encountered.
        for (int commandCount = 0; commandCount < 32; commandCount++)
        {
            ushort word = ReadWord(bus, InstructionPointer);
            if ((word & 0x8000) == 0)
            {
                InstructionTimer = word;
                SpritemapPointer = ReadWord(bus, unchecked((ushort)(InstructionPointer + 2)));
                InstructionPointer = unchecked((ushort)(InstructionPointer + 4));
                loadedFrame = true;
                return CreateResult();
            }

            ushort commandAddress = InstructionPointer;
            InstructionPointer = unchecked((ushort)(InstructionPointer + 2));
            switch (word)
            {
                case 0x812f: // Common enemy instruction: sleep.
                    // EnemyInstr_Sleep writes the address of the command itself back to the
                    // current-instruction word and returns null to stop this processing call.
                    InstructionPointer = commandAddress;
                    Sleeping = true;
                    return CreateResult();

                case 0x95fc: // X + 1, Y - 2.
                    MoveBody(1, -2);
                    break;

                case 0x960c: // X + 2.
                    MoveBody(2, 0);
                    break;

                case 0x961c: // Literal A=+1 into `$A9:9579`.
                    MoveBody(0, 1);
                    break;

                case 0x9622: // X + 3, Y + 1, footstep.
                    ApplyFootstep();
                    MoveBody(3, 1);
                    break;

                case 0x9638: // X + 15, Y - 2.
                    MoveBody(15, -2);
                    break;

                case 0x9648: // X + 6, Y - 4.
                    MoveBody(6, -4);
                    break;

                case 0x9658: // X - 2, Y + 4.
                    MoveBody(-2, 4);
                    break;

                case 0x9668: // X - 1, Y + 2, footstep.
                case 0x967e: // Duplicate opcode with a different discarded incoming A.
                    ApplyFootstep();
                    MoveBody(-1, 2);
                    break;

                case 0x9694: // X - 2.
                    MoveBody(-2, 0);
                    break;

                case 0x96a4: // Literal A=$FFFF into `$A9:9579`.
                    MoveBody(0, -1);
                    break;

                case 0x96aa: // X - 3, Y - 1.
                    MoveBody(-3, -1);
                    break;

                case 0x96ba: // X - 15, Y + 2, footstep.
                    ApplyFootstep();
                    MoveBody(-15, 2);
                    break;

                case 0x96d0: // X - 6, Y + 4.
                    MoveBody(-6, 4);
                    break;

                case 0x96e0: // X + 2, Y - 4.
                    MoveBody(2, -4);
                    break;

                case 0x96f0: // X + 1, Y - 2.
                    MoveBody(1, -2);
                    break;

                case 0x9700:
                    Pose = 0; // Standing.
                    break;

                case 0x9708:
                    Pose = 1; // Walking.
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported Mother Brain body instruction ${word:X4} " +
                        $"at $A9:{commandAddress:X4}.");
            }
        }

        throw new InvalidOperationException(
            "Mother Brain body instruction list did not reach a timed frame within 32 commands.");

        void ApplyFootstep()
        {
            // `$A9:9599` always requests earthquake 1 for four frames. Its attempted sound
            // `$16` is gated on form three; retain that otherwise silent distinction.
            footstep = true;
            earthquakeType = 1;
            earthquakeTimer = 4;
            footstepSound = Form == 3;
        }

        void MoveBody(int deltaX, int deltaY)
        {
            XPosition = unchecked((ushort)(XPosition + deltaX));
            YPosition = unchecked((ushort)(YPosition + deltaY));

            // Every admitted walking opcode ends in `$A9:9579`. The body moves in world
            // coordinates while BG2 is offset inversely so the composite remains attached.
            Bg2YScroll = unchecked((ushort)(Bg2YScroll - deltaY));
            Bg2XScroll = unchecked((ushort)(0x0022 - XPosition));
        }

        MotherBrainBodyAnimationStepResult CreateResult() => new(
            pointerBefore,
            InstructionPointer,
            timerBefore,
            InstructionTimer,
            xBefore,
            XPosition,
            yBefore,
            YPosition,
            poseBefore,
            Pose,
            SpritemapPointer,
            loadedFrame,
            Sleeping,
            footstep,
            footstepSound,
            earthquakeType,
            earthquakeTimer,
            Bg2XScroll,
            Bg2YScroll);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort address)
    {
        int cpuAddress = InstructionBank | address;
        byte low = bus.ReadByte(cpuAddress);
        byte high = bus.ReadByte(InstructionBank | unchecked((ushort)(address + 1)));
        return (ushort)(low | (high << 8));
    }
}

/// <summary>Debugger witness for one bank-$A0 enemy-instruction processing stage.</summary>
public readonly record struct MotherBrainBodyAnimationStepResult(
    ushort InstructionPointerBefore,
    ushort InstructionPointerAfter,
    ushort InstructionTimerBefore,
    ushort InstructionTimerAfter,
    ushort XBefore,
    ushort XAfter,
    ushort YBefore,
    ushort YAfter,
    ushort PoseBefore,
    ushort PoseAfter,
    ushort SpritemapPointer,
    bool LoadedFrame,
    bool Sleeping,
    bool FootstepRequested,
    bool FootstepSoundRequested,
    ushort EarthquakeType,
    ushort EarthquakeTimer,
    ushort Bg2XScroll,
    ushort Bg2YScroll);
