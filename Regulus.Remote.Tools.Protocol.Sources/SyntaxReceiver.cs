using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        private readonly List<GhostBuilder> _Ghosts;
        internal readonly System.Collections.Generic.IReadOnlyCollection<GhostBuilder> Ghosts;

        private readonly List<ProtocoNewlBuilder> _Protocols;
        internal readonly System.Collections.Generic.IReadOnlyCollection<ProtocoNewlBuilder> Protocols;

        private readonly System.Collections.Generic.HashSet<string> _SerializerTypes;
        public SyntaxReceiver()
        {
            _Ghosts = new List<GhostBuilder>();
            Ghosts = _Ghosts;
            _Protocols = new List<ProtocoNewlBuilder>();
            _SerializerTypes = new HashSet<string>();
            Protocols = _Protocols;
        }
        
     
        void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var node = context.Node;
            if(node.IsKind(SyntaxKind.InterfaceDeclaration))
            {
                _Ghosts.Add(_BuildGhost(context));
            }
            if (node.IsKind(SyntaxKind.MethodDeclaration))
            {
                ProtocoNewlBuilder builder;
                var method = node as MethodDeclarationSyntax;
                if (_TryBuildProtocol(out builder, method, context.SemanticModel))
                {
                    _Protocols.Add(builder);
                }

                // todo : 取得參數序列化
                var args = method.ParameterList.Parameters;
                foreach(ParameterSyntax arg in args)
                {
                    var name = arg.Type.ToFullString();
                    var syn2 = context.SemanticModel.GetDeclaredSymbol(arg.Type);
                    var syn3 =context.SemanticModel.GetTypeInfo(arg.Type);
                    var syn = context.SemanticModel.Compilation.GetTypeByMetadataName(name);
                    var dec = new TypeDecomposer(arg);
                    
                    //_SerializerTypes.Add();
                }
                
            }
            if (node.IsKind(SyntaxKind.ArrayType))
            {
                // todo : 取得序列化
            }
            if (node.IsKind(SyntaxKind.StructDeclaration))
            {
                // todo : 取得序列化
            }
            if (node.IsKind(SyntaxKind.GenericName))
            {
                // todo : 取得泛型參數序列化
            }
            if (node.IsKind(SyntaxKind.FieldDeclaration))
            {
                // todo : 取得序列化
            }

        }

        private bool _TryBuildProtocol(out ProtocoNewlBuilder builder, MethodDeclarationSyntax syntax, SemanticModel model)
        {
            builder = null;

            var symbol = model.GetDeclaredSymbol(syntax);
            var display = symbol.ToDisplayString();
            var attr = model.Compilation.GetTypeByMetadataName("Regulus.Remote.ProtocolProvideAttribute");
            if (attr == null)
                return false;
            var haveAttr = (from a in symbol.GetAttributes()
                               where a.AttributeClass.Equals(attr , SymbolEqualityComparer.Default)
                               select a).Any();

            builder = new ProtocoNewlBuilder(syntax , model);
            return true;
        }

        private GhostBuilder _BuildGhost(GeneratorSyntaxContext context)
        {
            var node = context.Node as InterfaceDeclarationSyntax;
            var ghost = new GhostBuilder(node, context.SemanticModel);
            return ghost;
        }
    }
}
