using Microsoft.CodeAnalysis;
using System;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    internal class ProtocolBuilder
    {

        public readonly SyntaxTree Tree;
       
        
        public ProtocolBuilder(Compilation compilation,GhostBuilder ghost_builder,SerializableExtractor extractor)
        {
            
                      
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