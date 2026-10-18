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
}
