using Microsoft.Win32;

namespace Radio.Web;

public static class ConfigurationRegistry
{
    public static RegistryKey HKLM =>
        Registry.LocalMachine.CreateSubKey(@"SOFTWARE\RadioBroadcastWorkflow");
}
