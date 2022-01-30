﻿using System;
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

        private static SyntaxTree _BuildGhostEvent(EventFieldDeclarationSyntax event_syntax, SemanticModel semantic_model)
        {

            string eventName = _BuildEventName(event_syntax);
            var interfaceSyntax = event_syntax.Parent as InterfaceDeclarationSyntax;
            
            var namespaceName = _BuildNamesapceName(interfaceSyntax, semantic_model);
            var fieldName = event_syntax.Declaration.Variables[0];
            var interfaceSymbol = semantic_model.GetDeclaredSymbol(interfaceSyntax);
            var eventSymbol =(interfaceSymbol.GetMembers(fieldName.ToString() ).Single() as IEventSymbol).Type as INamedTypeSymbol ;
            var typeArgs = from typeArg in eventSymbol.TypeArguments
                select typeArg.ToDisplayString();

            var enumerable = typeArgs as string[] ?? typeArgs.ToArray();
            var typeArgCode = $"<{string.Join(",", enumerable)}>";

            if (!enumerable.Any())
                typeArgCode = "";

            var typeName = interfaceSymbol.ToDisplayString();

                    var source = $@"
            using System;  
            
            
            namespace {namespaceName}
            {{ 
                public class {eventName} : Regulus.Remote.IEventProxyCreator
                {{
            
                    Type _Type;
                    string _Name;
                    
                    public {eventName}()
                    {{
                        _Name = ""{fieldName}"";
                        _Type = typeof({typeName});                   
                    
                    }}
                    Delegate Regulus.Remote.IEventProxyCreator.Create(long soul_id,int event_id,long handler_id, Regulus.Remote.InvokeEventCallabck invoke_Event)
                    {{                
                        var closure = new Regulus.Remote.GenericEventClosure{typeArgCode}(soul_id , event_id ,handler_id, invoke_Event);                
                        return new Action{typeArgCode}(closure.Run);
                    }}
                
            
                    Type Regulus.Remote.IEventProxyCreator.GetType()
                    {{
                        return _Type;
                    }}            
            
                    string Regulus.Remote.IEventProxyCreator.GetName()
                    {{
                        return _Name;
                    }}            
                }}
            }}
                        ";

                    return SyntaxFactory.ParseSyntaxTree(source, null, $"{namespaceName}.{typeName}_{eventName}.RegulusRemoteGhosts.cs",Encoding.UTF8);

          
        }

        private static string _BuildEventName(EventFieldDeclarationSyntax eventSyntax)
        {
            var eventName = string.Join("_", eventSyntax.Declaration.Variables);
            var interfaceSyntax= eventSyntax.Parent as InterfaceDeclarationSyntax;
            
            return $"{interfaceSyntax.Identifier}_{eventName}";
        }

        

        private static SyntaxTree _BuildGhost(InterfaceDeclarationSyntax interface_syntax, SemanticModel semantic_model)
        {
            INamedTypeSymbol interfaceSymbol = semantic_model.GetDeclaredSymbol(interface_syntax);
            var typeName = interfaceSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var interfaceSymbols = GetInterfaceSymbols(interfaceSymbol).ToArray();
            var fulNames = from i in interfaceSymbols select i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            var namespaceName = _BuildNamesapceName(interface_syntax, semantic_model);


            var source = $@"
namespace {namespaceName}
{{
    class C{typeName} : Regulus.Remote.IGhost , {string.Join(",", fulNames)}
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
        {_BuildMethods(interfaceSymbols, semantic_model)}
        {_BuildEvents(interfaceSymbols, semantic_model)}
        {_BuildPropertys(interfaceSymbols, semantic_model)}
    }}
}}
";

            return SyntaxFactory.ParseSyntaxTree(source, null, $"{namespaceName}.{typeName}.RegulusRemoteGhosts.cs", Encoding.UTF8);
        }

        private static IEnumerable<INamedTypeSymbol> GetInterfaceSymbols(INamedTypeSymbol interfaceSymbol)
        {
            foreach(var i in interfaceSymbol.Interfaces)
            {
                foreach(var i2 in GetInterfaceSymbols(i))
                {
                    yield return i2;
                }                
            }
            yield return interfaceSymbol;
        }

        private static string _BuildPropertys(IEnumerable<INamedTypeSymbol> interface_symbols, SemanticModel semantic_model)
        {
            return string.Join("\r\n",
                _SelectMembers<IPropertySymbol>(interface_symbols).Select(m => _BuildProperty(m, semantic_model)));

        }



        private static string _BuildProperty(IPropertySymbol symbol, SemanticModel model)
        {

            var fieldName = symbol.ToDisplayString().Replace('.', '_');
            var t = symbol.Type as INamedTypeSymbol;
            var source =
                $@"
public {t.ToDisplayString()} _{fieldName} = new {t.ToDisplayString()}();
{t.ToDisplayString()} {symbol.ToDisplayString()} {{ get{{ return _{fieldName};}} }}
";
            return source;
        }
      

        


        private static IEnumerable<T> _SelectMembers<T>(IEnumerable<INamedTypeSymbol> interface_symbols)
        {
            return interface_symbols.SelectMany(i => i.GetMembers()).OfType<T>();
        }
        private static string _BuildMethods(IEnumerable<INamedTypeSymbol> interface_symbols, SemanticModel semantic_model)
        {
           return string.Join("\r\n",
                _SelectMembers<IMethodSymbol>(interface_symbols).Where(m=>m.MethodKind == MethodKind.Ordinary).Select(m => _BuildMethod(m, semantic_model)));


        }

        private static string _BuildEvents(IEnumerable<INamedTypeSymbol >interface_symbols, SemanticModel semantic_model)
        {

            return string.Join("\r\n",
                _SelectMembers<IEventSymbol>(interface_symbols).Select(m => _BuildEvent(m, semantic_model)));


        }

        private static string _BuildEvent(IEventSymbol symbol, SemanticModel semanticModel)
        {
            
            
            var fieldName = symbol.ToDisplayString().Replace('.','_');
            
            var source =
                $@"
public Regulus.Remote.GhostEventHandler  _{fieldName} = new Regulus.Remote.GhostEventHandler();
event {symbol.Type.ToDisplayString()} {symbol.ToDisplayString()}
{{
    add 
    {{
        var id = _{fieldName}.Add(value);
        _AddEventEvent(typeof({symbol.ContainingType.ToDisplayString()}).GetEvent(""{symbol.Name}""),id);
    }}
    remove
    {{
        var id = _{fieldName}.Remove(value);
        _RemoveEventEvent(typeof({symbol.ContainingType.ToDisplayString()}).GetEvent(""{symbol.Name}""),id);
    }}
}}
";
            return source;
        }

        private static string _BuildMethod(IMethodSymbol symbol, SemanticModel semantic_model)
        {
            int idx = 0;
            var paramsCode = string.Join(",", symbol.Parameters.Select(symbolParameter => $"{symbolParameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} _{idx++}"));
            idx = 0;
            var methodCallParamsCode = string.Join(",", symbol.Parameters.Select(symbolParameter => $"_{idx++}"));

            var methodCode = symbol.Name;
            var interfaceCode = symbol.ContainingType.ToDisplayString();

            string retValue = "";
            string retRetValue = "";
            string retRetValueVar = "null";
            string retCode = "void";

            if (symbol.ReturnType.SpecialType == SpecialType.System_Void)
            {
                retValue = "";
                retRetValue = "";
                retRetValueVar = "null";
                retCode = "void";
            }
            else if (semantic_model.Compilation.GetTypeByMetadataName("Regulus.Remote.Value`1") == symbol.ReturnType.OriginalDefinition)
            {
                retValue = $"var returnValue = new {symbol.ReturnType}();";
                retRetValue = "return returnValue ;";
                retRetValueVar = "returnValue";
                retCode = symbol.ReturnType.ToDisplayString();
            }
            else
            {
                retValue = $"";
                retRetValue = "throw new NotSupportedException() ;";
                retRetValueVar = "null";
                retCode = symbol.ReturnType.ToDisplayString();
            }
            var code = $@"
{retCode} {interfaceCode}.{methodCode}({paramsCode})
{{
    {retValue}
     var info = typeof({interfaceCode}).GetMethod(""{methodCode}"");
    _CallMethodEvent(info , new object[] {{{methodCallParamsCode}}} , {retRetValueVar});                    
    {retRetValue}
}}
";
            return code;
        }


        private static string _BuildNamesapceName(InterfaceDeclarationSyntax interface_syntax, SemanticModel semantic_model)
        {
            var namespaceSyntax = interface_syntax.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceSyntax == null)
                return "RegulusRemoteGhosts";
            var namespaceSymbol = semantic_model.GetDeclaredSymbol(namespaceSyntax);
            var namespaceName = namespaceSymbol.ToDisplayString();
            return namespaceName + ".RegulusRemoteGhosts";
        }
    }

}