﻿namespace ARMeilleure.Decoders
{
    class OpCode32Simd : OpCode32SimdBase
    {
        public int Opc { get; protected set; }
        public bool Q { get; protected set; }
        public bool F { get; protected set; }
        public bool U { get; private set; }

        public OpCode32Simd(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size = (opCode >> 20) & 0x3;
            Q = ((opCode >> 6) & 0x1) != 0;
            F = ((opCode >> 10) & 0x1) != 0;
            U = ((opCode >> 24) & 0x1) != 0;
            Opc = ((opCode >> 7) & 0x3);

            RegisterSize = Q ? RegisterSize.Simd128 : RegisterSize.Simd64;

            Vd = ((opCode >> 18) & 0x10) | ((opCode >> 12) & 0xf);
            Vm = ((opCode >> 1) & 0x10) | ((opCode >> 0) & 0xf);

            if (this.GetType() == typeof(OpCode32Simd)) // Subclasses have their own handling of Vx to account for before checking.
            {
                DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm);
            }
        }
    }
}
