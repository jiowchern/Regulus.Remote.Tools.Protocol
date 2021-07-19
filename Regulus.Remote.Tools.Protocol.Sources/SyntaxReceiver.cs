using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        private readonly List<GhostBuilder> _Ghosts;
        internal readonly System.Collections.Generic.IReadOnlyCollection<GhostBuilder> Ghosts;
        /*private readonly List<MethodDeclarationSyntax> _Protocols;
        internal readonly System.Collections.Generic.IReadOnlyCollection<GhostBuilder> Protocols;*/


        public SyntaxReceiver()
        {
            _Ghosts = new List<GhostBuilder>();
            Ghosts = _Ghosts;
            //_Protocols = new List<MethodDeclarationSyntax>();
        }
        
     
        void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var node = context.Node;
            if(node.IsKind(SyntaxKind.InterfaceDeclaration))
            {
                _Ghosts.Add(_BuildGhost(context));
            }
        }

        private GhostBuilder _BuildGhost(GeneratorSyntaxContext context)
        {
            var node = context.Node as InterfaceDeclarationSyntax;
            var ghost = new GhostBuilder(node, context.SemanticModel);
            return ghost;
        }
    }
}
