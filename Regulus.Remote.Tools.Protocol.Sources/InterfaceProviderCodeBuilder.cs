﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Regulus.Remote.Tools.Protocol.Sources
{

    class MemberMapCodeBuilder
    {

    }

    class InterfaceProviderCodeBuilder
    {
        public readonly string Code;
        public InterfaceProviderCodeBuilder(IReadOnlyCollection<SyntaxTree> ghosts)
        {


            var ret = from ghost in ghosts
                from classSyntax in ghost.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                let baseSyntax = classSyntax.BaseList.Types[1]
                let namespaceSyntax = classSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().Single()
                      select $"{{typeof({baseSyntax}),typeof(global::{namespaceSyntax.Name}.{classSyntax.Identifier})}}";

            Code=string.Join(",", ret);
           
        }
    }
}