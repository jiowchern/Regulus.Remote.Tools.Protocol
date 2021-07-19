using Regulus.Remote;

namespace Regulus.Remote.Tools.Protocol.Sources.TestCommon
{
    public interface INext
    {
        Remote.Value<bool> Next();
    }
    public interface INumber
    {
        Property<int> Value { get; }
    }
}
