namespace PortForCommandPalette.Classes;

public class PortInfo
{
    public string Protocol { get; set; } = string.Empty;
    public string LocalAddress { get; set; } = string.Empty;
    public int LocalPort { get; set; }
    public string RemoteAddress { get; set; } = string.Empty;
    public int RemotePort { get; set; }
    public string State { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ProcessPath { get; set; } = string.Empty;
    public string Id => Helpers.IdGenerator.GetPortId(Protocol, LocalAddress, LocalPort, RemoteAddress, RemotePort);

    public string LocalAddressDisplay => $"{LocalAddress}:{LocalPort}";
    public string RemoteAddressDisplay => string.IsNullOrEmpty(RemoteAddress) ? "*:*" : $"{RemoteAddress}:{RemotePort}";
}