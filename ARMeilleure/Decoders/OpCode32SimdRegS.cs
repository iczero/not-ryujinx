﻿namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegS : OpCode32SimdS
    {
        public int Vn { get; private set; }

        public OpCode32SimdRegS(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            var single = Size != 0b11;
            if (single)
            {
                Vn = ((opCode >> 7) & 0x1) | ((opCode >> 15) & 0x1e);
            } 
            else
            {
                Vn = ((opCode >> 3) & 0x10) | ((opCode >> 16) & 0xf);
            }
        }
    }
}
