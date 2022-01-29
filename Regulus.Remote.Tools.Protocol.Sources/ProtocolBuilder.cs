﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    internal class ProtocolBuilder
    {

        public readonly SyntaxTree Tree;
       
        
        public ProtocolBuilder(
            Compilation compilation,
            SerializableExtractor extractor,
            EventProviderCodeBuilder event_provider_code_builder,
            InterfaceProviderCodeBuilder interface_provider_code_builder,
            MemberMapCodeBuilder membermap_code_builder)
        {

            /*
            "typeof(Regulus.Remote.PackageProtocolSubmit)",
            "typeof(Regulus.Remote.RequestPackage)",
            "typeof(Regulus.Remote.ResponsePackage)",
            "typeof(Regulus.Remote.PackageInvokeEvent)",
            "typeof(Regulus.Remote.PackageErrorMethod)",
            "typeof(Regulus.Remote.PackageReturnValue)",
            "typeof(Regulus.Remote.PackageLoadSoulCompile)",
            "typeof(Regulus.Remote.PackageLoadSoul)",
            "typeof(Regulus.Remote.PackageUnloadSoul)",
            "typeof(Regulus.Remote.PackageCallMethod)",
            "typeof(Regulus.Remote.PackageRelease)",
            "typeof(Regulus.Remote.PackageSetProperty)",
            "typeof(Regulus.Remote.PackageSetPropertyDone)",
            "typeof(Regulus.Remote.PackageAddEvent)",
            "typeof(Regulus.Remote.PackageRemoveEvent)",
            "typeof(Regulus.Remote.PackagePropertySoul)",   
             */


            var essentialTypes = new string[]
            {//typeof(Regulus.Remote.ClientToServerOpCode),typeof(Regulus.Remote.PackageAddEvent),typeof(Regulus.Remote.PackageCallMethod),typeof(Regulus.Remote.PackageErrorMethod),typeof(Regulus.Remote.PackageInvokeEvent),typeof(Regulus.Remote.PackageLoadSoul),typeof(Regulus.Remote.PackageLoadSoulCompile),typeof(Regulus.Remote.PackagePropertySoul),typeof(Regulus.Remote.PackageProtocolSubmit),typeof(Regulus.Remote.PackageRelease),typeof(Regulus.Remote.PackageRemoveEvent),typeof(Regulus.Remote.PackageReturnValue),typeof(Regulus.Remote.PackageSetProperty),typeof(Regulus.Remote.PackageSetPropertyDone),typeof(Regulus.Remote.PackageUnloadSoul),typeof(Regulus.Remote.RequestPackage),typeof(Regulus.Remote.ResponsePackage),typeof(Regulus.Remote.ServerToClientOpCode),typeof(System.Boolean),typeof(System.Byte[]),typeof(System.Byte[][]),typeof(System.Char),typeof(System.Char[]),typeof(System.Int32),typeof(System.Int64),typeof(System.String)
                "typeof(Regulus.Remote.PackageProtocolSubmit)",
                "typeof(Regulus.Remote.RequestPackage)",
                "typeof(Regulus.Remote.ResponsePackage)",
                "typeof(Regulus.Remote.PackageInvokeEvent)",
                "typeof(Regulus.Remote.PackageErrorMethod)",
                "typeof(Regulus.Remote.PackageReturnValue)",
                "typeof(Regulus.Remote.PackageLoadSoulCompile)",
                "typeof(Regulus.Remote.PackageLoadSoul)",
                "typeof(Regulus.Remote.PackageUnloadSoul)",
                "typeof(Regulus.Remote.PackageCallMethod)",
                "typeof(Regulus.Remote.PackageRelease)",
                "typeof(Regulus.Remote.PackageSetProperty)",
                "typeof(Regulus.Remote.PackageSetPropertyDone)",
                "typeof(Regulus.Remote.PackageAddEvent)",
                "typeof(Regulus.Remote.PackageRemoveEvent)",
                "typeof(Regulus.Remote.PackagePropertySoul)",
                "typeof(byte)",
                "typeof(byte[])",
                "typeof(byte[][])",
                "typeof(Regulus.Remote.ClientToServerOpCode)",
                "typeof(Regulus.Remote.ServerToClientOpCode)",
                "typeof(long)",
                "typeof(int)",
                "typeof(string)",
                "typeof(bool)",
                "typeof(char)",
                "typeof(char[])"


            };
            var types = essentialTypes.Union(extractor.Symbols.Select(s =>
                $"typeof({s.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})"));
            var serCode =string.Join(",", new HashSet<string>(types));

            var md5 = _BuildMd5(serCode + event_provider_code_builder.Code + interface_provider_code_builder.Code +membermap_code_builder.PropertyInfosCode + membermap_code_builder.EventInfosCode + membermap_code_builder.InterfacesCode + membermap_code_builder.MethodInfosCode);

            var protocolName = _BuildProtocolName(md5);
            var verCode = _BuildVerificationCode(md5);
            string code =$@"
using System;  
using System.Collections.Generic;
using Regulus.Remote;
public class {protocolName} : Regulus.Remote.IProtocol
{{
  
    readonly Regulus.Remote.InterfaceProvider _InterfaceProvider;
    readonly Regulus.Remote.EventProvider _EventProvider;
    readonly Regulus.Remote.MemberMap _MemberMap;
    readonly Regulus.Serialization.ISerializer _Serializer;
    readonly System.Reflection.Assembly _Base;
    public {protocolName}()
    {{
        _Base = System.Reflection.Assembly.Load(""{compilation.Assembly}"");
       
        _InterfaceProvider = new Regulus.Remote.InterfaceProvider(new Dictionary<Type, Type> (){{ {interface_provider_code_builder.Code}}});
   
        _EventProvider = new Regulus.Remote.EventProvider( new IEventProxyCreator[]{{ {event_provider_code_builder.Code} }});
        _Serializer = new Regulus.Serialization.Serializer(new Regulus.Serialization.DescriberBuilder({serCode}).Describers);
        _MemberMap = new Regulus.Remote.MemberMap(
            new System.Reflection.MethodInfo[] {{{membermap_code_builder.MethodInfosCode}}} ,
            new System.Reflection.EventInfo[]{{ {membermap_code_builder.EventInfosCode}}}, 
            new System.Reflection.PropertyInfo[] {{{membermap_code_builder.PropertyInfosCode}}}, 
            new System.Tuple<System.Type, System.Func<Regulus.Remote.IProvider>>[] {{{membermap_code_builder.InterfacesCode}}});
    }}
    
    System.Reflection.Assembly Regulus.Remote.IProtocol.Base =>_Base;
    byte[] Regulus.Remote.IProtocol.VerificationCode {{ get {{ return new byte[]{{{verCode}}};}} }}
    Regulus.Remote.InterfaceProvider Regulus.Remote.IProtocol.GetInterfaceProvider()
    {{
        return _InterfaceProvider;
    }}

    Regulus.Remote.EventProvider Regulus.Remote.IProtocol.GetEventProvider()
    {{
        return _EventProvider;
    }}

    Regulus.Serialization.ISerializer Regulus.Remote.IProtocol.GetSerialize()
    {{
        return _Serializer;
    }}

    Regulus.Remote.MemberMap Regulus.Remote.IProtocol.GetMemberMap()
    {{
        return _MemberMap;
    }}  
    
}}
            
";

           Tree= SyntaxFactory.ParseSyntaxTree(code, null, $"RegulusRemoteProtocol.{protocolName}.cs", Encoding.UTF8);

        }
        private string _BuildProtocolName(byte[] code)
        {
            return $"C{BitConverter.ToString(code).Replace("-", "")}";
        }
        private string _BuildVerificationCode(byte[] code)
        {
            
            return string.Join(",", code.Select(val => val.ToString()).ToArray());
        }

        private byte[] _BuildMd5(string codes)
        {
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(Encoding.ASCII.GetBytes(codes));
        }
    }
}