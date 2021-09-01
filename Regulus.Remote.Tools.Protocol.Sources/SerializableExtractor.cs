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


            var typeSyntaxs = from tree in compilation.SyntaxTrees
                from interfaceSyntax in tree.GetRoot().DescendantNodesAndSelf().OfType<TypeSyntax>()
                from typeSyntax in interfaceSyntax.DescendantNodes().OfType<TypeSyntax>()
                let symbols = compilation.GetSymbolsWithName(typeSyntax.ToFullString()).OfType<INamedTypeSymbol>()
                    from symbol in symbols
                where symbol.TypeKind == TypeKind.Class || symbol.TypeKind == TypeKind.Enum || symbol.TypeKind == TypeKind.Array || symbol.TypeKind == TypeKind.Struct 
                              select symbol;

            Symbols = new HashSet<INamedTypeSymbol>(typeSyntaxs);

        }
    }
}