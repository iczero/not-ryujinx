namespace Ryujinx.Graphics
{
    internal enum NvGpuEngine3dReg
    {
        FrameBufferNAddress  = 0x200,
        FrameBufferNWidth    = 0x202,
        FrameBufferNHeight   = 0x203,
        FrameBufferNFormat   = 0x204,
        FrameBufferNBlockDim = 0x205,
        ViewportNScaleX      = 0x280,
        ViewportNScaleY      = 0x281,
        ViewportNScaleZ      = 0x282,
        ViewportNTranslateX  = 0x283,
        ViewportNTranslateY  = 0x284,
        ViewportNTranslateZ  = 0x285,
        ViewportNHoriz       = 0x300,
        ViewportNVert        = 0x301,
        DepthRangeNNear      = 0x302,
        DepthRangeNFar       = 0x303,
        VertexArrayFirst     = 0x35d,
        VertexArrayCount     = 0x35e,
        ClearNColor          = 0x360,
        ClearDepth           = 0x364,
        ClearStencil         = 0x368,
        StencilBackFuncRef   = 0x3d5,
        StencilBackMask      = 0x3d6,
        StencilBackFuncMask  = 0x3d7,
        ColorMaskCommon      = 0x3e4,
        RtSeparateFragData   = 0x3eb,
        ZetaAddress          = 0x3f8,
        ZetaFormat           = 0x3fa,
        ZetaBlockDimensions  = 0x3fb,
        ZetaLayerStride      = 0x3fc,
        VertexAttribNFormat  = 0x458,
        RtControl            = 0x487,
        ZetaHoriz            = 0x48a,
        ZetaVert             = 0x48b,
        ZetaArrayMode        = 0x48c,
        DepthTestEnable      = 0x4b3,
        IBlendEnable         = 0x4b9,
        DepthWriteEnable     = 0x4ba,
        DepthTestFunction    = 0x4c3,
        BlendSeparateAlpha   = 0x4cf,
        BlendEquationRgb     = 0x4d0,
        BlendFuncSrcRgb      = 0x4d1,
        BlendFuncDstRgb      = 0x4d2,
        BlendEquationAlpha   = 0x4d3,
        BlendFuncSrcAlpha    = 0x4d4,
        BlendFuncDstAlpha    = 0x4d6,
        BlendEnableMaster    = 0x4d7,
        BlendNEnable         = 0x4d8,
        StencilEnable        = 0x4e0,
        StencilFrontOpFail   = 0x4e1,
        StencilFrontOpZFail  = 0x4e2,
        StencilFrontOpZPass  = 0x4e3,
        StencilFrontFuncFunc = 0x4e4,
        StencilFrontFuncRef  = 0x4e5,
        StencilFrontFuncMask = 0x4e6,
        StencilFrontMask     = 0x4e7,
        VertexArrayElemBase  = 0x50d,
        VertexArrayInstBase  = 0x50e,
        ZetaEnable           = 0x54e,
        TexHeaderPoolOffset  = 0x55d,
        TexSamplerPoolOffset = 0x557,
        StencilTwoSideEnable = 0x565,
        StencilBackOpFail    = 0x566,
        StencilBackOpZFail   = 0x567,
        StencilBackOpZPass   = 0x568,
        StencilBackFuncFunc  = 0x569,
        FrameBufferSrgb      = 0x56e,
        ShaderAddress        = 0x582,
        VertexBeginGl        = 0x586,
        PrimRestartEnable    = 0x591,
        PrimRestartIndex     = 0x592,
        IndexArrayAddress    = 0x5f2,
        IndexArrayEndAddr    = 0x5f4,
        IndexArrayFormat     = 0x5f6,
        IndexBatchFirst      = 0x5f7,
        IndexBatchCount      = 0x5f8,
        VertexArrayNInstance = 0x620,
        CullFaceEnable       = 0x646,
        FrontFace            = 0x647,
        CullFace             = 0x648,
        ColorMaskN           = 0x680,
        QueryAddress         = 0x6c0,
        QuerySequence        = 0x6c2,
        QueryControl         = 0x6c3,
        VertexArrayNControl  = 0x700,
        VertexArrayNAddress  = 0x701,
        VertexArrayNDivisor  = 0x703,
        IBlendNSeparateAlpha = 0x780,
        IBlendNEquationRgb   = 0x781,
        IBlendNFuncSrcRgb    = 0x782,
        IBlendNFuncDstRgb    = 0x783,
        IBlendNEquationAlpha = 0x784,
        IBlendNFuncSrcAlpha  = 0x785,
        IBlendNFuncDstAlpha  = 0x786,
        VertexArrayNEndAddr  = 0x7c0,
        ShaderNControl       = 0x800,
        ShaderNOffset        = 0x801,
        ShaderNMaxGprs       = 0x803,
        ShaderNType          = 0x804,
        ConstBufferSize      = 0x8e0,
        ConstBufferAddress   = 0x8e1,
        ConstBufferOffset    = 0x8e3,
        TextureCbIndex       = 0x982
    }
}