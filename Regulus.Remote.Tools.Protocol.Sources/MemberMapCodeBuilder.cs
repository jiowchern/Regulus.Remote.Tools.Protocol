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
        public readonly string EventInfosCode;
        public MemberMapCodeBuilder(Compilation compilation)
        {
            _Compilation = compilation;

            var methods = from tree in compilation.SyntaxTrees
                from interfaceSyntax in tree.GetRoot().DescendantNodesAndSelf().OfType<InterfaceDeclarationSyntax>()
                from methodSyntax in interfaceSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>()
                select _BuildMethodInfo(methodSyntax);

            MethodInfosCode = string.Join(",",methods);

            var events = from tree in compilation.SyntaxTrees
                from interfaceSyntax in tree.GetRoot().DescendantNodesAndSelf().OfType<InterfaceDeclarationSyntax>()
                from eventSyntax in interfaceSyntax.DescendantNodes().OfType<EventFieldDeclarationSyntax>()
                select _BuildEventInfo(eventSyntax);

            EventInfosCode = string.Join(",", events);
        }

        private string _BuildEventInfo(EventFieldDeclarationSyntax event_syntax)
        {

            var model = _Compilation.GetSemanticModel(event_syntax.SyntaxTree);
            var interfaceSymbol = model.GetDeclaredSymbol(event_syntax.Parent) as INamedTypeSymbol;
            
            
            string typeName = interfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat); ;
            string eventName= event_syntax.Declaration.Variables[0].ToFullString();
            return $@"typeof({typeName}).GetEvent(""{eventName}"")";
           
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