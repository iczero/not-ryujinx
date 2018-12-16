namespace Ryujinx.HLE.HOS
{
    enum ErrorModule
    {
        Kernel                = 1,
        Fs                    = 2,
        Os                    = 3, // (Memory, Thread, Mutex, NVIDIA)
        Htcs                  = 4,
        Ncm                   = 5,
        Dd                    = 6,
        DebugMonitor          = 7,
        Lr                    = 8,
        Loader                = 9,
        IpcCommandInterface   = 10,
        Ipc                   = 11,
        Pm                    = 15,
        Ns                    = 16,
        Socket                = 17,
        Htc                   = 18,
        NcmContent            = 20,
        Sm                    = 21,
        RoUserland            = 22,
        SdMmc                 = 24,
        Ovln                  = 25,
        Spl                   = 26,
        Ethc                  = 100,
        I2C                   = 101,
        Gpio                  = 102,
        Uart                  = 103,
        Settings              = 105,
        Wlan                  = 107,
        Xcd                   = 108,
        Nifm                  = 110,
        Hwopus                = 111,
        Bluetooth             = 113,
        Vi                    = 114,
        Nfp                   = 115,
        Time                  = 116,
        Fgm                   = 117,
        Oe                    = 118,
        Pcie                  = 120,
        Friends               = 121,
        Bcat                  = 122,
        Ssl                   = 123,
        Account               = 124,
        News                  = 125,
        Mii                   = 126,
        Nfc                   = 127,
        Am                    = 128,
        PlayReport            = 129,
        Ahid                  = 130,
        Qlaunch               = 132,
        Pcv                   = 133,
        Omm                   = 134,
        Bpc                   = 135,
        Psm                   = 136,
        Nim                   = 137,
        Psc                   = 138,
        Tc                    = 139,
        Usb                   = 140,
        Nsd                   = 141,
        Pctl                  = 142,
        Btm                   = 143,
        Ec                    = 144,
        ETicket               = 145,
        Ngc                   = 146,
        ErrorReport           = 147,
        Apm                   = 148,
        Profiler              = 150,
        ErrorUpload           = 151,
        Audio                 = 153,
        Npns                  = 154,
        NpnsHttpStream        = 155,
        Arp                   = 157,
        Swkbd                 = 158,
        Boot                  = 159,
        NfcMifare             = 161,
        UserlandAssert        = 162,
        Fatal                 = 163,
        NimShop               = 164,
        Spsm                  = 165,
        Bgtc                  = 167,
        UserlandCrash         = 168,
        SRepo                 = 180,
        Dauth                 = 181,
        Hid                   = 202,
        Ldn                   = 203,
        Irsensor              = 205,
        Capture               = 206,
        Manu                  = 208,
        Atk                   = 209,
        Web                   = 210,
        Grc                   = 212,
        Migration             = 216,
        MigrationLdcServer    = 217,
        GeneralWebApplet      = 800,
        WifiWebAuthApplet     = 809,
        WhitelistedApplet     = 810,
        ShopN                 = 811
    }
}
