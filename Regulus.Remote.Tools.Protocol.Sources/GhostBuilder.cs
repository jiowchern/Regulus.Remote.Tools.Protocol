using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    internal class GhostBuilder
    {
        internal readonly string Name;
        internal readonly SyntaxTree Syntax;
        internal object Common;
        internal object Protocol;
        internal string[] Events;

        public GhostBuilder(InterfaceDeclarationSyntax soul, SemanticModel model)
        {
            var soulInfo = model.GetDeclaredSymbol(soul);

            var methods = (from m in soul.Members
                            where m.IsKind(SyntaxKind.MethodDeclaration)
                            select _BuildMethod(m as MethodDeclarationSyntax, model));

            var propertys = (from m in soul.Members
                           where m.IsKind(SyntaxKind.PropertyDeclaration)
                           select _BuildProperty(m as PropertyDeclarationSyntax, model));
            
            var events = (from m in soul.Members
                             where m.IsKind(SyntaxKind.EventFieldDeclaration)
                             select _BuildEvent(m as Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax, model));

            Syntax = SyntaxFactory.ParseSyntaxTree($@"
namespace {soulInfo.ContainingNamespace}.Ghosts
{{
    class C{soul.Identifier} : Regulus.Remote.IGhost , {soulInfo}
    {{
        readonly bool _HaveReturn ;            
        readonly long _GhostId;
        public C{soul.Identifier}(long id,bool have_return)
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

        {string.Join("\r\n", methods)}
        {string.Join("\r\n", propertys)}
        {string.Join("\r\n", events)}

    }}
}}
", null, "", System.Text.Encoding.UTF8);
            Name = $"test-{soul.Identifier}.cs";
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

        private string _BuildMethod(MethodDeclarationSyntax method_declaration_syntax, SemanticModel model)
        {
            var symbol = model.GetDeclaredSymbol(method_declaration_syntax) ;
            
            int idx = 0;
            var pl = (from p in method_declaration_syntax.ParameterList.Parameters
                     select p.WithIdentifier(SyntaxFactory.Identifier($"_{idx++}"))).ToArray();
            var method = SyntaxFactory.MethodDeclaration(method_declaration_syntax.ReturnType, method_declaration_syntax.Identifier);
            method = method.AddParameterListParameters(pl.ToArray());
            method = method.WithExplicitInterfaceSpecifier(SyntaxFactory.ExplicitInterfaceSpecifier(SyntaxFactory.IdentifierName(symbol.ReceiverType.ToString())));

            string retValue = "";
            string retRetValue = "";
            string retRetValueVar = "null";
            if (!symbol.ReturnsVoid)
            {
                retValue = $"var returnValue = new {symbol.ReturnType}();";
                retRetValue = "return returnValue ;";
                retRetValueVar = "returnValue";
            }

            method = method.WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(
$@"
    {retValue}
        var info = typeof({symbol.ReceiverType}).GetMethod(""{method_declaration_syntax.Identifier}"");
        _CallMethodEvent(info , new object[] {{{string.Join(",", from p in pl select p.Identifier)}}} , {retRetValueVar});                    
    {retRetValue}
")));

            var text = method.GetText(System.Text.Encoding.UTF8).ToString();
            return text;
        }

      
    }
}