using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OPCUA_Client.Handlers;

namespace OPCUA_Client.Services;

public class OpcClientService
{
    private ISession? _session;
    private Subscription? _subscription;

    public bool IsConnected => _session != null;

    public async Task Connect(
    ApplicationConfiguration config,
    string serverUrl)
    {
        Console.WriteLine(
            $"Connecting to: {serverUrl}");

       

        var telemetry =
    DefaultTelemetry.Create(logging => { });

        var selectedEndpoint =
            await CoreClientUtils.SelectEndpointAsync(
                config,
                serverUrl,
                false,
                CoreClientUtils.DefaultDiscoverTimeout,
                telemetry,
                CancellationToken.None);

        var endpointConfiguration =
            EndpointConfiguration.Create(config);

        var endpoint =
            new ConfiguredEndpoint(
                null,
                selectedEndpoint,
                endpointConfiguration);

        var sessionFactory =
            new DefaultSessionFactory(telemetry);

        _session =
            await sessionFactory.CreateAsync(
                config,
                endpoint,
                false,
                false,
                "SimpleOpcUaClient",
                60000,
                new UserIdentity(),
                null,
                CancellationToken.None);

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

            _subscription.CreateAsync();

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

        _subscription.ApplyChangesAsync();

        Console.WriteLine(
            $"Monitoring: {displayName}");
    }

    public void Disconnect()
    {
        if (_subscription != null)
        {
            _subscription.DeleteAsync(true);
        }

        _session?.CloseAsync();

        Console.WriteLine(
            "Disconnected.");
    }

}