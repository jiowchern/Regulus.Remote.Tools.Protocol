using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

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
}