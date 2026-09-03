namespace SuperMetroid.Core.Input;

/// <summary>Verified bank-$91 demo-controller object and bytecode definitions.</summary>
public static class DemoInputRomData
{
    public const int BankBase = 0x910000;

    public static class Routines
    {
        public const ushort NoOp = 0x83bf;
        public const ushort ClearedPreInstruction = 0x8447;
    }

    public static class Instructions
    {
        public const ushort OpcodeBit = 0x8000;
        public const ushort Delete = 0x8427;
        public const ushort SetPreInstruction = 0x8434;
        public const ushort ClearPreInstruction = 0x843f;
        public const ushort Goto = 0x8448;
        public const ushort DecrementTimerAndGoto = 0x844f;
        public const ushort SetTimer = 0x8459;
        public const int InputRecordBytes = 6;
        public const int WordBytes = sizeof(ushort);
    }

    public static class IntroMotherBrain
    {
        public const ushort Object = 0x8784;
        public const ushort InputList = 0x8694;
        public const ushort NextRecord = 0x86b8;
    }
}
