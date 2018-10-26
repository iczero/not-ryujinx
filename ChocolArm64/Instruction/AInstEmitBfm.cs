using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    internal static partial class AInstEmit
    {
        public static void Bfm(AILEmitterCtx context)
        {
            AOpCodeBfm op = (AOpCodeBfm)context.CurrOp;

            EmitBfmLoadRn(context);

            context.EmitLdintzr(op.Rd);
            context.EmitLdc_I(~op.WMask & op.TMask);

            context.Emit(OpCodes.And);
            context.Emit(OpCodes.Or);

            context.EmitLdintzr(op.Rd);
            context.EmitLdc_I(~op.TMask);

            context.Emit(OpCodes.And);
            context.Emit(OpCodes.Or);

            context.EmitStintzr(op.Rd);
        }

        public static void Sbfm(AILEmitterCtx context)
        {
            AOpCodeBfm op = (AOpCodeBfm)context.CurrOp;

            int bitsCount = op.GetBitsCount();

            if (op.Pos + 1 == bitsCount)
            {
                EmitSbfmShift(context);
            }
            else if (op.Pos < op.Shift)
            {
                EmitSbfiz(context);
            }
            else if (op.Pos == 7 && op.Shift == 0)
            {
                EmitSbfmCast(context, OpCodes.Conv_I1);
            }
            else if (op.Pos == 15 && op.Shift == 0)
            {
                EmitSbfmCast(context, OpCodes.Conv_I2);
            }
            else if (op.Pos == 31 && op.Shift == 0)
            {
                EmitSbfmCast(context, OpCodes.Conv_I4);
            }
            else
            {
                EmitBfmLoadRn(context);

                context.EmitLdintzr(op.Rn);

                context.EmitLsl(bitsCount - 1 - op.Pos);
                context.EmitAsr(bitsCount - 1);

                context.EmitLdc_I(~op.TMask);

                context.Emit(OpCodes.And);
                context.Emit(OpCodes.Or);

                context.EmitStintzr(op.Rd);
            }
        }

        public static void Ubfm(AILEmitterCtx context)
        {
            AOpCodeBfm op = (AOpCodeBfm)context.CurrOp;

            if (op.Pos + 1 == op.GetBitsCount())
            {
                EmitUbfmShift(context);
            }
            else if (op.Pos < op.Shift)
            {
                EmitUbfiz(context);
            }
            else if (op.Pos + 1 == op.Shift)
            {
                EmitBfmLsl(context);
            }
            else if (op.Pos == 7 && op.Shift == 0)
            {
                EmitUbfmCast(context, OpCodes.Conv_U1);
            }
            else if (op.Pos == 15 && op.Shift == 0)
            {
                EmitUbfmCast(context, OpCodes.Conv_U2);
            }
            else
            {
                EmitBfmLoadRn(context);

                context.EmitStintzr(op.Rd);
            }
        }

        private static void EmitSbfiz(AILEmitterCtx context)
        {
            EmitBfiz(context, true);
        }

        private static void EmitUbfiz(AILEmitterCtx context)
        {
            EmitBfiz(context, false);
        }

        private static void EmitBfiz(AILEmitterCtx context, bool signed)
        {
            AOpCodeBfm op = (AOpCodeBfm)context.CurrOp;

            int width = op.Pos + 1;

            context.EmitLdintzr(op.Rn);

            context.EmitLsl(op.GetBitsCount() - width);

            if (signed)
                context.EmitAsr(op.Shift - width);
            else
                context.EmitLsr(op.Shift - width);

            context.EmitStintzr(op.Rd);
        }

        private static void EmitSbfmCast(AILEmitterCtx context, OpCode ilOp)
        {
            EmitBfmCast(context, ilOp, true);
        }

        private static void EmitUbfmCast(AILEmitterCtx context, OpCode ilOp)
        {
            EmitBfmCast(context, ilOp, false);
        }

        private static void EmitBfmCast(AILEmitterCtx context, OpCode ilOp, bool signed)
        {
            AOpCodeBfm op = (AOpCodeBfm)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            context.Emit(ilOp);

            if (op.RegisterSize != ARegisterSize.Int32)
                context.Emit(signed
                    ? OpCodes.Conv_I8
                    : OpCodes.Conv_U8);

            context.EmitStintzr(op.Rd);
        }

        private static void EmitSbfmShift(AILEmitterCtx context)
        {
            EmitBfmShift(context, true);
        }

        private static void EmitUbfmShift(AILEmitterCtx context)
        {
            EmitBfmShift(context, false);
        }

        private static void EmitBfmShift(AILEmitterCtx context, bool signed)
        {
            AOpCodeBfm op = (AOpCodeBfm)context.CurrOp;

            context.EmitLdintzr(op.Rn);
            context.EmitLdc_I4(op.Shift);

            context.Emit(signed
                ? OpCodes.Shr
                : OpCodes.Shr_Un);

            context.EmitStintzr(op.Rd);
        }

        private static void EmitBfmLsl(AILEmitterCtx context)
        {
            AOpCodeBfm op = (AOpCodeBfm)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            context.EmitLsl(op.GetBitsCount() - op.Shift);

            context.EmitStintzr(op.Rd);
        }

        private static void EmitBfmLoadRn(AILEmitterCtx context)
        {
            AOpCodeBfm op = (AOpCodeBfm)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            context.EmitRor(op.Shift);

            context.EmitLdc_I(op.WMask & op.TMask);

            context.Emit(OpCodes.And);
        }
    }
}