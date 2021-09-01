using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

            _Souls = souls;

            IEnumerable<MetadataReference> references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(Regulus.Remote.Value<>).GetTypeInfo().Assembly.Location)
            };
            var compilation =  CSharpCompilation.Create(Guid.NewGuid().ToString() , souls, references) ;


            _Ghosts = new GhostBuilder(compilation).Ghosts.ToArray();
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