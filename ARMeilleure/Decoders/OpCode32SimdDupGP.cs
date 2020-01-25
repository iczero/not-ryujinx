﻿namespace ARMeilleure.Decoders
{
    class OpCode32SimdDupGP : OpCode32, IOpCode32Simd
    {
        public int Size { get; private set; }
        public int Vd { get; private set; }
        public int Rt { get; private set; }
        public bool Q { get; private set; }

        public OpCode32SimdDupGP(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size = 2 - (((opCode >> 21) & 0x2) | ((opCode >> 5) & 0x1)); // B:E - 0 for 32, 16 then 8.
            if (Size == -1)
            {
                Instruction = InstDescriptor.Undefined;
            }
            Q = ((opCode >> 21) & 0x1) != 0;

            RegisterSize = Q ? RegisterSize.Simd128 : RegisterSize.Simd64;

            Vd = ((opCode >> 3) & 0x10) | ((opCode >> 16) & 0xf);
            Rt = ((opCode >> 12) & 0xf);

            if (DecoderHelper.VectorArgumentsInvalid(Q, Vd))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
