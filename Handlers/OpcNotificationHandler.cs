using Opc.Ua.Client;

namespace OPCUA_Client.Handlers;

public static class OpcNotificationHandler
{
    public static void OnDataChanged(
        MonitoredItem item,
        MonitoredItemNotificationEventArgs e)
    {
        foreach (var value in item.DequeueValues())
        {
            Console.WriteLine(
                $"[{DateTime.Now:HH:mm:ss}] {item.DisplayName} = {value.Value}");
        }
    }
}
