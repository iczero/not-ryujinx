﻿namespace ARMeilleure.Decoders
{
    interface IOpCode32AluBf
    {
        public int Rd { get; }
        public int Rn { get; }

        public int Msb { get; }
        public int Lsb { get; }
    }
}
