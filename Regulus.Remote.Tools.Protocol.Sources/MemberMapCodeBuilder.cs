using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    using System.Linq;
    class MemberMapCodeBuilder
    {
        private readonly Compilation _Compilation;
        public readonly string MethodInfosCode;
        public MemberMapCodeBuilder(Compilation compilation)
        {
            _Compilation = compilation;

            var methods = from tree in compilation.SyntaxTrees
                from interfaceSyntax in tree.GetRoot().DescendantNodesAndSelf().OfType<InterfaceDeclarationSyntax>()
                from methodSyntax in interfaceSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>()
                select methodSyntax;

            MethodInfosCode = _BuildMethodInfos(methods);
        }

        private string _BuildMethodInfos(IEnumerable<MethodDeclarationSyntax> methods)
        {

            var codes = from method in methods
                select _BuildMethodInfo(method);

            return string.Join(",", codes);
       
            
        }

        private string _BuildMethodInfo(MethodDeclarationSyntax method_syntax)
        {
            var model = _Compilation.GetSemanticModel(method_syntax.SyntaxTree);
            var methodSymbol = model.GetDeclaredSymbol(method_syntax) as IMethodSymbol;
            
            var interfaceSymbol = methodSymbol.ContainingType;
          
            
            string typeName = interfaceSymbol.ToDisplayString( SymbolDisplayFormat.FullyQualifiedFormat);
            var argTypes = from arg in methodSymbol.Parameters
                select arg.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            var comma = argTypes.Any() ? "," : "";
            string methodArgTypes= comma + string.Join(",", argTypes);
            string methodName =  methodSymbol.MetadataName;

            int number=2;
            string beginArgName = argTypes.Any() ? "_1" : "";
            string methodArgNames = argTypes.Skip(1).Aggregate(beginArgName, (s,a) => $"{s},_{number++}");
            string methodArgNamesWithComma = comma + methodArgNames;

            return $"new Regulus.Utility.Reflection.TypeMethodCatcher((System.Linq.Expressions.Expression<System.Action<{typeName}{methodArgTypes}>>)((ins{methodArgNamesWithComma}) => ins.{methodName}({methodArgNames}))).Method";
       
        }
    }
}