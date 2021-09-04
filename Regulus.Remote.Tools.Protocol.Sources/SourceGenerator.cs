using Microsoft.CodeAnalysis;
using System;

namespace Regulus.Remote.Tools.Protocol.Sources
{
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
