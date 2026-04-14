// Copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

namespace PortForCommandPalette.Enums
{
    public enum TcpState
    {
        Unknown,
        Closed,
        Listen,
        SynSent,
        SynReceived,
        Established,
        FinWait1,
        FinWait2,
        CloseWait,
        Closing,
        LastAck,
        TimeWait,
        DeleteTcb
    }
}
