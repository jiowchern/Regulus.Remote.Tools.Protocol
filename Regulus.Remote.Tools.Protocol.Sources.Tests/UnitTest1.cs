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
        public async Task Test2()
        {

            var code =

@"
namespace NS
{
    public interface IA{
        void Method1();
    }
}
";
            var generated = $@"
namespace NS.Ghosts
{{
    class CIA : Regulus.Remote.IGhost , NS.IA
    {{
        readonly bool _HaveReturn ;            
        readonly long _GhostId;
        public CIA(long id,bool have_return)
        {{
            _GhostId = id;
            _HaveReturn = have_return;
        }}
        long Regulus.Remote.IGhost.GetID()
        {{
            return _GhostId;
        }}

        bool Regulus.Remote.IGhost.IsReturnType()   
        {{
            return _HaveReturn;
        }}
        object Regulus.Remote.IGhost.GetInstance()
        {{
            return this;
        }}

        private event Regulus.Remote.CallMethodCallback _CallMethodEvent;

        event Regulus.Remote.CallMethodCallback Regulus.Remote.IGhost.CallMethodEvent
        {{
            add {{ this._CallMethodEvent += value; }}
            remove {{ this._CallMethodEvent -= value; }}
        }}

        private event Regulus.Remote.EventNotifyCallback _AddEventEvent;

        event Regulus.Remote.EventNotifyCallback Regulus.Remote.IGhost.AddEventEvent
        {{
            add {{ this._AddEventEvent += value; }}
            remove {{ this._AddEventEvent -= value; }}
        }}

        private event Regulus.Remote.EventNotifyCallback _RemoveEventEvent;

        event Regulus.Remote.EventNotifyCallback Regulus.Remote.IGhost.RemoveEventEvent
        {{
            add {{ this._RemoveEventEvent += value; }}
            remove {{ this._RemoveEventEvent -= value; }}
        }}


        void NS.IA.Method1(){{

            var info = typeof(NS.IA).GetMethod(""Method1"");
            _CallMethodEvent(info , new object[] {{}} , null);                    
        }}
    }}
}}";
            generated = SyntaxFactory.ParseSyntaxTree(generated,null , "",System.Text.Encoding.UTF8).GetRoot().NormalizeWhitespace().ToFullString(); ;

           // generated = new string((from c in generated where !char.IsWhiteSpace(c) select c).ToArray());
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code },
                    GeneratedSources =
                    {
                        (typeof(SourceGenerator), "test-IA.cs", SourceText.From(generated, Encoding.UTF8)),
                    },
                   /* AdditionalFiles =
                    {
                        ("test-IA.cs", generated),
                    },*/
                    ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages(ImmutableArray.Create(
                        new PackageIdentity("Regulus.Remote.Protocol", "0.1.9.1"))),
                },

                //estBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
                
            };
            await test.RunAsync();
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