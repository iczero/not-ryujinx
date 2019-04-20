using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Acc;
using Ryujinx.HLE.HOS.Services.Am;
using Ryujinx.HLE.HOS.Services.Apm;
using Ryujinx.HLE.HOS.Services.Aud;
using Ryujinx.HLE.HOS.Services.Bsd;
using Ryujinx.HLE.HOS.Services.Caps;
using Ryujinx.HLE.HOS.Services.Es;
using Ryujinx.HLE.HOS.Services.FspSrv;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Services.Irs;
using Ryujinx.HLE.HOS.Services.Ldr;
using Ryujinx.HLE.HOS.Services.Lm;
using Ryujinx.HLE.HOS.Services.Mm;
using Ryujinx.HLE.HOS.Services.Ncm;
using Ryujinx.HLE.HOS.Services.Nfp;
using Ryujinx.HLE.HOS.Services.Ns;
using Ryujinx.HLE.HOS.Services.Nv;
using Ryujinx.HLE.HOS.Services.Pctl;
using Ryujinx.HLE.HOS.Services.Pl;
using Ryujinx.HLE.HOS.Services.Pm;
using Ryujinx.HLE.HOS.Services.Prepo;
using Ryujinx.HLE.HOS.Services.Psm;
using Ryujinx.HLE.HOS.Services.Set;
using Ryujinx.HLE.HOS.Services.Sfdnsres;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.HLE.HOS.Services.Spl;
using Ryujinx.HLE.HOS.Services.Ssl;
using Ryujinx.HLE.HOS.Services.Vi;
using System;

namespace Ryujinx.HLE.HOS.Services
{
    public static class ServiceConfiguration
    {
        public static bool IgnoreMissingServices { get; set; }
    }

    static class ServiceFactory
    {
        public static IpcService MakeService(Horizon system, string name)
        {
            switch (name)
            {
                case "acc:u0":
                case "acc:u1":
                    return new IAccountService();

                case "aoc:u":
                    return new IAddOnContentManager();

                case "apm":
                case "apm:p":
                    return new IManager();

                case "appletAE":
                    return new IAllSystemAppletProxiesService();

                case "appletOE":
                    return new IApplicationProxyService();

                case "audout:u":
                    return new IAudioOutManager();

                case "audren:u":
                    return new IAudioRendererManager();

                case "bcat:a":
                case "bcat:m":
                case "bcat:u":
                case "bcat:s":
                    return new Bcat.IServiceCreator();

                case "bsd:s":
                    return new IClient(true);

                case "bsd:u":
                    return new IClient(false);

                case "caps:a":
                    return new IAlbumAccessorService();

                case "caps:ss":
                    return new IScreenshotService();

                case "csrng":
                    return new IRandomInterface();

                case "es":
                    return new IeTicketService();

                case "friend:a":
                case "friend:u":
                    return new Friend.IServiceCreator();

                case "fsp-srv":
                    return new IFileSystemProxy();

                case "hid":
                    return new IHidServer(system);

                case "irs":
                    return new IIrSensorServer();

                case "ldr:ro":
                    return new IRoInterface();

                case "hwopus":
                    return new IHardwareOpusDecoderManager();

                case "lm":
                    return new ILogService();

                case "mm:u":
                    return new IRequest();

                case "ncm":
                    return new IContentManager();

                case "nfp:user":
                    return new IUserManager();

                case "nifm:u":
                    return new Nifm.IStaticService();

                case "ns:am":
                    return new IApplicationManagerInterface();

                case "ns:am2":
                case "ns:ec":
                    return new IServiceGetterInterface();

                case "ns:su":
                    return new ISystemUpdateInterface();

                case "ns:vm":
                    return new IVulnerabilityManagerInterface();

                case "nvdrv":
                case "nvdrv:a":
                    return new INvDrvServices(system);

                case "pctl:s":
                case "pctl:r":
                case "pctl:a":
                case "pctl":
                    return new IParentalControlServiceFactory();

                case "pl:u":
                    return new ISharedFontManager();

                case "pm:shell":
                    return new IShellInterface();

                case "prepo:a":
                case "prepo:u":
                    return new IPrepoService();

                case "psm":
                    return new IPsmServer();

                case "set":
                    return new ISettingsServer();

                case "set:sys":
                    return new ISystemSettingsServer();

                case "sfdnsres":
                    return new IResolver();

                case "sm:":
                    return new IUserInterface();

                case "ssl":
                    return new ISslService();

                case "time:a":
                case "time:s":
                case "time:u":
                    return new Time.IStaticService(system.TimeSharedMem);

                case "vi:m":
                    return new IManagerRootService();

                case "vi:s":
                    return new ISystemRootService();

                case "vi:u":
                    return new IApplicationRootService();
            }

            if (ServiceConfiguration.IgnoreMissingServices)
            {
                Logger.PrintWarning(LogClass.Service, $"Missing service {name} ignored");
                return new DummyService(name);
            }

            throw new NotImplementedException(name);
        }
    }
}
