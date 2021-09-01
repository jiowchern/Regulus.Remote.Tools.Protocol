using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    
    using System.Linq;

    
    public class GhostBuilder
    {
        public readonly IReadOnlyCollection<SyntaxTree> Ghosts;
        public readonly IReadOnlyCollection<SyntaxTree> Events;
        public GhostBuilder(Compilation compilation)
        {
            var ghosts = 
                from syntax in compilation.SyntaxTrees
                let SemanticModel = compilation.GetSemanticModel(syntax)
                    from interfaceSyntax in syntax.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>()
                    select _BuildGhost(interfaceSyntax, SemanticModel);
            Ghosts= ghosts.ToArray();


            var events =
                from syntax in compilation.SyntaxTrees
                let SemanticModel = compilation.GetSemanticModel(syntax)
                from interfaceSyntax in syntax.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>()
                from eventSyntax in interfaceSyntax.DescendantNodes().OfType<EventFieldDeclarationSyntax>()
                select _BuildGhostEvent(eventSyntax, SemanticModel);
            Events = events.ToArray();
        }

        private static SyntaxTree _BuildGhostEvent(EventFieldDeclarationSyntax eventSyntax, SemanticModel semanticModel)
        {
//             var source = $@"
//     using System;  
//     using System.Collections.Generic;
//     
//     namespace {nameSpace}.Invoker.{name} 
//     {{ 
//         public class {eventName} : Regulus.Remote.IEventProxyCreator
//         {{
//
//             Type _Type;
//             string _Name;
//             
//             public {eventName}()
//             {{
//                 _Name = ""{eventName}"";
//                 _Type = typeof({type.FullName});                   
//             
//             }}
//             Delegate Regulus.Remote.IEventProxyCreator.Create(long soul_id,int event_id,long handler_id, Regulus.Remote.InvokeEventCallabck invoke_Event)
//             {{                
//                 var closure = new Regulus.Remote.GenericEventClosure{_GetTypes(argTypes)}(soul_id , event_id ,handler_id, invoke_Event);                
//                 return new Action{_GetTypes(argTypes)}(closure.Run);
//             }}
//         
//
//             Type Regulus.Remote.IEventProxyCreator.GetType()
//             {{
//                 return _Type;
//             }}            
//
//             string Regulus.Remote.IEventProxyCreator.GetName()
//             {{
//                 return _Name;
//             }}            
//         }}
//     }}
//                 ";
            throw new NotImplementedException();
        }

        private static SyntaxTree _BuildGhost(InterfaceDeclarationSyntax interface_syntax, SemanticModel semantic_model)
        {
            INamedTypeSymbol interfaceSymbol = semantic_model.GetDeclaredSymbol(interface_syntax);
            var typeName = interfaceSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat );
            var fullName = interfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat );

            var namespaceName = _BuildNamesapceName(interface_syntax, semantic_model);


            var source = $@"
namespace {namespaceName}RegulusRemoteGhosts
{{
    class C{typeName} : Regulus.Remote.IGhost , {fullName}
    {{
        readonly bool _HaveReturn ;            
        readonly long _GhostId;
        public C{typeName}(long id,bool have_return)
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
        {_BuildMethods(interface_syntax, semantic_model)}
        {_BuildEvents(interface_syntax, semantic_model)}
        {_BuildPropertys(interface_syntax, semantic_model)}
    }}
}}
";

            return SyntaxFactory.ParseSyntaxTree(source,null,$"{namespaceName}.{typeName}.RegulusRemoteGhosts.cs");
        }

        private static string _BuildPropertys(InterfaceDeclarationSyntax root, SemanticModel semantic_model)
        {
            var trees = from syntax in root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                select _BuildProperty(syntax, semantic_model);


            return string.Join("\r\n", trees);
        }
        private static string _BuildProperty(PropertyDeclarationSyntax property_declaration_syntax, SemanticModel model)
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
        private static string _BuildEvents(InterfaceDeclarationSyntax root, SemanticModel semantic_model)
        {
            var trees = from syntax in root.DescendantNodes().OfType<EventFieldDeclarationSyntax>()
                select _BuildEvent(syntax, semantic_model);


            return string.Join("\r\n", trees);
        }

        private static string _BuildMethods(InterfaceDeclarationSyntax root, SemanticModel semantic_model)
        {
            var trees = from syntax in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                select _BuildMethod(syntax, semantic_model);

            
            return string.Join("\r\n", trees);
        }

        private static string _BuildEvent(Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax event_field_declaration_syntax, SemanticModel model)
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

        private static string _BuildMethod(MethodDeclarationSyntax method_declaration_syntax, SemanticModel semanticModel)
        {
           
            var interfaceSyntax = method_declaration_syntax.Ancestors().OfType<InterfaceDeclarationSyntax>().Single();
            var interfaceSymbol = semanticModel.GetDeclaredSymbol(interfaceSyntax);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method_declaration_syntax);
            
            bool haveReturn = false;
            int idx = 0;
            var pl = (from p in method_declaration_syntax.ParameterList.Parameters
                select p.WithIdentifier(SyntaxFactory.Identifier($"_{idx++}"))).ToArray();
            var method = SyntaxFactory.MethodDeclaration(method_declaration_syntax.ReturnType, method_declaration_syntax.Identifier);
            method = method.AddParameterListParameters(pl.ToArray());

            
            NameSyntax methodName = SyntaxFactory.ParseName(interfaceSymbol.ToDisplayString());
            method = method.WithExplicitInterfaceSpecifier(SyntaxFactory.ExplicitInterfaceSpecifier(methodName));

            string retValue = "";
            string retRetValue = "";
            string retRetValueVar = "null";
           
           
            if (methodSymbol.ReturnType.SpecialType == SpecialType.System_Void)
            {
                 retValue = "";
                 retRetValue = "";
                 retRetValueVar = "null";
            }
            else if (semanticModel.Compilation.GetTypeByMetadataName("Regulus.Remote.Value`1") == methodSymbol.ReturnType.OriginalDefinition)
            {
                retValue = $"var returnValue = new {methodSymbol.ReturnType}();";
                retRetValue = "return returnValue ;";
                retRetValueVar = "returnValue";
            }
            else
            {
                retValue = $"";
                retRetValue = "throw new NotSupportedException() ;";
                retRetValueVar = "null";
            }
          
            method = method.WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(
                $@"
    {retValue}
        var info = typeof({interfaceSymbol.ToDisplayString()}).GetMethod(""{method_declaration_syntax.Identifier}"");
        _CallMethodEvent(info , new object[] {{{string.Join(",", from p in pl select p.Identifier)}}} , {retRetValueVar});                    
    {retRetValue}
")));

            var text = method.GetText(System.Text.Encoding.UTF8).ToString();
            return text;
        }

        private static string _BuildNamesapceName(InterfaceDeclarationSyntax interface_syntax, SemanticModel semantic_model)
        {
            var namespaceSyntax = interface_syntax.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceSyntax == null)
                return "";
            var namespaceSymbol = semantic_model.GetDeclaredSymbol(namespaceSyntax);
            var namespaceName = namespaceSymbol.ToDisplayString();
            return namespaceName + ".";
        }
    }
}