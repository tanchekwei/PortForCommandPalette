// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace PortForCommandPalette.Classes
{
    public class ErrorCommandPaletteMessage : ToastStatusMessage
    {
        public ErrorCommandPaletteMessage(string message) : base(message)
        {
            Message.State = MessageState.Error;
        }
    }
}
