namespace SuperMetroid.Core.Input;

/// <summary>Verified bank-$91 demo-controller object and bytecode definitions.</summary>
public static class DemoInputRomData
{

    /// <summary>Bank-$91 return-only routine identities accepted by the demo object's initializer and pre-instruction dispatch.</summary>
    public static class Routines
    {
        /// <summary>$91:83BF, <c>RTS_9183BF</c>: performs no initialization or per-update pre-instruction work.</summary>
        public const ushort NoOp = 0x83bf;
        /// <summary>$91:8447, <c>Instruction_DemoInputObject_ClearPreInstruction.return</c>: the no-op RTS installed when the demo pre-instruction is cleared.</summary>
        public const ushort ClearedPreInstruction = 0x8447;
    }

    /// <summary>Title-demo-specific pre-instructions and their redirect destinations.</summary>
    public static class Attract
    {
        /// <summary>$91:8776, InstList_DemoInput_Delete: common demo object deletion list.</summary>
        public const ushort DeleteList = 0x8776;
        /// <summary>$91:9346, unused shinespark continuation targeted literally by $8AB0.</summary>
        public const ushort ShinesparkContinuation = 0x9346;
    }

    /// <summary>Bank-$91 demo-list control routines and the six-byte input-record layout consumed by the instruction interpreter.</summary>
    public static class Instructions
    {
        /// <summary>Leading-word bit $8000 tested at $91:8400-$8403: set selects an indirect bank-$91 routine address; clear selects an input-record duration.</summary>
        public const ushort OpcodeBit = 0x8000;
        /// <summary>Six bytes per input record: duration word, held-button word, and explicitly authored newly-pressed-button word; $91:8420 advances the cursor by this byte count.</summary>
        public const int InputRecordBytes = 6;
    }
}
