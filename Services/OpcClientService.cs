using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OPCUA_Client.Handlers;

namespace OPCUA_Client.Services;

public class OpcClientService
{
    private Session? _session;
    private Subscription? _subscription;

    public bool IsConnected => _session != null;

    public async Task Connect(
        ApplicationConfiguration config,
        string serverUrl)
    {
        Console.WriteLine(
            $"Connecting to: {serverUrl}");

        var selectedEndpoint =
            CoreClientUtils.SelectEndpoint(
                config,
                serverUrl,
                false);

        var endpointConfiguration =
            EndpointConfiguration.Create(config);

        var endpoint =
            new ConfiguredEndpoint(
                null,
                selectedEndpoint,
                endpointConfiguration);

        _session = await Session.Create(
            config,
            endpoint,
            false,
            "SimpleOpcUaClient",
            60000,
            null,
            null);

        Console.WriteLine(
            "Connected Successfully!");
    }

    public void Subscribe(
        string nodeId,
        string displayName)
    {
        if (_session == null)
            throw new Exception(
                "Session not established.");

        if (_subscription == null)
        {
            _subscription =
                new Subscription(
                    _session.DefaultSubscription)
                {
                    PublishingInterval = 1000
                };

            _session.AddSubscription(_subscription);

            _subscription.Create();

            Console.WriteLine(
                "Subscription Created");
        }

        var monitoredItem =
            new MonitoredItem(
                _subscription.DefaultItem)
            {
                DisplayName = displayName,
                StartNodeId = NodeId.Parse(nodeId),
                AttributeId = Attributes.Value,
                SamplingInterval = 1000
            };

        monitoredItem.Notification += OpcNotificationHandler.OnDataChanged;

        _subscription.AddItem(monitoredItem);

        _subscription.ApplyChanges();

        Console.WriteLine(
            $"Monitoring: {displayName}");
    }

    public void Disconnect()
    {
        if (_subscription != null)
        {
            _subscription.Delete(true);
        }

        _session?.Close();

        Console.WriteLine(
            "Disconnected.");
    }

}