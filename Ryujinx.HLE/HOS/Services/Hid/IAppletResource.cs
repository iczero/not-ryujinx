using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    class AppletResource : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _mCommands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _mCommands;

        private KSharedMemory _hidSharedMem;

        public AppletResource(KSharedMemory hidSharedMem)
        {
            _mCommands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetSharedMemoryHandle }
            };

            this._hidSharedMem = hidSharedMem;
        }

        public long GetSharedMemoryHandle(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_hidSharedMem, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }
    }
}