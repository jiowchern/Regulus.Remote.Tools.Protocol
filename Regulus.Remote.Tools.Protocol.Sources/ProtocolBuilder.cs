using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    internal class ProtocolBuilder
    {

        public readonly SyntaxTree Tree;
       

        public ProtocolBuilder(Compilation compilation)
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