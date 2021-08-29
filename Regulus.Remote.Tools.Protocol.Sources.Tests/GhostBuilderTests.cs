using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Regulus.Remote.Tools.Protocol.Sources.Tests
{
    public class GhostBuilderTests
    {
        [Test]
        public async Task InterfaceTest()
        {
            var source = @"

namespace NS
{
    public interface IA {
      
    }
}
";
            var syntaxBuilder = new Regulus.Remote.Tools.Protocol.Sources.SyntaxTreeBuilder(SourceText.From(source, System.Text.Encoding.UTF8));

            await new GhostTest(syntaxBuilder.Tree).RunAsync();


        }

        [Test]
        public async Task NoNamespaceInterfaceTest()
        {
            var source = @"

 public interface IA {
      
    }
";
            var syntaxBuilder = new Regulus.Remote.Tools.Protocol.Sources.SyntaxTreeBuilder(SourceText.From(source, System.Text.Encoding.UTF8));

            await new GhostTest(syntaxBuilder.Tree).RunAsync();


        }
    }
}