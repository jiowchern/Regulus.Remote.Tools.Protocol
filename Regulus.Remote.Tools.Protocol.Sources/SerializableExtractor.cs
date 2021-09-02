using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    using System.Linq;
    class SerializableExtractor
    {
        public readonly IReadOnlyCollection<INamedTypeSymbol> Symbols;

        public SerializableExtractor(Compilation compilation)
        {

            
            var typeSyntaxs = 
                from tree in compilation.SyntaxTrees
                let semanticModel = compilation.GetSemanticModel(tree)
                from node in tree.GetRoot().DescendantNodes()
                let symbol = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol
                where symbol!=null &&(symbol.TypeKind == TypeKind.Class || symbol.TypeKind == TypeKind.Enum || symbol.TypeKind == TypeKind.Array || symbol.TypeKind == TypeKind.Struct) 
                select symbol;

            Symbols = new HashSet<INamedTypeSymbol>(typeSyntaxs);

        }
    }
}