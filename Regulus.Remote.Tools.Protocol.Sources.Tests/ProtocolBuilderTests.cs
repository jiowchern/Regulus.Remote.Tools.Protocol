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
        public async Task EventProviderCodeBuilderTest()
        {

            var source = @"
public interface IA
    {
        event System.Action<int> Event1;
        event System.Action Event2;
    }
namespace NS1
{
    
    
public interface IB
    {
        event System.Action<int> Event1;
    }
}

";
            var tree = CSharpSyntaxTree.ParseText(source);
            Compilation compilation = tree.Compilation();

            var interfaceMap = new EventProviderCodeBuilder(new GhostBuilder(compilation).Events);
            NUnit.Framework.Assert.AreEqual("new global::RegulusRemoteGhosts.IA_Event1(),new global::RegulusRemoteGhosts.IA_Event2(),new global::NS1.RegulusRemoteGhosts.IB_Event1()", interfaceMap.Code);
        }

        [Test]
        public async Task InterfaceProviderCodeBuilderTest()
        {
            var source = @"
namespace NS1
{
    
    public interface IA
    {
        
    }
public interface IB
    {
        
    }
}

";
            var tree = CSharpSyntaxTree.ParseText(source);
            Compilation compilation = tree.Compilation();
            
            var interfaceMap = new InterfaceProviderCodeBuilder(new GhostBuilder(compilation).Ghosts);
            NUnit.Framework.Assert.AreEqual("{typeof(global::NS1.IA),typeof(global::NS1.RegulusRemoteGhosts.CIA)},{typeof(global::NS1.IB),typeof(global::NS1.RegulusRemoteGhosts.CIB)}", interfaceMap.Code);
        }

        [Test]
        public async Task SerializableExtractor3Test()
        {
            var source = @"
namespace NS1
{
    public class CReturn
    {
        public CReturn Field1;
    }
    public class CUseless
    {
        public CArg1 Field1;
    }
    public enum ENUM1
    {
        ITEM1,
    }
    public struct CArg3
    {
        public ENUM1 Field2;
        public string Field1;
    }
    public struct CArg2
    {
        public CArg2 Field3;
        public ENUM1 Field2;
        public string Field1;
    }
    public class CArg1
    {
        public CArg2 Field1;
    }
    public interface IB
    {
        event System.Action<float , CArg1,CArg3[][]> Event1;
        Regulus.Remote.Property<string> Property1 {get;}
    }
    public interface IA
    {
        Regulus.Remote.Value<CReturn> Method(CArg1 a1,int a2,System.Guid id);
        Regulus.Remote.Notifier<IB> Property1 {get;}
    }
}

";
            var tree = CSharpSyntaxTree.ParseText(source);
            Compilation compilation = tree.Compilation();
            var symbols = new SerializableExtractor(compilation).Symbols.Select(s=>s.ToDisplayString());
           
            var cSymbols = new[]
            {
                "NS1.CReturn",
                "NS1.CArg1",
                "NS1.CArg2",
                "NS1.CArg3",
                "NS1.CArg3[]",
                "NS1.CArg3[][]",
                "int",
                "string",
                "System.Guid",
                "float",
                "NS1.ENUM1",
            };
           
            var count = symbols.Except(cSymbols).Count();
            NUnit.Framework.Assert.AreEqual(0, count);
        }


        [Test]
        public async Task SerializableExtractor1Test()
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
            var symbols = new SerializableExtractor(compilation).Symbols;

            var cSymbols = new[]
            {
                compilation.GetTypeByMetadataName("NS1.C2"),
                compilation.GetTypeByMetadataName("NS1.C1")
            };
            var count = cSymbols.Except(symbols).Count();
            NUnit.Framework.Assert.AreEqual(0, count);

        }

        [Test]
        public async Task SerializableExtractor2Test()
        {
            var source = @"
namespace NS1
{
    public struct C2
    {
        
    }
    public class C1
    {
        public C2 Field1;
    }
    public interface IB{
        Regulus.Remote.Property<C1> Property1 {get;}
    }
    public interface IA
    {
        void Method( C1 a2);
        Regulus.Remote.Notifier<IB> Property1 {get;}
    }
}

";
            var tree = CSharpSyntaxTree.ParseText(source);
            Compilation compilation = tree.Compilation();
            var  symbols = new SerializableExtractor(compilation).Symbols;

            var cSymbols = new[]
            {
                compilation.GetTypeByMetadataName("NS1.C2"),
                compilation.GetTypeByMetadataName("NS1.C1")
            };
            var count = cSymbols.Except(symbols).Count();
            NUnit.Framework.Assert.AreEqual(0,count);

        }
    }
}