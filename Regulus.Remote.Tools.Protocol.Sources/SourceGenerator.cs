using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Regulus.Remote.Tools.Protocol.Sources
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        void ISourceGenerator.Execute(GeneratorExecutionContext context)
        {
           
            


            var builder = new GhostBuilder(context.Compilation);
            
            foreach (var g in builder.Ghosts)
            {
                context.AddSource(g.FilePath, g.ToNormalizeWhitespace());
            }

            var protocol = new ProtocolBuilder(context.Compilation).Tree;

           // context.AddSource(protocol.FilePath, protocol.ToNormalizeWhitespace());
            /*var builder = new ProtocolBuilder(context.Compilation ,receiver.Ghosts);
            context.AddSource(builder.Name, builder.Build());
            foreach (var p in receiver.Protocols)
            {

            }*/
        }

        

        void ISourceGenerator.Initialize(GeneratorInitializationContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            //context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
          
            
        }
    }
}
