using Regulus.Remote.Standalone;

namespace Regulus.Remote.Tools.Protocol.Sources.TestCommon.Tests
{
    public class TestEnv<T,T2> where T : Regulus.Remote.IBinderProvider, System.IDisposable
    {
        readonly ThreadUpdater _AgentUpdater;
        readonly IService _Service;
        readonly Ghost.IAgent _Agent;
        public readonly INotifierQueryable Queryable;
        public readonly T Entry;

        public TestEnv(T entry)
        {

            Entry = entry;
            IProtocol protocol = Regulus.Remote.Protocol.ProtocolProvider.Create(typeof(T2).Assembly);
            _Service = new Regulus.Remote.Standalone.Service(entry, protocol);
            _Agent = new Regulus.Remote.Ghost.Agent(protocol);
            _Service.Join(_Agent);


            Queryable = _Agent;

            _AgentUpdater = new ThreadUpdater(_Update);
            _AgentUpdater.Start();
        }

        private void _Update()
        {
            _Agent.Update();
        }

        public void Dispose()
        {
            Entry.Dispose();
            _AgentUpdater.Stop();
            _Service.Leave(_Agent);
            _Service.Dispose();

        }
    }
}