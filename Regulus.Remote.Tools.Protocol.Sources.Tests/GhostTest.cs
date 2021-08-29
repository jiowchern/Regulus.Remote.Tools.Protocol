using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Regulus.Remote.Tools.Protocol.Sources.Tests
{
    public class GhostTest
    {
        private readonly SyntaxTree[] _Souls;
        private readonly SyntaxTree[] _Ghosts;

        public GhostTest(params SyntaxTree[] souls)
        {
            var builder = new GhostBuilder(souls);
            _Ghosts = builder.Ghosts;
         
            _Souls = souls;
        }

        public GhostTest(System.Collections.Generic.IEnumerable<SyntaxTree> souls) : this(souls.ToArray())
        {
          
        }

        public async Task RunAsync()
        {
           
            var test = new CSharpSourceGeneratorVerifier<SourceGenerator>.Test
            {
                TestState =
                {
                    ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages(ImmutableArray.Create(
                        new PackageIdentity("Regulus.Remote.Protocol", "0.1.9.1"))),
                },


            };

            foreach (var syntaxTree in _Ghosts)
            {
               
                test.TestState.GeneratedSources.Add((typeof(SourceGenerator), syntaxTree.FilePath, syntaxTree.ToNormalizeWhitespace()));
            }
            foreach (var syntaxTree in _Souls)
            {
                test.TestState.Sources.Add(syntaxTree.ToNormalizeWhitespace());
            }
           
            await test.RunAsync();
        }
    }
}