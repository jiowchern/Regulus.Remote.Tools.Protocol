using Regulus.Remote;

namespace Regulus.Remote.Tools.Protocol.Sources.TestCommon
{
    
    public interface ISample
    {

        Regulus.Remote.Property<int> LastValue { get; }
        Regulus.Remote.Value<int> Add(int num1,int num2);

        
        event System.Action<int> IntsEvent;
        
        Regulus.Remote.Notifier<INumber> Numbers { get; }
        Regulus.Remote.Value<bool> RemoveNumber(int val);
    }
}
