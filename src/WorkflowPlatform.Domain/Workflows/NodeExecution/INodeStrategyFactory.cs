using System.Threading;
using System.Threading.Tasks;

namespace WorkflowPlatform.Domain.Workflows.NodeExecution
{
    /// <summary>
    /// Factory interface for creating node execution strategies
    /// </summary>
    public interface INodeStrategyFactory
    {
        /// <summary>
        /// Get a strategy for the specified node type
        /// </summary>
        INodeExecutionStrategy? GetStrategy(string nodeType);

        /// <summary>
        /// Check if a strategy is available for the specified node type
        /// </summary>
        bool SupportsNodeType(string nodeType);

        /// <summary>
        /// Get all supported node types
        /// </summary>
        string[] GetSupportedNodeTypes();
    }
}
