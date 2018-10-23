namespace Ryujinx.Graphics.Gal.Shader
{
    enum ShaderIrInst
    {
        Invalid,

        BStart,
        Band,
        Bnot,
        Bor,
        Bxor,
        BEnd,

        FStart,
        Ceil,

        Fabs,
        Fadd,
        Fceq,
        Fcequ,
        Fcge,
        Fcgeu,
        Fcgt,
        Fcgtu,
        Fclamp,
        Fcle,
        Fcleu,
        Fclt,
        Fcltu,
        Fcnan,
        Fcne,
        Fcneu,
        Fcnum,
        Fcos,
        Fex2,
        Ffma,
        Flg2,
        Floor,
        Fmax,
        Fmin,
        Fmul,
        Fneg,
        Frcp,
        Frsq,
        Fsin,
        Fsqrt,
        Ftos,
        Ftou,
        Ipa,
        Texb,
        Texs,
        Trunc,
        FEnd,

        Start,
        Abs,
        Add,
        And,
        Asr,
        Ceq,
        Cge,
        Cgt,
        Clamps,
        Clampu,
        Cle,
        Clt,
        Cne,
        Lsl,
        Lsr,
        Max,
        Min,
        Mul,
        Neg,
        Not,
        Or,
        Stof,
        Sub,
        Texq,
        Txlf,
        Utof,
        Xor,
        End,

        Bra,
        Exit,
        Kil,
        Ssy,
        Sync,

        Emit,
        Cut
    }
}