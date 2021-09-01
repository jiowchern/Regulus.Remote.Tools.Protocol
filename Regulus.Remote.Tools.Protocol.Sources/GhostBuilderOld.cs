using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    public class GhostBuilderOld
    {
        
        public readonly SyntaxTree[] Ghosts;
   

        public static SyntaxTree CreateGhost(InterfaceDeclarationSyntax root)
        {
            var extNamespace = "RegulusRemoteGhosts";
            var nss = root.Ancestors().OfType<NamespaceDeclarationSyntax>().ToArray();
            
            
            var namespaceDel = nss.Length == 1 ? $"{nss.Single().Name}." : "";
            var className = $"C{root.Identifier}";
            var fullName = $"{namespaceDel}{root.Identifier}";
            var source = $@"
namespace {namespaceDel}{extNamespace}
{{
    class {className} : Regulus.Remote.IGhost , {fullName}
    {{
        readonly bool _HaveReturn ;            
        readonly long _GhostId;
        public {className}(long id,bool have_return)
        {{
            _GhostId = id;
            _HaveReturn = have_return;
        }}
        long Regulus.Remote.IGhost.GetID()
        {{
            return _GhostId;
        }}

        bool Regulus.Remote.IGhost.IsReturnType()   
        {{
            return _HaveReturn;
        }}
        object Regulus.Remote.IGhost.GetInstance()
        {{
            return this;
        }}

        private event Regulus.Remote.CallMethodCallback _CallMethodEvent;

        event Regulus.Remote.CallMethodCallback Regulus.Remote.IGhost.CallMethodEvent
        {{
            add {{ this._CallMethodEvent += value; }}
            remove {{ this._CallMethodEvent -= value; }}
        }}

        private event Regulus.Remote.EventNotifyCallback _AddEventEvent;

        event Regulus.Remote.EventNotifyCallback Regulus.Remote.IGhost.AddEventEvent
        {{
            add {{ this._AddEventEvent += value; }}
            remove {{ this._AddEventEvent -= value; }}
        }}

        private event Regulus.Remote.EventNotifyCallback _RemoveEventEvent;

        event Regulus.Remote.EventNotifyCallback Regulus.Remote.IGhost.RemoveEventEvent
        {{
            add {{ this._RemoveEventEvent += value; }}
            remove {{ this._RemoveEventEvent -= value; }}
        }}
        {_BuildMethods(root)}
    }}
}}
";
            string path = $"{fullName}.{extNamespace}.cs";
            return SyntaxFactory.ParseSyntaxTree(source,null,path, System.Text.Encoding.UTF8);
           
        }

        private static string _BuildMethods(InterfaceDeclarationSyntax root)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var methodDeclarationSyntax in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                builder.Append(_BuildMethod(root, methodDeclarationSyntax));
            }

            return builder.ToString();
        }

        public GhostBuilderOld(IEnumerable<SyntaxTree> commons)
        {

            var ghosts = new System.Collections.Generic.List<SyntaxTree>();
            foreach (var common in commons)
            {
                foreach (var interfaceDeclarationSyntax in common.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>())
                {
                   var ghost =  CreateGhost(interfaceDeclarationSyntax);
                   ghosts.Add(ghost);
                }
            }

            Ghosts = ghosts.ToArray();

           
        }

        private string _BuildEvent(Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax event_field_declaration_syntax, SemanticModel model)
        {
            var symbol = model.GetDeclaredSymbol(event_field_declaration_syntax.Parent as InterfaceDeclarationSyntax);
            var id = event_field_declaration_syntax.Declaration.Variables[0].Identifier;

            
            var source =
$@"
Regulus.Remote.GhostEventHandler  _{id} = new Regulus.Remote.GhostEventHandler();
event {event_field_declaration_syntax.Declaration.Type} {symbol.ToDisplayString()}.{id}
{{
    add 
    {{
        var id = _{id}.Add(value);
        _AddEventEvent(typeof({symbol.ToDisplayString()}).GetEvent(""{id}""),id);
    }}
    remove
    {{
        var id = _{id}.Remove(value);
        _RemoveEventEvent(typeof({symbol.ToDisplayString()}).GetEvent(""{id}""),id);
    }}
}}
";
            return source;
        }

        private string _BuildProperty(PropertyDeclarationSyntax property_declaration_syntax, SemanticModel model)
        {
            var symbol = model.GetDeclaredSymbol(property_declaration_syntax);
            var t = symbol.Type as INamedTypeSymbol;
            var source = 
$@"
{t.ToDisplayString()} _{property_declaration_syntax.Identifier} = new {t.ToDisplayString()}();
{t.ToDisplayString()} {symbol.ToDisplayString()} {{ get{{ return _{property_declaration_syntax.Identifier};}} }}
";
            return source;
        }

        private static string _BuildMethod(InterfaceDeclarationSyntax root,MethodDeclarationSyntax method_declaration_syntax)
        {

            bool haveReturn = false;
            int idx = 0;
            var pl = (from p in method_declaration_syntax.ParameterList.Parameters
                     select p.WithIdentifier(SyntaxFactory.Identifier($"_{idx++}"))).ToArray();
            var method = SyntaxFactory.MethodDeclaration(method_declaration_syntax.ReturnType, method_declaration_syntax.Identifier);
            method = method.AddParameterListParameters(pl.ToArray());

            
            NameSyntax methodName = SyntaxFactory.ParseName(root.ToNamespaceTypeName());
            method = method.WithExplicitInterfaceSpecifier(SyntaxFactory.ExplicitInterfaceSpecifier(methodName));

            string retValue = "";
            string retRetValue = "";
            string retRetValueVar = "null";
            if (haveReturn)
            {
               // retValue = $"var returnValue = new {symbol.ReturnType}();";
                retRetValue = "return returnValue ;";
                retRetValueVar = "returnValue";
            }

            method = method.WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(
$@"
    {retValue}
        var info = typeof({root.ToNamespaceTypeName()}).GetMethod(""{method_declaration_syntax.Identifier}"");
        _CallMethodEvent(info , new object[] {{{string.Join(",", from p in pl select p.Identifier)}}} , {retRetValueVar});                    
    {retRetValue}
")));

            var text = method.GetText(System.Text.Encoding.UTF8).ToString();
            return text;
        }


        
    }
}