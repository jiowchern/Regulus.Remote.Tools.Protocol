using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Regulus.Remote.Tools.Protocol.Sources.Tests
{
    public class ProtocolBuilderTests
    {

        [Test]
        public async Task SerializableExtractorTest()
        {
            var source = @"
namespace NS1
{
    public struct C2
    {
        public int F1;
        public string F2;
        public float F3;
    }
    public class C1
    {
        public C2;
    }
    public interface IB{
        Regulus.Remote.Property<C1> Property1 {get;}
    }
    public interface IA
    {
        Regulus.Remote.Value<C2> Method(C2 a1 , C1 a2);
        Regulus.Remote.Notifier<IB> Property1 {get;}
    }
}

";
            var tree=CSharpSyntaxTree.ParseText(source);
            Compilation compilation =tree.Compilation();
            IEnumerable<INamedTypeSymbol> symbols = new SerializableExtractor(compilation).Symbols;

            var cSymbols = new[]
            {
                compilation.GetTypeByMetadataName("NS1.C2"),
                compilation.GetTypeByMetadataName("NS1.C1")
            };
            NUnit.Framework.Assert.IsTrue(symbols.Any(s1 => cSymbols.Any(s2 => s1==s2)));

        }
    }
}