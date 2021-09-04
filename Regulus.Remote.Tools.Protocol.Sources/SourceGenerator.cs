﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regulus.Remote.Tools.Protocol.Sources
{

    public class ProjectSourceBuilder
    {
        public readonly IEnumerable<SyntaxTree> Sources;
        public ProjectSourceBuilder(Compilation compilation)
        {
            var builder = new GhostBuilder(compilation);

            SerializableExtractor extractor = new SerializableExtractor(compilation);
            EventProviderCodeBuilder event_provider_code_builder = new EventProviderCodeBuilder(builder.Events);

            InterfaceProviderCodeBuilder interface_provider_code_builder = new InterfaceProviderCodeBuilder(builder.Ghosts);
            MemberMapCodeBuilder membermap_code_builder = new MemberMapCodeBuilder(compilation);
            var protocol = new ProtocolBuilder(compilation, extractor, event_provider_code_builder, interface_provider_code_builder, membermap_code_builder).Tree;

            Sources = builder.Ghosts.Union(builder.Events).Union(new[] {protocol});
        }
    }
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        void ISourceGenerator.Execute(GeneratorExecutionContext context)
        {


            Compilation compilation = context.Compilation;
            var sources = new ProjectSourceBuilder(compilation).Sources;
            
            foreach (var syntaxTree in sources)
            {
                context.AddSource(syntaxTree.FilePath, syntaxTree.GetText());
            }
           
            
        }

        

        void ISourceGenerator.Initialize(GeneratorInitializationContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            //context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
          
            
        }
    }
}
