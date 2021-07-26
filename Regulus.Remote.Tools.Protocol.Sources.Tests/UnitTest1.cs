using NUnit.Framework;

namespace Regulus.Remote.Tools.Protocol.Sources.TestProject
{
}
namespace Regulus.Remote.Tools.Protocol.Sources.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var entry = new Regulus.Remote.Tools.Protocol.Sources.TestProject.Entry();
            IProtocol protocol = Regulus.Remote.Tools.Protocol.Sources.TestCommon.ProtocolProvider.Create();
            var service = new Regulus.Remote.Standalone.Service(entry, protocol);
            var agent = new Regulus.Remote.Ghost.Agent(protocol);
            service.Join(agent,null);
        }
    }
}