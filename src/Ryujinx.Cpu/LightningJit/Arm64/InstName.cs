namespace Ryujinx.Cpu.LightningJit.Arm64
{
    enum InstName
    {
        Abs,
        AbsAdvsimdS,
        AbsAdvsimdV,
        Adc,
        Adcs,
        AddAddsubExt,
        AddAddsubImm,
        AddAddsubShift,
        AddAdvsimdS,
        AddAdvsimdV,
        Addg,
        AddhnAdvsimd,
        AddpAdvsimdPair,
        AddpAdvsimdVec,
        AddsAddsubExt,
        AddsAddsubImm,
        AddsAddsubShift,
        AddvAdvsimd,
        Adr,
        Adrp,
        AesdAdvsimd,
        AeseAdvsimd,
        AesimcAdvsimd,
        AesmcAdvsimd,
        AndAdvsimd,
        AndLogImm,
        AndLogShift,
        AndsLogImm,
        AndsLogShift,
        Asrv,
        Autda,
        Autdb,
        AutiaGeneral,
        AutiaSystem,
        AutibGeneral,
        AutibSystem,
        Axflag,
        BcaxAdvsimd,
        BcCond,
        BCond,
        BfcvtFloat,
        BfcvtnAdvsimd,
        BfdotAdvsimdElt,
        BfdotAdvsimdVec,
        Bfm,
        BfmlalAdvsimdElt,
        BfmlalAdvsimdVec,
        BfmmlaAdvsimd,
        BicAdvsimdImm,
        BicAdvsimdReg,
        BicLogShift,
        Bics,
        BifAdvsimd,
        BitAdvsimd,
        Bl,
        Blr,
        Blra,
        Br,
        Bra,
        Brk,
        BslAdvsimd,
        Bti,
        BUncond,
        Cas,
        Casb,
        Cash,
        Casp,
        Cbnz,
        Cbz,
        CcmnImm,
        CcmnReg,
        CcmpImm,
        CcmpReg,
        Cfinv,
        Chkfeat,
        Clrbhb,
        Clrex,
        ClsAdvsimd,
        ClsInt,
        ClzAdvsimd,
        ClzInt,
        CmeqAdvsimdRegS,
        CmeqAdvsimdRegV,
        CmeqAdvsimdZeroS,
        CmeqAdvsimdZeroV,
        CmgeAdvsimdRegS,
        CmgeAdvsimdRegV,
        CmgeAdvsimdZeroS,
        CmgeAdvsimdZeroV,
        CmgtAdvsimdRegS,
        CmgtAdvsimdRegV,
        CmgtAdvsimdZeroS,
        CmgtAdvsimdZeroV,
        CmhiAdvsimdS,
        CmhiAdvsimdV,
        CmhsAdvsimdS,
        CmhsAdvsimdV,
        CmleAdvsimdS,
        CmleAdvsimdV,
        CmltAdvsimdS,
        CmltAdvsimdV,
        CmtstAdvsimdS,
        CmtstAdvsimdV,
        Cnt,
        CntAdvsimd,
        Cpyfp,
        Cpyfpn,
        Cpyfprn,
        Cpyfprt,
        Cpyfprtn,
        Cpyfprtrn,
        Cpyfprtwn,
        Cpyfpt,
        Cpyfptn,
        Cpyfptrn,
        Cpyfptwn,
        Cpyfpwn,
        Cpyfpwt,
        Cpyfpwtn,
        Cpyfpwtrn,
        Cpyfpwtwn,
        Cpyp,
        Cpypn,
        Cpyprn,
        Cpyprt,
        Cpyprtn,
        Cpyprtrn,
        Cpyprtwn,
        Cpypt,
        Cpyptn,
        Cpyptrn,
        Cpyptwn,
        Cpypwn,
        Cpypwt,
        Cpypwtn,
        Cpypwtrn,
        Cpypwtwn,
        Crc32,
        Crc32c,
        Csdb,
        Csel,
        Csinc,
        Csinv,
        Csneg,
        Ctz,
        Dcps1,
        Dcps2,
        Dcps3,
        Dgh,
        Dmb,
        Drps,
        DsbDsbMemory,
        DsbDsbNxs,
        DupAdvsimdEltScalarFromElement,
        DupAdvsimdEltVectorFromElement,
        DupAdvsimdGen,
        Eon,
        Eor3Advsimd,
        EorAdvsimd,
        EorLogImm,
        EorLogShift,
        Eret,
        Ereta,
        Esb,
        ExtAdvsimd,
        Extr,
        FabdAdvsimdS,
        FabdAdvsimdSH,
        FabdAdvsimdV,
        FabdAdvsimdVH,
        FabsAdvsimdHalf,
        FabsAdvsimdSingleAndDouble,
        FabsFloat,
        FacgeAdvsimdS,
        FacgeAdvsimdSH,
        FacgeAdvsimdV,
        FacgeAdvsimdVH,
        FacgtAdvsimdS,
        FacgtAdvsimdSH,
        FacgtAdvsimdV,
        FacgtAdvsimdVH,
        FaddAdvsimdHalf,
        FaddAdvsimdSingleAndDouble,
        FaddFloat,
        FaddpAdvsimdPairHalf,
        FaddpAdvsimdPairSingleAndDouble,
        FaddpAdvsimdVecHalf,
        FaddpAdvsimdVecSingleAndDouble,
        FcaddAdvsimdVec,
        FccmpeFloat,
        FccmpFloat,
        FcmeqAdvsimdRegS,
        FcmeqAdvsimdRegSH,
        FcmeqAdvsimdRegV,
        FcmeqAdvsimdRegVH,
        FcmeqAdvsimdZeroS,
        FcmeqAdvsimdZeroSH,
        FcmeqAdvsimdZeroV,
        FcmeqAdvsimdZeroVH,
        FcmgeAdvsimdRegS,
        FcmgeAdvsimdRegSH,
        FcmgeAdvsimdRegV,
        FcmgeAdvsimdRegVH,
        FcmgeAdvsimdZeroS,
        FcmgeAdvsimdZeroSH,
        FcmgeAdvsimdZeroV,
        FcmgeAdvsimdZeroVH,
        FcmgtAdvsimdRegS,
        FcmgtAdvsimdRegSH,
        FcmgtAdvsimdRegV,
        FcmgtAdvsimdRegVH,
        FcmgtAdvsimdZeroS,
        FcmgtAdvsimdZeroSH,
        FcmgtAdvsimdZeroV,
        FcmgtAdvsimdZeroVH,
        FcmlaAdvsimdElt,
        FcmlaAdvsimdVec,
        FcmleAdvsimdS,
        FcmleAdvsimdSH,
        FcmleAdvsimdV,
        FcmleAdvsimdVH,
        FcmltAdvsimdS,
        FcmltAdvsimdSH,
        FcmltAdvsimdV,
        FcmltAdvsimdVH,
        FcmpeFloat,
        FcmpFloat,
        FcselFloat,
        FcvtasAdvsimdS,
        FcvtasAdvsimdSH,
        FcvtasAdvsimdV,
        FcvtasAdvsimdVH,
        FcvtasFloat,
        FcvtauAdvsimdS,
        FcvtauAdvsimdSH,
        FcvtauAdvsimdV,
        FcvtauAdvsimdVH,
        FcvtauFloat,
        FcvtFloat,
        FcvtlAdvsimd,
        FcvtmsAdvsimdS,
        FcvtmsAdvsimdSH,
        FcvtmsAdvsimdV,
        FcvtmsAdvsimdVH,
        FcvtmsFloat,
        FcvtmuAdvsimdS,
        FcvtmuAdvsimdSH,
        FcvtmuAdvsimdV,
        FcvtmuAdvsimdVH,
        FcvtmuFloat,
        FcvtnAdvsimd,
        FcvtnsAdvsimdS,
        FcvtnsAdvsimdSH,
        FcvtnsAdvsimdV,
        FcvtnsAdvsimdVH,
        FcvtnsFloat,
        FcvtnuAdvsimdS,
        FcvtnuAdvsimdSH,
        FcvtnuAdvsimdV,
        FcvtnuAdvsimdVH,
        FcvtnuFloat,
        FcvtpsAdvsimdS,
        FcvtpsAdvsimdSH,
        FcvtpsAdvsimdV,
        FcvtpsAdvsimdVH,
        FcvtpsFloat,
        FcvtpuAdvsimdS,
        FcvtpuAdvsimdSH,
        FcvtpuAdvsimdV,
        FcvtpuAdvsimdVH,
        FcvtpuFloat,
        FcvtxnAdvsimdS,
        FcvtxnAdvsimdV,
        FcvtzsAdvsimdFixS,
        FcvtzsAdvsimdFixV,
        FcvtzsAdvsimdIntS,
        FcvtzsAdvsimdIntSH,
        FcvtzsAdvsimdIntV,
        FcvtzsAdvsimdIntVH,
        FcvtzsFloatFix,
        FcvtzsFloatInt,
        FcvtzuAdvsimdFixS,
        FcvtzuAdvsimdFixV,
        FcvtzuAdvsimdIntS,
        FcvtzuAdvsimdIntSH,
        FcvtzuAdvsimdIntV,
        FcvtzuAdvsimdIntVH,
        FcvtzuFloatFix,
        FcvtzuFloatInt,
        FdivAdvsimdHalf,
        FdivAdvsimdSingleAndDouble,
        FdivFloat,
        Fjcvtzs,
        FmaddFloat,
        FmaxAdvsimdHalf,
        FmaxAdvsimdSingleAndDouble,
        FmaxFloat,
        FmaxnmAdvsimdHalf,
        FmaxnmAdvsimdSingleAndDouble,
        FmaxnmFloat,
        FmaxnmpAdvsimdPairHalf,
        FmaxnmpAdvsimdPairSingleAndDouble,
        FmaxnmpAdvsimdVecHalf,
        FmaxnmpAdvsimdVecSingleAndDouble,
        FmaxnmvAdvsimdHalf,
        FmaxnmvAdvsimdSingleAndDouble,
        FmaxpAdvsimdPairHalf,
        FmaxpAdvsimdPairSingleAndDouble,
        FmaxpAdvsimdVecHalf,
        FmaxpAdvsimdVecSingleAndDouble,
        FmaxvAdvsimdHalf,
        FmaxvAdvsimdSingleAndDouble,
        FminAdvsimdHalf,
        FminAdvsimdSingleAndDouble,
        FminFloat,
        FminnmAdvsimdHalf,
        FminnmAdvsimdSingleAndDouble,
        FminnmFloat,
        FminnmpAdvsimdPairHalf,
        FminnmpAdvsimdPairSingleAndDouble,
        FminnmpAdvsimdVecHalf,
        FminnmpAdvsimdVecSingleAndDouble,
        FminnmvAdvsimdHalf,
        FminnmvAdvsimdSingleAndDouble,
        FminpAdvsimdPairHalf,
        FminpAdvsimdPairSingleAndDouble,
        FminpAdvsimdVecHalf,
        FminpAdvsimdVecSingleAndDouble,
        FminvAdvsimdHalf,
        FminvAdvsimdSingleAndDouble,
        FmlaAdvsimdElt2regElementHalf,
        FmlaAdvsimdElt2regElementSingleAndDouble,
        FmlaAdvsimdElt2regScalarHalf,
        FmlaAdvsimdElt2regScalarSingleAndDouble,
        FmlaAdvsimdVecHalf,
        FmlaAdvsimdVecSingleAndDouble,
        FmlalAdvsimdEltFmlal,
        FmlalAdvsimdEltFmlal2,
        FmlalAdvsimdVecFmlal,
        FmlalAdvsimdVecFmlal2,
        FmlsAdvsimdElt2regElementHalf,
        FmlsAdvsimdElt2regElementSingleAndDouble,
        FmlsAdvsimdElt2regScalarHalf,
        FmlsAdvsimdElt2regScalarSingleAndDouble,
        FmlsAdvsimdVecHalf,
        FmlsAdvsimdVecSingleAndDouble,
        FmlslAdvsimdEltFmlsl,
        FmlslAdvsimdEltFmlsl2,
        FmlslAdvsimdVecFmlsl,
        FmlslAdvsimdVecFmlsl2,
        FmovAdvsimdPerHalf,
        FmovAdvsimdSingleAndDouble,
        FmovFloat,
        FmovFloatGen,
        FmovFloatImm,
        FmsubFloat,
        FmulAdvsimdElt2regElementHalf,
        FmulAdvsimdElt2regElementSingleAndDouble,
        FmulAdvsimdElt2regScalarHalf,
        FmulAdvsimdElt2regScalarSingleAndDouble,
        FmulAdvsimdVecHalf,
        FmulAdvsimdVecSingleAndDouble,
        FmulFloat,
        FmulxAdvsimdElt2regElementHalf,
        FmulxAdvsimdElt2regElementSingleAndDouble,
        FmulxAdvsimdElt2regScalarHalf,
        FmulxAdvsimdElt2regScalarSingleAndDouble,
        FmulxAdvsimdVecS,
        FmulxAdvsimdVecSH,
        FmulxAdvsimdVecV,
        FmulxAdvsimdVecVH,
        FnegAdvsimdHalf,
        FnegAdvsimdSingleAndDouble,
        FnegFloat,
        FnmaddFloat,
        FnmsubFloat,
        FnmulFloat,
        FrecpeAdvsimdS,
        FrecpeAdvsimdSH,
        FrecpeAdvsimdV,
        FrecpeAdvsimdVH,
        FrecpsAdvsimdS,
        FrecpsAdvsimdSH,
        FrecpsAdvsimdV,
        FrecpsAdvsimdVH,
        FrecpxAdvsimdHalf,
        FrecpxAdvsimdSingleAndDouble,
        Frint32xAdvsimd,
        Frint32xFloat,
        Frint32zAdvsimd,
        Frint32zFloat,
        Frint64xAdvsimd,
        Frint64xFloat,
        Frint64zAdvsimd,
        Frint64zFloat,
        FrintaAdvsimdHalf,
        FrintaAdvsimdSingleAndDouble,
        FrintaFloat,
        FrintiAdvsimdHalf,
        FrintiAdvsimdSingleAndDouble,
        FrintiFloat,
        FrintmAdvsimdHalf,
        FrintmAdvsimdSingleAndDouble,
        FrintmFloat,
        FrintnAdvsimdHalf,
        FrintnAdvsimdSingleAndDouble,
        FrintnFloat,
        FrintpAdvsimdHalf,
        FrintpAdvsimdSingleAndDouble,
        FrintpFloat,
        FrintxAdvsimdHalf,
        FrintxAdvsimdSingleAndDouble,
        FrintxFloat,
        FrintzAdvsimdHalf,
        FrintzAdvsimdSingleAndDouble,
        FrintzFloat,
        FrsqrteAdvsimdS,
        FrsqrteAdvsimdSH,
        FrsqrteAdvsimdV,
        FrsqrteAdvsimdVH,
        FrsqrtsAdvsimdS,
        FrsqrtsAdvsimdSH,
        FrsqrtsAdvsimdV,
        FrsqrtsAdvsimdVH,
        FsqrtAdvsimdHalf,
        FsqrtAdvsimdSingleAndDouble,
        FsqrtFloat,
        FsubAdvsimdHalf,
        FsubAdvsimdSingleAndDouble,
        FsubFloat,
        Gcsb,
        Gcsstr,
        Gcssttr,
        Gmi,
        Hint,
        Hlt,
        Hvc,
        InsAdvsimdElt,
        InsAdvsimdGen,
        Irg,
        Isb,
        Ld1AdvsimdMultAsNoPostIndex,
        Ld1AdvsimdMultAsPostIndex,
        Ld1AdvsimdSnglAsNoPostIndex,
        Ld1AdvsimdSnglAsPostIndex,
        Ld1rAdvsimdAsNoPostIndex,
        Ld1rAdvsimdAsPostIndex,
        Ld2AdvsimdMultAsNoPostIndex,
        Ld2AdvsimdMultAsPostIndex,
        Ld2AdvsimdSnglAsNoPostIndex,
        Ld2AdvsimdSnglAsPostIndex,
        Ld2rAdvsimdAsNoPostIndex,
        Ld2rAdvsimdAsPostIndex,
        Ld3AdvsimdMultAsNoPostIndex,
        Ld3AdvsimdMultAsPostIndex,
        Ld3AdvsimdSnglAsNoPostIndex,
        Ld3AdvsimdSnglAsPostIndex,
        Ld3rAdvsimdAsNoPostIndex,
        Ld3rAdvsimdAsPostIndex,
        Ld4AdvsimdMultAsNoPostIndex,
        Ld4AdvsimdMultAsPostIndex,
        Ld4AdvsimdSnglAsNoPostIndex,
        Ld4AdvsimdSnglAsPostIndex,
        Ld4rAdvsimdAsNoPostIndex,
        Ld4rAdvsimdAsPostIndex,
        Ld64b,
        Ldadd,
        Ldaddb,
        Ldaddh,
        Ldap1AdvsimdSngl,
        Ldaprb,
        LdaprBaseRegister,
        Ldaprh,
        LdaprPostIndexed,
        Ldapurb,
        LdapurFpsimd,
        LdapurGen,
        Ldapurh,
        Ldapursb,
        Ldapursh,
        Ldapursw,
        Ldar,
        Ldarb,
        Ldarh,
        Ldaxp,
        Ldaxr,
        Ldaxrb,
        Ldaxrh,
        Ldclr,
        Ldclrb,
        Ldclrh,
        Ldclrp,
        Ldeor,
        Ldeorb,
        Ldeorh,
        Ldg,
        Ldgm,
        Ldiapp,
        Ldlar,
        Ldlarb,
        Ldlarh,
        LdnpFpsimd,
        LdnpGen,
        LdpFpsimdPostIndexed,
        LdpFpsimdPreIndexed,
        LdpFpsimdSignedScaledOffset,
        LdpGenPostIndexed,
        LdpGenPreIndexed,
        LdpGenSignedScaledOffset,
        LdpswPostIndexed,
        LdpswPreIndexed,
        LdpswSignedScaledOffset,
        Ldra,
        LdrbImmPostIndexed,
        LdrbImmPreIndexed,
        LdrbImmUnsignedScaledOffset,
        LdrbReg,
        LdrhImmPostIndexed,
        LdrhImmPreIndexed,
        LdrhImmUnsignedScaledOffset,
        LdrhReg,
        LdrImmFpsimdPostIndexed,
        LdrImmFpsimdPreIndexed,
        LdrImmFpsimdUnsignedScaledOffset,
        LdrImmGenPostIndexed,
        LdrImmGenPreIndexed,
        LdrImmGenUnsignedScaledOffset,
        LdrLitFpsimd,
        LdrLitGen,
        LdrRegFpsimd,
        LdrRegGen,
        LdrsbImmPostIndexed,
        LdrsbImmPreIndexed,
        LdrsbImmUnsignedScaledOffset,
        LdrsbReg,
        LdrshImmPostIndexed,
        LdrshImmPreIndexed,
        LdrshImmUnsignedScaledOffset,
        LdrshReg,
        LdrswImmPostIndexed,
        LdrswImmPreIndexed,
        LdrswImmUnsignedScaledOffset,
        LdrswLit,
        LdrswReg,
        Ldset,
        Ldsetb,
        Ldseth,
        Ldsetp,
        Ldsmax,
        Ldsmaxb,
        Ldsmaxh,
        Ldsmin,
        Ldsminb,
        Ldsminh,
        Ldtr,
        Ldtrb,
        Ldtrh,
        Ldtrsb,
        Ldtrsh,
        Ldtrsw,
        Ldumax,
        Ldumaxb,
        Ldumaxh,
        Ldumin,
        Lduminb,
        Lduminh,
        Ldurb,
        LdurFpsimd,
        LdurGen,
        Ldurh,
        Ldursb,
        Ldursh,
        Ldursw,
        Ldxp,
        Ldxr,
        Ldxrb,
        Ldxrh,
        Lslv,
        Lsrv,
        Madd,
        MlaAdvsimdElt,
        MlaAdvsimdVec,
        MlsAdvsimdElt,
        MlsAdvsimdVec,
        MoviAdvsimd,
        Movk,
        Movn,
        Movz,
        Mrrs,
        Mrs,
        MsrImm,
        Msrr,
        MsrReg,
        Msub,
        MulAdvsimdElt,
        MulAdvsimdVec,
        MvniAdvsimd,
        NegAdvsimdS,
        NegAdvsimdV,
        Nop,
        NotAdvsimd,
        OrnAdvsimd,
        OrnLogShift,
        OrrAdvsimdImm,
        OrrAdvsimdReg,
        OrrLogImm,
        OrrLogShift,
        Pacda,
        Pacdb,
        Pacga,
        PaciaGeneral,
        PaciaSystem,
        PacibGeneral,
        PacibSystem,
        PmulAdvsimd,
        PmullAdvsimd,
        PrfmImm,
        PrfmLit,
        PrfmReg,
        Prfum,
        Psb,
        RaddhnAdvsimd,
        Rax1Advsimd,
        RbitAdvsimd,
        RbitInt,
        Rcwcas,
        Rcwcasp,
        Rcwclr,
        Rcwclrp,
        Rcwscas,
        Rcwscasp,
        Rcwsclr,
        Rcwsclrp,
        Rcwset,
        Rcwsetp,
        Rcwsset,
        Rcwssetp,
        Rcwsswp,
        Rcwsswpp,
        Rcwswp,
        Rcwswpp,
        Ret,
        Reta,
        Rev,
        Rev16Advsimd,
        Rev16Int,
        Rev32Advsimd,
        Rev32Int,
        Rev64Advsimd,
        Rmif,
        Rorv,
        RprfmReg,
        RshrnAdvsimd,
        RsubhnAdvsimd,
        SabaAdvsimd,
        SabalAdvsimd,
        SabdAdvsimd,
        SabdlAdvsimd,
        SadalpAdvsimd,
        SaddlAdvsimd,
        SaddlpAdvsimd,
        SaddlvAdvsimd,
        SaddwAdvsimd,
        Sb,
        Sbc,
        Sbcs,
        Sbfm,
        ScvtfAdvsimdFixS,
        ScvtfAdvsimdFixV,
        ScvtfAdvsimdIntS,
        ScvtfAdvsimdIntSH,
        ScvtfAdvsimdIntV,
        ScvtfAdvsimdIntVH,
        ScvtfFloatFix,
        ScvtfFloatInt,
        Sdiv,
        SdotAdvsimdElt,
        SdotAdvsimdVec,
        Setf,
        Setgp,
        Setgpn,
        Setgpt,
        Setgptn,
        Setp,
        Setpn,
        Setpt,
        Setptn,
        Sev,
        Sevl,
        Sha1cAdvsimd,
        Sha1hAdvsimd,
        Sha1mAdvsimd,
        Sha1pAdvsimd,
        Sha1su0Advsimd,
        Sha1su1Advsimd,
        Sha256h2Advsimd,
        Sha256hAdvsimd,
        Sha256su0Advsimd,
        Sha256su1Advsimd,
        Sha512h2Advsimd,
        Sha512hAdvsimd,
        Sha512su0Advsimd,
        Sha512su1Advsimd,
        ShaddAdvsimd,
        ShlAdvsimdS,
        ShlAdvsimdV,
        ShllAdvsimd,
        ShrnAdvsimd,
        ShsubAdvsimd,
        SliAdvsimdS,
        SliAdvsimdV,
        Sm3partw1Advsimd,
        Sm3partw2Advsimd,
        Sm3ss1Advsimd,
        Sm3tt1aAdvsimd,
        Sm3tt1bAdvsimd,
        Sm3tt2aAdvsimd,
        Sm3tt2bAdvsimd,
        Sm4eAdvsimd,
        Sm4ekeyAdvsimd,
        Smaddl,
        SmaxAdvsimd,
        SmaxImm,
        SmaxpAdvsimd,
        SmaxReg,
        SmaxvAdvsimd,
        Smc,
        SminAdvsimd,
        SminImm,
        SminpAdvsimd,
        SminReg,
        SminvAdvsimd,
        SmlalAdvsimdElt,
        SmlalAdvsimdVec,
        SmlslAdvsimdElt,
        SmlslAdvsimdVec,
        SmmlaAdvsimdVec,
        SmovAdvsimd,
        Smsubl,
        Smulh,
        SmullAdvsimdElt,
        SmullAdvsimdVec,
        SqabsAdvsimdS,
        SqabsAdvsimdV,
        SqaddAdvsimdS,
        SqaddAdvsimdV,
        SqdmlalAdvsimdElt2regElement,
        SqdmlalAdvsimdElt2regScalar,
        SqdmlalAdvsimdVecS,
        SqdmlalAdvsimdVecV,
        SqdmlslAdvsimdElt2regElement,
        SqdmlslAdvsimdElt2regScalar,
        SqdmlslAdvsimdVecS,
        SqdmlslAdvsimdVecV,
        SqdmulhAdvsimdElt2regElement,
        SqdmulhAdvsimdElt2regScalar,
        SqdmulhAdvsimdVecS,
        SqdmulhAdvsimdVecV,
        SqdmullAdvsimdElt2regElement,
        SqdmullAdvsimdElt2regScalar,
        SqdmullAdvsimdVecS,
        SqdmullAdvsimdVecV,
        SqnegAdvsimdS,
        SqnegAdvsimdV,
        SqrdmlahAdvsimdElt2regElement,
        SqrdmlahAdvsimdElt2regScalar,
        SqrdmlahAdvsimdVecS,
        SqrdmlahAdvsimdVecV,
        SqrdmlshAdvsimdElt2regElement,
        SqrdmlshAdvsimdElt2regScalar,
        SqrdmlshAdvsimdVecS,
        SqrdmlshAdvsimdVecV,
        SqrdmulhAdvsimdElt2regElement,
        SqrdmulhAdvsimdElt2regScalar,
        SqrdmulhAdvsimdVecS,
        SqrdmulhAdvsimdVecV,
        SqrshlAdvsimdS,
        SqrshlAdvsimdV,
        SqrshrnAdvsimdS,
        SqrshrnAdvsimdV,
        SqrshrunAdvsimdS,
        SqrshrunAdvsimdV,
        SqshlAdvsimdImmS,
        SqshlAdvsimdImmV,
        SqshlAdvsimdRegS,
        SqshlAdvsimdRegV,
        SqshluAdvsimdS,
        SqshluAdvsimdV,
        SqshrnAdvsimdS,
        SqshrnAdvsimdV,
        SqshrunAdvsimdS,
        SqshrunAdvsimdV,
        SqsubAdvsimdS,
        SqsubAdvsimdV,
        SqxtnAdvsimdS,
        SqxtnAdvsimdV,
        SqxtunAdvsimdS,
        SqxtunAdvsimdV,
        SrhaddAdvsimd,
        SriAdvsimdS,
        SriAdvsimdV,
        SrshlAdvsimdS,
        SrshlAdvsimdV,
        SrshrAdvsimdS,
        SrshrAdvsimdV,
        SrsraAdvsimdS,
        SrsraAdvsimdV,
        SshlAdvsimdS,
        SshlAdvsimdV,
        SshllAdvsimd,
        SshrAdvsimdS,
        SshrAdvsimdV,
        SsraAdvsimdS,
        SsraAdvsimdV,
        SsublAdvsimd,
        SsubwAdvsimd,
        St1AdvsimdMultAsNoPostIndex,
        St1AdvsimdMultAsPostIndex,
        St1AdvsimdSnglAsNoPostIndex,
        St1AdvsimdSnglAsPostIndex,
        St2AdvsimdMultAsNoPostIndex,
        St2AdvsimdMultAsPostIndex,
        St2AdvsimdSnglAsNoPostIndex,
        St2AdvsimdSnglAsPostIndex,
        St2gPostIndexed,
        St2gPreIndexed,
        St2gSignedScaledOffset,
        St3AdvsimdMultAsNoPostIndex,
        St3AdvsimdMultAsPostIndex,
        St3AdvsimdSnglAsNoPostIndex,
        St3AdvsimdSnglAsPostIndex,
        St4AdvsimdMultAsNoPostIndex,
        St4AdvsimdMultAsPostIndex,
        St4AdvsimdSnglAsNoPostIndex,
        St4AdvsimdSnglAsPostIndex,
        St64b,
        St64bv,
        St64bv0,
        Stgm,
        StgPostIndexed,
        StgpPostIndexed,
        StgpPreIndexed,
        StgPreIndexed,
        StgpSignedScaledOffset,
        StgSignedScaledOffset,
        Stilp,
        Stl1AdvsimdSngl,
        Stllr,
        Stllrb,
        Stllrh,
        Stlrb,
        StlrBaseRegister,
        Stlrh,
        StlrPreIndexed,
        Stlurb,
        StlurFpsimd,
        StlurGen,
        Stlurh,
        Stlxp,
        Stlxr,
        Stlxrb,
        Stlxrh,
        StnpFpsimd,
        StnpGen,
        StpFpsimdPostIndexed,
        StpFpsimdPreIndexed,
        StpFpsimdSignedScaledOffset,
        StpGenPostIndexed,
        StpGenPreIndexed,
        StpGenSignedScaledOffset,
        StrbImmPostIndexed,
        StrbImmPreIndexed,
        StrbImmUnsignedScaledOffset,
        StrbReg,
        StrhImmPostIndexed,
        StrhImmPreIndexed,
        StrhImmUnsignedScaledOffset,
        StrhReg,
        StrImmFpsimdPostIndexed,
        StrImmFpsimdPreIndexed,
        StrImmFpsimdUnsignedScaledOffset,
        StrImmGenPostIndexed,
        StrImmGenPreIndexed,
        StrImmGenUnsignedScaledOffset,
        StrRegFpsimd,
        StrRegGen,
        Sttr,
        Sttrb,
        Sttrh,
        Sturb,
        SturFpsimd,
        SturGen,
        Sturh,
        Stxp,
        Stxr,
        Stxrb,
        Stxrh,
        Stz2gPostIndexed,
        Stz2gPreIndexed,
        Stz2gSignedScaledOffset,
        Stzgm,
        StzgPostIndexed,
        StzgPreIndexed,
        StzgSignedScaledOffset,
        SubAddsubExt,
        SubAddsubImm,
        SubAddsubShift,
        SubAdvsimdS,
        SubAdvsimdV,
        Subg,
        SubhnAdvsimd,
        Subp,
        Subps,
        SubsAddsubExt,
        SubsAddsubImm,
        SubsAddsubShift,
        SudotAdvsimdElt,
        SuqaddAdvsimdS,
        SuqaddAdvsimdV,
        Svc,
        Swp,
        Swpb,
        Swph,
        Swpp,
        Sys,
        Sysl,
        Sysp,
        TblAdvsimd,
        Tbnz,
        TbxAdvsimd,
        Tbz,
        Tcancel,
        Tcommit,
        Trn1Advsimd,
        Trn2Advsimd,
        Tsb,
        Tstart,
        Ttest,
        UabaAdvsimd,
        UabalAdvsimd,
        UabdAdvsimd,
        UabdlAdvsimd,
        UadalpAdvsimd,
        UaddlAdvsimd,
        UaddlpAdvsimd,
        UaddlvAdvsimd,
        UaddwAdvsimd,
        Ubfm,
        UcvtfAdvsimdFixS,
        UcvtfAdvsimdFixV,
        UcvtfAdvsimdIntS,
        UcvtfAdvsimdIntSH,
        UcvtfAdvsimdIntV,
        UcvtfAdvsimdIntVH,
        UcvtfFloatFix,
        UcvtfFloatInt,
        UdfPermUndef,
        Udiv,
        UdotAdvsimdElt,
        UdotAdvsimdVec,
        UhaddAdvsimd,
        UhsubAdvsimd,
        Umaddl,
        UmaxAdvsimd,
        UmaxImm,
        UmaxpAdvsimd,
        UmaxReg,
        UmaxvAdvsimd,
        UminAdvsimd,
        UminImm,
        UminpAdvsimd,
        UminReg,
        UminvAdvsimd,
        UmlalAdvsimdElt,
        UmlalAdvsimdVec,
        UmlslAdvsimdElt,
        UmlslAdvsimdVec,
        UmmlaAdvsimdVec,
        UmovAdvsimd,
        Umsubl,
        Umulh,
        UmullAdvsimdElt,
        UmullAdvsimdVec,
        UqaddAdvsimdS,
        UqaddAdvsimdV,
        UqrshlAdvsimdS,
        UqrshlAdvsimdV,
        UqrshrnAdvsimdS,
        UqrshrnAdvsimdV,
        UqshlAdvsimdImmS,
        UqshlAdvsimdImmV,
        UqshlAdvsimdRegS,
        UqshlAdvsimdRegV,
        UqshrnAdvsimdS,
        UqshrnAdvsimdV,
        UqsubAdvsimdS,
        UqsubAdvsimdV,
        UqxtnAdvsimdS,
        UqxtnAdvsimdV,
        UrecpeAdvsimd,
        UrhaddAdvsimd,
        UrshlAdvsimdS,
        UrshlAdvsimdV,
        UrshrAdvsimdS,
        UrshrAdvsimdV,
        UrsqrteAdvsimd,
        UrsraAdvsimdS,
        UrsraAdvsimdV,
        UsdotAdvsimdElt,
        UsdotAdvsimdVec,
        UshlAdvsimdS,
        UshlAdvsimdV,
        UshllAdvsimd,
        UshrAdvsimdS,
        UshrAdvsimdV,
        UsmmlaAdvsimdVec,
        UsqaddAdvsimdS,
        UsqaddAdvsimdV,
        UsraAdvsimdS,
        UsraAdvsimdV,
        UsublAdvsimd,
        UsubwAdvsimd,
        Uzp1Advsimd,
        Uzp2Advsimd,
        Wfe,
        Wfet,
        Wfi,
        Wfit,
        Xaflag,
        XarAdvsimd,
        XpacGeneral,
        XpacSystem,
        XtnAdvsimd,
        Yield,
        Zip1Advsimd,
        Zip2Advsimd,
    }

    static class InstNameExtensions
    {
        public static bool IsCall(this InstName name)
        {
            return name == InstName.Bl || name == InstName.Blr;
        }

        public static bool IsControlFlowOrException(this InstName name)
        {
            switch (name)
            {
                case InstName.BUncond:
                case InstName.BCond:
                case InstName.Bl:
                case InstName.Blr:
                case InstName.Br:
                case InstName.Brk:
                case InstName.Cbnz:
                case InstName.Cbz:
                case InstName.Ret:
                case InstName.Tbnz:
                case InstName.Tbz:
                case InstName.Svc:
                case InstName.UdfPermUndef:
                    return true;
            }

            return false;
        }

        public static bool IsException(this InstName name)
        {
            switch (name)
            {
                case InstName.Brk:
                case InstName.Svc:
                case InstName.UdfPermUndef:
                    return true;
            }

            return false;
        }

        public static bool IsSystem(this InstName name)
        {
            switch (name)
            {
                case InstName.Mrs:
                case InstName.MsrImm:
                case InstName.MsrReg:
                    return true;
            }

            return name.IsException();
        }

        public static bool IsSystemOrCall(this InstName name)
        {
            switch (name)
            {
                case InstName.Bl:
                case InstName.Blr:
                case InstName.Svc:
                case InstName.Mrs:
                case InstName.MsrImm:
                case InstName.MsrReg:
                    return true;
            }

            return false;
        }

        public static bool IsPrivileged(this InstName name)
        {
            switch (name)
            {
                case InstName.Dcps1:
                case InstName.Dcps2:
                case InstName.Dcps3:
                case InstName.Drps:
                case InstName.Eret:
                case InstName.Ereta:
                case InstName.Hvc:
                case InstName.MsrImm:
                case InstName.Smc:
                    return true;
            }

            return false;
        }

        public static bool IsRmwMemory(this InstName name)
        {
            switch (name)
            {
                case InstName.Ld1AdvsimdSnglAsNoPostIndex:
                case InstName.Ld1AdvsimdSnglAsPostIndex:
                case InstName.Ld2AdvsimdSnglAsNoPostIndex:
                case InstName.Ld2AdvsimdSnglAsPostIndex:
                case InstName.Ld3AdvsimdSnglAsNoPostIndex:
                case InstName.Ld3AdvsimdSnglAsPostIndex:
                case InstName.Ld4AdvsimdSnglAsNoPostIndex:
                case InstName.Ld4AdvsimdSnglAsPostIndex:
                    return true;
            }

            return false;
        }
    }
}
