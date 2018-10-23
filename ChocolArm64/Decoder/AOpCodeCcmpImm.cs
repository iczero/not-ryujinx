using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeCcmpImm : AOpCodeCcmp, IAOpCodeAluImm
    {
        public long Imm => RmImm;

        public AOpCodeCcmpImm(AInst inst, long position, int opCode) : base(inst, position, opCode) { }
    }
}