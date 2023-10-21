﻿using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Shader;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Buffer manager.
    /// </summary>
    class BufferManager
    {
        private readonly GpuContext _context;
        private readonly GpuChannel _channel;

        private int _unalignedStorageBuffers;
        public bool HasUnalignedStorageBuffers => _unalignedStorageBuffers > 0;

        public bool HasTransformFeedbackOutputs { get; set; }

        private IndexBuffer _indexBuffer;
        private readonly VertexBuffer[] _vertexBuffers;
        private readonly BufferBounds[] _transformFeedbackBuffers;
        private readonly List<BufferTextureBinding> _bufferTextures;
        private readonly BufferAssignment[] _ranges;

        /// <summary>
        /// Holds shader stage buffer state and binding information.
        /// </summary>
        private class BuffersPerStage
        {
            /// <summary>
            /// Shader buffer binding information.
            /// </summary>
            public BufferDescriptor[] Bindings { get; private set; }

            /// <summary>
            /// Buffer regions.
            /// </summary>
            public BufferBounds[] Buffers { get; }

            /// <summary>
            /// Flag indicating if this binding is unaligned.
            /// </summary>
            public bool[] Unaligned { get; }

            /// <summary>
            /// Total amount of buffers used on the shader.
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// Creates a new instance of the shader stage buffer information.
            /// </summary>
            /// <param name="count">Maximum amount of buffers that the shader stage can use</param>
            public BuffersPerStage(int count)
            {
                Bindings = new BufferDescriptor[count];
                Buffers = new BufferBounds[count];
                Unaligned = new bool[count];
            }

            /// <summary>
            /// Sets the region of a buffer at a given slot.
            /// </summary>
            /// <param name="index">Buffer slot</param>
            /// <param name="physical">Physical memory backing the buffer</param>
            /// <param name="address">Region virtual address</param>
            /// <param name="size">Region size in bytes</param>
            /// <param name="flags">Buffer usage flags</param>
            public void SetBounds(int index, PhysicalMemory physical, ulong address, ulong size, BufferUsageFlags flags = BufferUsageFlags.None)
            {
                Buffers[index] = new BufferBounds(physical, address, size, flags);
            }

            /// <summary>
            /// Sets shader buffer binding information.
            /// </summary>
            /// <param name="descriptors">Buffer binding information</param>
            public void SetBindings(BufferDescriptor[] descriptors)
            {
                if (descriptors == null)
                {
                    Count = 0;
                    return;
                }

                if ((Count = descriptors.Length) != 0)
                {
                    Bindings = descriptors;
                }
            }
        }

        private readonly BuffersPerStage _cpStorageBuffers;
        private readonly BuffersPerStage _cpUniformBuffers;
        private readonly BuffersPerStage[] _gpStorageBuffers;
        private readonly BuffersPerStage[] _gpUniformBuffers;

        private bool _gpStorageBuffersDirty;
        private bool _gpUniformBuffersDirty;

        private bool _indexBufferDirty;
        private bool _vertexBuffersDirty;
        private uint _vertexBuffersEnableMask;
        private bool _transformFeedbackBuffersDirty;

        private bool _rebind;

        /// <summary>
        /// Creates a new instance of the buffer manager.
        /// </summary>
        /// <param name="context">GPU context that the buffer manager belongs to</param>
        /// <param name="channel">GPU channel that the buffer manager belongs to</param>
        public BufferManager(GpuContext context, GpuChannel channel)
        {
            _context = context;
            _channel = channel;

            _vertexBuffers = new VertexBuffer[Constants.TotalVertexBuffers];

            _transformFeedbackBuffers = new BufferBounds[Constants.TotalTransformFeedbackBuffers];

            _cpStorageBuffers = new BuffersPerStage(Constants.TotalCpStorageBuffers);
            _cpUniformBuffers = new BuffersPerStage(Constants.TotalCpUniformBuffers);

            _gpStorageBuffers = new BuffersPerStage[Constants.ShaderStages];
            _gpUniformBuffers = new BuffersPerStage[Constants.ShaderStages];

            for (int index = 0; index < Constants.ShaderStages; index++)
            {
                _gpStorageBuffers[index] = new BuffersPerStage(Constants.TotalGpStorageBuffers);
                _gpUniformBuffers[index] = new BuffersPerStage(Constants.TotalGpUniformBuffers);
            }

            _bufferTextures = new List<BufferTextureBinding>();

            _ranges = new BufferAssignment[Constants.TotalGpUniformBuffers * Constants.ShaderStages];
        }


        /// <summary>
        /// Sets the memory range with the index buffer data, to be used for subsequent draw calls.
        /// </summary>
        /// <param name="gpuVa">Start GPU virtual address of the index buffer</param>
        /// <param name="size">Size, in bytes, of the index buffer</param>
        /// <param name="type">Type of each index buffer element</param>
        public void SetIndexBuffer(ulong gpuVa, ulong size, IndexType type)
        {
            var bufferCache = _channel.MemoryManager.GetBackingMemory(gpuVa).BufferCache;
            ulong address = bufferCache.TranslateAndCreateBuffer(_channel.MemoryManager, gpuVa, size);

            _indexBuffer.BufferCache = bufferCache;
            _indexBuffer.Address = address;
            _indexBuffer.Size = size;
            _indexBuffer.Type = type;

            _indexBufferDirty = true;
        }

        /// <summary>
        /// Sets a new index buffer that overrides the one set on the call to <see cref="CommitGraphicsBindings"/>.
        /// </summary>
        /// <param name="buffer">Buffer to be used as index buffer</param>
        /// <param name="type">Type of each index buffer element</param>
        public void SetIndexBuffer(BufferRange buffer, IndexType type)
        {
            _context.Renderer.Pipeline.SetIndexBuffer(buffer, type);

            _indexBufferDirty = true;
        }

        /// <summary>
        /// Sets the memory range with vertex buffer data, to be used for subsequent draw calls.
        /// </summary>
        /// <param name="index">Index of the vertex buffer (up to 16)</param>
        /// <param name="gpuVa">GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the buffer</param>
        /// <param name="stride">Stride of the buffer, defined as the number of bytes of each vertex</param>
        /// <param name="divisor">Vertex divisor of the buffer, for instanced draws</param>
        public void SetVertexBuffer(int index, ulong gpuVa, ulong size, int stride, int divisor)
        {
            var bufferCache = _channel.MemoryManager.GetBackingMemory(gpuVa).BufferCache;
            ulong address = bufferCache.TranslateAndCreateBuffer(_channel.MemoryManager, gpuVa, size);

            ref VertexBuffer vb = ref _vertexBuffers[index];

            vb.BufferCache = bufferCache;
            vb.Address = address;
            vb.Size = size;
            vb.Stride = stride;
            vb.Divisor = divisor;

            _vertexBuffersDirty = true;

            if (address != 0)
            {
                _vertexBuffersEnableMask |= 1u << index;
            }
            else
            {
                _vertexBuffersEnableMask &= ~(1u << index);
            }
        }

        /// <summary>
        /// Sets a transform feedback buffer on the graphics pipeline.
        /// The output from the vertex transformation stages are written into the feedback buffer.
        /// </summary>
        /// <param name="index">Index of the transform feedback buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the transform feedback buffer</param>
        public void SetTransformFeedbackBuffer(int index, ulong gpuVa, ulong size)
        {
            var physical = _channel.MemoryManager.GetBackingMemory(gpuVa);
            ulong address = physical.BufferCache.TranslateAndCreateBuffer(_channel.MemoryManager, gpuVa, size);

            _transformFeedbackBuffers[index] = new BufferBounds(physical, address, size);
            _transformFeedbackBuffersDirty = true;
        }

        /// <summary>
        /// Records the alignment of a storage buffer.
        /// Unaligned storage buffers disable some optimizations on the shader.
        /// </summary>
        /// <param name="buffers">The binding list to modify</param>
        /// <param name="index">Index of the storage buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        private void RecordStorageAlignment(BuffersPerStage buffers, int index, ulong gpuVa)
        {
            bool unaligned = (gpuVa & ((ulong)_context.Capabilities.StorageBufferOffsetAlignment - 1)) != 0;

            if (unaligned || HasUnalignedStorageBuffers)
            {
                // Check if the alignment changed for this binding.

                ref bool currentUnaligned = ref buffers.Unaligned[index];

                if (currentUnaligned != unaligned)
                {
                    currentUnaligned = unaligned;
                    _unalignedStorageBuffers += unaligned ? 1 : -1;
                }
            }
        }

        /// <summary>
        /// Sets a storage buffer on the compute pipeline.
        /// Storage buffers can be read and written to on shaders.
        /// </summary>
        /// <param name="index">Index of the storage buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        /// <param name="flags">Buffer usage flags</param>
        public void SetComputeStorageBuffer(int index, ulong gpuVa, ulong size, BufferUsageFlags flags)
        {
            size += gpuVa & ((ulong)_context.Capabilities.StorageBufferOffsetAlignment - 1);

            RecordStorageAlignment(_cpStorageBuffers, index, gpuVa);

            gpuVa = BitUtils.AlignDown<ulong>(gpuVa, (ulong)_context.Capabilities.StorageBufferOffsetAlignment);

            var physical = _channel.MemoryManager.GetBackingMemory(gpuVa);
            ulong address = physical.BufferCache.TranslateAndCreateBuffer(_channel.MemoryManager, gpuVa, size);

            _cpStorageBuffers.SetBounds(index, physical, address, size, flags);
        }

        /// <summary>
        /// Sets a storage buffer on the graphics pipeline.
        /// Storage buffers can be read and written to on shaders.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="index">Index of the storage buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        /// <param name="flags">Buffer usage flags</param>
        public void SetGraphicsStorageBuffer(int stage, int index, ulong gpuVa, ulong size, BufferUsageFlags flags)
        {
            size += gpuVa & ((ulong)_context.Capabilities.StorageBufferOffsetAlignment - 1);

            BuffersPerStage buffers = _gpStorageBuffers[stage];

            RecordStorageAlignment(buffers, index, gpuVa);

            gpuVa = BitUtils.AlignDown<ulong>(gpuVa, (ulong)_context.Capabilities.StorageBufferOffsetAlignment);

            var physical = _channel.MemoryManager.GetBackingMemory(gpuVa);
            ulong address = physical.BufferCache.TranslateAndCreateBuffer(_channel.MemoryManager, gpuVa, size);

            if (buffers.Buffers[index].Address != address ||
                buffers.Buffers[index].Size != size)
            {
                _gpStorageBuffersDirty = true;
            }

            buffers.SetBounds(index, physical, address, size, flags);
        }

        /// <summary>
        /// Sets a uniform buffer on the compute pipeline.
        /// Uniform buffers are read-only from shaders, and have a small capacity.
        /// </summary>
        /// <param name="index">Index of the uniform buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        public void SetComputeUniformBuffer(int index, ulong gpuVa, ulong size)
        {
            var physical = _channel.MemoryManager.GetBackingMemory(gpuVa);
            ulong address = physical.BufferCache.TranslateAndCreateBuffer(_channel.MemoryManager, gpuVa, size);

            _cpUniformBuffers.SetBounds(index, physical, address, size);
        }

        /// <summary>
        /// Sets a uniform buffer on the graphics pipeline.
        /// Uniform buffers are read-only from shaders, and have a small capacity.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="index">Index of the uniform buffer</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the storage buffer</param>
        public void SetGraphicsUniformBuffer(int stage, int index, ulong gpuVa, ulong size)
        {
            var physical = _channel.MemoryManager.GetBackingMemory(gpuVa);
            ulong address = physical.BufferCache.TranslateAndCreateBuffer(_channel.MemoryManager, gpuVa, size);

            _gpUniformBuffers[stage].SetBounds(index, physical, address, size);
            _gpUniformBuffersDirty = true;
        }

        /// <summary>
        /// Sets the number of vertices per instance on a instanced draw. Used for transform feedback emulation.
        /// </summary>
        /// <param name="vertexCount">Vertex count per instance</param>
        public void SetInstancedDrawVertexCount(int vertexCount)
        {
            if (!_context.Capabilities.SupportsTransformFeedback && HasTransformFeedbackOutputs)
            {
                _context.SupportBufferUpdater.SetTfeVertexCount(vertexCount);
                _context.SupportBufferUpdater.Commit();
            }
        }

        /// <summary>
        /// Forces transform feedback and storage buffers to be updated on the next draw.
        /// </summary>
        public void ForceTransformFeedbackAndStorageBuffersDirty()
        {
            _transformFeedbackBuffersDirty = true;
            _gpStorageBuffersDirty = true;
        }

        /// <summary>
        /// Sets the binding points for the storage buffers bound on the compute pipeline.
        /// </summary>
        /// <param name="bindings">Bindings for the active shader</param>
        public void SetComputeBufferBindings(CachedShaderBindings bindings)
        {
            _cpStorageBuffers.SetBindings(bindings.StorageBufferBindings[0]);
            _cpUniformBuffers.SetBindings(bindings.ConstantBufferBindings[0]);
        }

        /// <summary>
        /// Sets the binding points for the storage buffers bound on the graphics pipeline.
        /// </summary>
        /// <param name="bindings">Bindings for the active shader</param>
        public void SetGraphicsBufferBindings(CachedShaderBindings bindings)
        {
            for (int i = 0; i < Constants.ShaderStages; i++)
            {
                _gpStorageBuffers[i].SetBindings(bindings.StorageBufferBindings[i]);
                _gpUniformBuffers[i].SetBindings(bindings.ConstantBufferBindings[i]);
            }

            _gpStorageBuffersDirty = true;
            _gpUniformBuffersDirty = true;
        }

        /// <summary>
        /// Gets a bit mask indicating which compute uniform buffers are currently bound.
        /// </summary>
        /// <returns>Mask where each bit set indicates a bound constant buffer</returns>
        public uint GetComputeUniformBufferUseMask()
        {
            uint mask = 0;

            for (int i = 0; i < _cpUniformBuffers.Buffers.Length; i++)
            {
                if (_cpUniformBuffers.Buffers[i].Address != 0)
                {
                    mask |= 1u << i;
                }
            }

            return mask;
        }

        /// <summary>
        /// Gets a bit mask indicating which graphics uniform buffers are currently bound.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <returns>Mask where each bit set indicates a bound constant buffer</returns>
        public uint GetGraphicsUniformBufferUseMask(int stage)
        {
            uint mask = 0;

            for (int i = 0; i < _gpUniformBuffers[stage].Buffers.Length; i++)
            {
                if (_gpUniformBuffers[stage].Buffers[i].Address != 0)
                {
                    mask |= 1u << i;
                }
            }

            return mask;
        }

        /// <summary>
        /// Gets the address of the compute uniform buffer currently bound at the given index.
        /// </summary>
        /// <param name="index">Index of the uniform buffer binding</param>
        /// <returns>The uniform buffer address, or an undefined value if the buffer is not currently bound</returns>
        public (PhysicalMemory, ulong) GetComputeUniformBufferAddress(int index)
        {
            var buffer = _cpUniformBuffers.Buffers[index];
            return (buffer.Physical, buffer.Address);
        }

        /// <summary>
        /// Gets the address of the graphics uniform buffer currently bound at the given index.
        /// </summary>
        /// <param name="stage">Index of the shader stage</param>
        /// <param name="index">Index of the uniform buffer binding</param>
        /// <returns>The uniform buffer address, or an undefined value if the buffer is not currently bound</returns>
        public (PhysicalMemory, ulong) GetGraphicsUniformBufferAddress(int stage, int index)
        {
            var buffer = _gpUniformBuffers[stage].Buffers[index];
            return (buffer.Physical, buffer.Address);
        }

        /// <summary>
        /// Gets the bounds of the uniform buffer currently bound at the given index.
        /// </summary>
        /// <param name="isCompute">Indicates whenever the uniform is requested by the 3D or compute engine</param>
        /// <param name="stage">Index of the shader stage, if the uniform is for the 3D engine</param>
        /// <param name="index">Index of the uniform buffer binding</param>
        /// <returns>The uniform buffer bounds, or an undefined value if the buffer is not currently bound</returns>
        public ref BufferBounds GetUniformBufferBounds(bool isCompute, int stage, int index)
        {
            if (isCompute)
            {
                return ref _cpUniformBuffers.Buffers[index];
            }
            else
            {
                return ref _gpUniformBuffers[stage].Buffers[index];
            }
        }

        /// <summary>
        /// Ensures that the compute engine bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
        public void CommitComputeBindings()
        {
            BindBuffers(_cpStorageBuffers, isStorage: true);
            BindBuffers(_cpUniformBuffers, isStorage: false);

            CommitBufferTextureBindings();

            // Force rebind after doing compute work.
            Rebind();

            _context.SupportBufferUpdater.Commit();
        }

        /// <summary>
        /// Commit any queued buffer texture bindings.
        /// </summary>
        private void CommitBufferTextureBindings()
        {
            if (_bufferTextures.Count > 0)
            {
                foreach (var binding in _bufferTextures)
                {
                    var isStore = binding.BindingInfo.Flags.HasFlag(TextureUsageFlags.ImageStore);
                    var range = binding.BufferCache.GetBufferRange(binding.Address, binding.Size, isStore);
                    binding.HostTexture.SetStorage(range);

                    // The texture must be rebound to use the new storage if it was updated.

                    if (binding.IsImage)
                    {
                        _context.Renderer.Pipeline.SetImage(binding.BindingInfo.Binding, binding.HostTexture, binding.Format);
                    }
                    else
                    {
                        _context.Renderer.Pipeline.SetTextureAndSampler(binding.Stage, binding.BindingInfo.Binding, binding.HostTexture, null);
                    }
                }

                _bufferTextures.Clear();
            }
        }

        /// <summary>
        /// Ensures that the graphics engine bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
        /// <param name="indexed">True if the index buffer is in use</param>
        public void CommitGraphicsBindings(bool indexed)
        {
            if (indexed)
            {
                if (_indexBufferDirty || _rebind)
                {
                    _indexBufferDirty = false;

                    if (_indexBuffer.Address != 0)
                    {
                        BufferRange buffer = _indexBuffer.BufferCache.GetBufferRange(_indexBuffer.Address, _indexBuffer.Size);

                        _context.Renderer.Pipeline.SetIndexBuffer(buffer, _indexBuffer.Type);
                    }
                }
                else if (_indexBuffer.Address != 0)
                {
                    _indexBuffer.BufferCache.SynchronizeBufferRange(_indexBuffer.Address, _indexBuffer.Size);
                }
            }
            else if (_rebind)
            {
                _indexBufferDirty = true;
            }

            uint vbEnableMask = _vertexBuffersEnableMask;

            if (_vertexBuffersDirty || _rebind)
            {
                _vertexBuffersDirty = false;

                Span<VertexBufferDescriptor> vertexBuffers = stackalloc VertexBufferDescriptor[Constants.TotalVertexBuffers];

                for (int index = 0; (vbEnableMask >> index) != 0; index++)
                {
                    VertexBuffer vb = _vertexBuffers[index];

                    if (vb.Address == 0)
                    {
                        continue;
                    }

                    BufferRange buffer = vb.BufferCache.GetBufferRange(vb.Address, vb.Size);

                    vertexBuffers[index] = new VertexBufferDescriptor(buffer, vb.Stride, vb.Divisor);
                }

                _context.Renderer.Pipeline.SetVertexBuffers(vertexBuffers);
            }
            else
            {
                for (int index = 0; (vbEnableMask >> index) != 0; index++)
                {
                    VertexBuffer vb = _vertexBuffers[index];

                    if (vb.Address == 0)
                    {
                        continue;
                    }

                    vb.BufferCache.SynchronizeBufferRange(vb.Address, vb.Size);
                }
            }

            if (_transformFeedbackBuffersDirty || _rebind)
            {
                _transformFeedbackBuffersDirty = false;

                if (_context.Capabilities.SupportsTransformFeedback)
                {
                    Span<BufferRange> tfbs = stackalloc BufferRange[Constants.TotalTransformFeedbackBuffers];

                    for (int index = 0; index < Constants.TotalTransformFeedbackBuffers; index++)
                    {
                        BufferBounds tfb = _transformFeedbackBuffers[index];

                        if (tfb.Address == 0)
                        {
                            tfbs[index] = BufferRange.Empty;
                            continue;
                        }

                        tfbs[index] = tfb.BufferCache.GetBufferRange(tfb.Address, tfb.Size, write: true);
                    }

                    _context.Renderer.Pipeline.SetTransformFeedbackBuffers(tfbs);
                }
                else if (HasTransformFeedbackOutputs)
                {
                    Span<BufferAssignment> buffers = stackalloc BufferAssignment[Constants.TotalTransformFeedbackBuffers];

                    int alignment = _context.Capabilities.StorageBufferOffsetAlignment;

                    for (int index = 0; index < Constants.TotalTransformFeedbackBuffers; index++)
                    {
                        BufferBounds tfb = _transformFeedbackBuffers[index];

                        if (tfb.Address == 0)
                        {
                            buffers[index] = new BufferAssignment(index, BufferRange.Empty);
                        }
                        else
                        {
                            ulong endAddress = tfb.Address + tfb.Size;
                            ulong address = BitUtils.AlignDown(tfb.Address, (ulong)alignment);
                            ulong size = endAddress - address;

                            int tfeOffset = ((int)tfb.Address & (alignment - 1)) / 4;

                            _context.SupportBufferUpdater.SetTfeOffset(index, tfeOffset);

                            buffers[index] = new BufferAssignment(index, tfb.BufferCache.GetBufferRange(address, size, write: true));
                        }
                    }

                    _context.Renderer.Pipeline.SetStorageBuffers(buffers);
                }
            }
            else
            {
                for (int index = 0; index < Constants.TotalTransformFeedbackBuffers; index++)
                {
                    BufferBounds tfb = _transformFeedbackBuffers[index];

                    if (tfb.Address == 0)
                    {
                        continue;
                    }

                    tfb.BufferCache.SynchronizeBufferRange(tfb.Address, tfb.Size);
                }
            }

            if (_gpStorageBuffersDirty || _rebind)
            {
                _gpStorageBuffersDirty = false;

                BindBuffers(_gpStorageBuffers, isStorage: true);
            }
            else
            {
                UpdateBuffers(_gpStorageBuffers);
            }

            if (_gpUniformBuffersDirty || _rebind)
            {
                _gpUniformBuffersDirty = false;

                BindBuffers(_gpUniformBuffers, isStorage: false);
            }
            else
            {
                UpdateBuffers(_gpUniformBuffers);
            }

            CommitBufferTextureBindings();

            _rebind = false;

            _context.SupportBufferUpdater.Commit();
        }

        /// <summary>
        /// Bind respective buffer bindings on the host API.
        /// </summary>
        /// <param name="bindings">Buffer memory ranges to bind</param>
        /// <param name="isStorage">True to bind as storage buffer, false to bind as uniform buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindBuffers(BuffersPerStage[] bindings, bool isStorage)
        {
            int rangesCount = 0;

            Span<BufferAssignment> ranges = _ranges;

            for (ShaderStage stage = ShaderStage.Vertex; stage <= ShaderStage.Fragment; stage++)
            {
                ref var buffers = ref bindings[(int)stage - 1];

                for (int index = 0; index < buffers.Count; index++)
                {
                    ref var bindingInfo = ref buffers.Bindings[index];

                    BufferBounds bounds = buffers.Buffers[bindingInfo.Slot];

                    if (bounds.Address != 0)
                    {
                        var isWrite = bounds.Flags.HasFlag(BufferUsageFlags.Write);
                        var range = isStorage
                            ? bounds.BufferCache.GetBufferRangeAligned(bounds.Address, bounds.Size, isWrite)
                            : bounds.BufferCache.GetBufferRange(bounds.Address, bounds.Size);

                        ranges[rangesCount++] = new BufferAssignment(bindingInfo.Binding, range);
                    }
                }
            }

            if (rangesCount != 0)
            {
                SetHostBuffers(ranges, rangesCount, isStorage);
            }
        }

        /// <summary>
        /// Bind respective buffer bindings on the host API.
        /// </summary>
        /// <param name="buffers">Buffer memory ranges to bind</param>
        /// <param name="isStorage">True to bind as storage buffer, false to bind as uniform buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindBuffers(BuffersPerStage buffers, bool isStorage)
        {
            int rangesCount = 0;

            Span<BufferAssignment> ranges = _ranges;

            for (int index = 0; index < buffers.Count; index++)
            {
                ref var bindingInfo = ref buffers.Bindings[index];

                BufferBounds bounds = buffers.Buffers[bindingInfo.Slot];

                if (bounds.Address != 0)
                {
                    var isWrite = bounds.Flags.HasFlag(BufferUsageFlags.Write);
                    var range = isStorage
                        ? bounds.BufferCache.GetBufferRangeAligned(bounds.Address, bounds.Size, isWrite)
                        : bounds.BufferCache.GetBufferRange(bounds.Address, bounds.Size);

                    ranges[rangesCount++] = new BufferAssignment(bindingInfo.Binding, range);
                }
            }

            if (rangesCount != 0)
            {
                SetHostBuffers(ranges, rangesCount, isStorage);
            }
        }

        /// <summary>
        /// Bind respective buffer bindings on the host API.
        /// </summary>
        /// <param name="ranges">Host buffers to bind, with their offsets and sizes</param>
        /// <param name="first">First binding point</param>
        /// <param name="count">Number of bindings</param>
        /// <param name="isStorage">Indicates if the buffers are storage or uniform buffers</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetHostBuffers(ReadOnlySpan<BufferAssignment> ranges, int count, bool isStorage)
        {
            if (isStorage)
            {
                _context.Renderer.Pipeline.SetStorageBuffers(ranges[..count]);
            }
            else
            {
                _context.Renderer.Pipeline.SetUniformBuffers(ranges[..count]);
            }
        }

        /// <summary>
        /// Updates data for the already bound buffer bindings.
        /// </summary>
        /// <param name="bindings">Bindings to update</param>
        private void UpdateBuffers(BuffersPerStage[] bindings)
        {
            for (ShaderStage stage = ShaderStage.Vertex; stage <= ShaderStage.Fragment; stage++)
            {
                ref var buffers = ref bindings[(int)stage - 1];

                for (int index = 0; index < buffers.Count; index++)
                {
                    ref var binding = ref buffers.Bindings[index];

                    BufferBounds bounds = buffers.Buffers[binding.Slot];

                    if (bounds.Address == 0)
                    {
                        continue;
                    }

                    bounds.BufferCache.SynchronizeBufferRange(bounds.Address, bounds.Size);
                }
            }
        }

        /// <summary>
        /// Sets the buffer storage of a buffer texture. This will be bound when the buffer manager commits bindings.
        /// </summary>
        /// <param name="stage">Shader stage accessing the texture</param>
        /// <param name="texture">Buffer texture</param>
        /// <param name="hostTexture">Buffer host texture</param>
        /// <param name="bindingInfo">Binding info for the buffer texture</param>
        /// <param name="format">Format of the buffer texture</param>
        /// <param name="isImage">Whether the binding is for an image or a sampler</param>
        public void SetBufferTextureStorage(
            ShaderStage stage,
            Image.Texture texture,
            ITexture hostTexture,
            TextureBindingInfo bindingInfo,
            Format format,
            bool isImage)
        {
            var binding = new BufferTextureBinding(stage, texture, hostTexture, bindingInfo, format, isImage);

            texture.PhysicalMemory.BufferCache.CreateBuffer(binding.Address, binding.Size);

            _bufferTextures.Add(binding);
        }

        /// <summary>
        /// Force all bound textures and images to be rebound the next time CommitBindings is called.
        /// </summary>
        public void Rebind()
        {
            _rebind = true;
        }
    }
}
