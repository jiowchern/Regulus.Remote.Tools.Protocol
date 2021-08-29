using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Regulus.Remote.Tools.Protocol.Sources.TestProject
{
}
namespace Regulus.Remote.Tools.Protocol.Sources.Tests
{
    using Microsoft.CodeAnalysis.Testing.Verifiers;

    using VerifyCS = CSharpSourceGeneratorVerifier<Regulus.Remote.Tools.Protocol.Sources.SourceGenerator>;

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