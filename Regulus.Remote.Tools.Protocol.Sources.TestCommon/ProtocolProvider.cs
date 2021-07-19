using System;
using System.Collections.Generic;
using System.Text;

namespace Regulus.Remote
{
    public class ProtocolProviderAttribute : System.Attribute
    {

    }
}
namespace Regulus.Remote.Tools.Protocol.Sources.TestCommon
{
    public class ProtocolProvider
    {
        [ProtocolProviderAttribute]
        public static Regulus.Remote.IProtocol New()
        {
            return null;
        }
    }
}
