using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
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
        public async Task Test2()
        {
            var code = @"interface IAA{}";
            var generated = "interface IAA{}";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code },
                    GeneratedSources =
                    {
                        (typeof(SourceGenerator), "GeneratedFileName", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha256)),
                    },ReferenceAssemblies = ReferenceAssemblies.Default.AddAssemblies(ImmutableArray.Create("Regulus.Remote.Tools.Protocol.Sources"))
                },
            }.RunAsync();
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