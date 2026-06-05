# OPCUA_Client
.Net console app for OPC UA Client. 
Connects to the OPC server. Then subscribes to 2 datanodes and display its data in console


*Nuget Package installed*
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Tools
Microsoft.Extensions.Configuration
OPCFoundation.NetStandard.Opc.Ua.Client
OPCFoundation.NetStandard.Opc.Ua.Core

*Config*
in Program.cs change "opc.tcp://address:port" with your specific address and port

*to change node subscription*
in Models/OpcTags.cs, the line [public const string TestDAddress ="ns=2;s=DataSource::PLC_R04_CPU.TestDAddress";] is my default NodeId for my OPC server. you can change the whole line to your server NodeId
