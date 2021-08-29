using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    internal class ProtocolBuilder
    {
        internal readonly string Name;

      

        public ProtocolBuilder(Compilation compilation, IReadOnlyCollection<GhostBuilder> ghosts)
        {
            
            /*System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            var nameCode = ghosts.Aggregate("", (s, g) => s + g.Syntax.ToString());
            var code = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(nameCode));
            Name = "C"+_BuildProtocolName(code);
            var assemblyName = compilation.Assembly.ToDisplayString();
            
            var types = from g in ghosts
                        select $"types.Add(typeof({g.Common}), typeof({g.Protocol}))";

            var events = from g in ghosts
                        from e in g.Events
                        select $"eventClosures.Add(new {e}())";

            var source = 
$@"
public class {Name} : Regulus.Remote.IProtocol
{{
    readonly Regulus.Remote.InterfaceProvider _InterfaceProvider;
    readonly Regulus.Remote.EventProvider _EventProvider;
    readonly Regulus.Remote.MemberMap _MemberMap;
    readonly Regulus.Serialization.ISerializer _Serializer;
    readonly System.Reflection.Assembly _Base;

    public {Name}()
    {{
        _Base = System.Reflection.Assembly.Load(""{assemblyName}"");
        var types = new Dictionary<Type, Type>();
        {string.Join("\r\n" , types)}     
        _InterfaceProvider = new Regulus.Remote.InterfaceProvider(types);
        var eventClosures = new List<Regulus.Remote.IEventProxyCreator>();
        {string.Join("\r\n", events)}     
        _EventProvider = new Regulus.Remote.EventProvider(eventClosures);
        _Serializer = new Regulus.Serialization.Serializer(new Regulus.Serialization.DescriberBuilder(typeof(Regulus.Projects.TestProtocol.Common.MultipleNotices.MultipleNotices),typeof(Regulus.Projects.TestProtocol.Common.Number),typeof(Regulus.Projects.TestProtocol.Common.Sample),typeof(Regulus.Remote.ClientToServerOpCode),typeof(Regulus.Remote.PackageAddEvent),typeof(Regulus.Remote.PackageCallMethod),typeof(Regulus.Remote.PackageErrorMethod),typeof(Regulus.Remote.PackageInvokeEvent),typeof(Regulus.Remote.PackageLoadSoul),typeof(Regulus.Remote.PackageLoadSoulCompile),typeof(Regulus.Remote.PackagePropertySoul),typeof(Regulus.Remote.PackageProtocolSubmit),typeof(Regulus.Remote.PackageRelease),typeof(Regulus.Remote.PackageRemoveEvent),typeof(Regulus.Remote.PackageReturnValue),typeof(Regulus.Remote.PackageSetProperty),typeof(Regulus.Remote.PackageSetPropertyDone),typeof(Regulus.Remote.PackageUnloadSoul),typeof(Regulus.Remote.RequestPackage),typeof(Regulus.Remote.ResponsePackage),typeof(Regulus.Remote.ServerToClientOpCode),typeof(System.Boolean),typeof(System.Byte[]),typeof(System.Byte[][]),typeof(System.Char),typeof(System.Char[]),typeof(System.Int32),typeof(System.Int64),typeof(System.String)).Describers);


    }}
}}
";*/

        }
        private string _BuildProtocolName(byte[] code)
        {
            return $"C{BitConverter.ToString(code).Replace("-", "")}";
        }
        internal string Build()
        {
            throw new NotImplementedException();
        }
    }
}