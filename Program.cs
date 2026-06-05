using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var config = new ApplicationConfiguration()
            {
                ApplicationName = "SimpleOpcUaClient",
                ApplicationType = ApplicationType.Client,

                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = "pki/own",
                        SubjectName = "CN=SimpleOpcUaClient"
                    },

                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "pki/trusted"
                    },

                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "pki/issuer"
                    },

                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "pki/rejected"
                    },

                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 1024
                },

                TransportConfigurations = new TransportConfigurationCollection(),

                TransportQuotas = new TransportQuotas
                {
                    OperationTimeout = 15000
                },

                ClientConfiguration = new ClientConfiguration
                {
                    DefaultSessionTimeout = 60000
                }
            };

            await config.ValidateAsync(ApplicationType.Client);

            config.CertificateValidator.CertificateValidation += (s, e) =>
            {
                Console.WriteLine($"Accepting Certificate: {e.Certificate.Subject}");
                e.Accept = true;
            };

            string serverUrl = "opc.tcp://192.168.87.102:52220";

            Console.WriteLine($"Connecting to: {serverUrl}");

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

            var session = await Session.Create(
                config,
                endpoint,
                false,
                "SimpleOpcUaClient",
                60000,
                null,
                null);

            Console.WriteLine("Connected Successfully!");

            //-------------------------------------------------
            // CREATE SUBSCRIPTION
            //-------------------------------------------------

            var subscription = new Subscription(session.DefaultSubscription)
            {
                PublishingInterval = 1000
            };

            //-------------------------------------------------
            // TESTDADDRESS
            //-------------------------------------------------

            var testDAddress = new MonitoredItem(subscription.DefaultItem)
            {
                DisplayName = "TestDAddress",
                StartNodeId =
                    new NodeId(
                        "DataSource::PLC_R04_CPU.TestDAddress",
                        2),

                AttributeId = Attributes.Value,
                SamplingInterval = 1000
            };

            testDAddress.Notification += TestDAddressChanged;

            //-------------------------------------------------
            // TESTMADDRESS
            //-------------------------------------------------

            var testMAddress = new MonitoredItem(subscription.DefaultItem)
            {
                DisplayName = "TestMAddress",
                StartNodeId =
                    new NodeId(
                        "DataSource::PLC_R04_CPU.TestMAddress",
                        2),

                AttributeId = Attributes.Value,
                SamplingInterval = 1000
            };

            testMAddress.Notification += TestMAddressChanged;

            //-------------------------------------------------
            // ADD ITEMS TO SUBSCRIPTION
            //-------------------------------------------------

            subscription.AddItem(testDAddress);
            subscription.AddItem(testMAddress);

            session.AddSubscription(subscription);

            subscription.Create();

            Console.WriteLine();
            Console.WriteLine("Subscription Created");
            Console.WriteLine("Monitoring:");
            Console.WriteLine(" - TestDAddress");
            Console.WriteLine(" - TestMAddress");
            Console.WriteLine();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            subscription.Delete(true);
            session.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR:");
            Console.WriteLine(ex);
        }
    }

    private static void TestDAddressChanged(
        MonitoredItem item,
        MonitoredItemNotificationEventArgs e)
    {
        foreach (var value in item.DequeueValues())
        {
            Console.WriteLine(
                $"[{DateTime.Now:HH:mm:ss}] {item.DisplayName} = {value.Value}");
        }
    }

    private static void TestMAddressChanged(
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