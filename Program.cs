
using OPCUA_Client.Models;
using OPCUA_Client.Services;
using Microsoft.Extensions.Configuration;
using Opc.Ua;       
using Opc.Ua.Configuration;


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

    TransportConfigurations =
        new TransportConfigurationCollection(),

    TransportQuotas = new TransportQuotas
    {
        OperationTimeout = 15000
    },

    ClientConfiguration = new ClientConfiguration
    {
        DefaultSessionTimeout = 60000
    }
};

await config.Validate(ApplicationType.Client);

config.CertificateValidator.CertificateValidation += (s, e) =>
{
    Console.WriteLine(
        $"Accepting Certificate: {e.Certificate.Subject}");

    e.Accept = true;
};

var opc =
    new OpcClientService();

await opc.Connect(
    config,
    "opc.tcp://192.168.87.102:52220");

opc.Subscribe(
    OpcTags.TestDAddress,
    nameof(OpcTags.TestDAddress));

opc.Subscribe(
    OpcTags.TestMAddress,
    nameof(OpcTags.TestMAddress));

Console.WriteLine();
Console.WriteLine("Press ENTER to exit...");
Console.ReadLine();

opc.Disconnect();