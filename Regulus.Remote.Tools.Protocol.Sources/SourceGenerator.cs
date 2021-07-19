using Microsoft.CodeAnalysis;
using System;


namespace Regulus.Remote.Tools.Protocol.Sources
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        void ISourceGenerator.Execute(GeneratorExecutionContext context)
        {

            var receiver = context.SyntaxContextReceiver as SyntaxReceiver;
            if (receiver == null)
                return;
            
            foreach(var g in receiver.Ghosts)
            {
                var text = g.Syntax.GetText();
                context.AddSource(g.Name, text);
            }
        }

        void ISourceGenerator.Initialize(GeneratorInitializationContext context)
        {
            System.Diagnostics.Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            
        }
    }
}
