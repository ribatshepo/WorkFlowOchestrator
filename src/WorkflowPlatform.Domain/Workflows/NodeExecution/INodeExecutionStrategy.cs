using System.Threading;
using System.Threading.Tasks;

namespace WorkflowPlatform.Domain.Workflows.NodeExecution
{
    /// <summary>
    /// Interface for node execution strategies following the 4-phase lifecycle
    /// </summary>
    public interface INodeExecutionStrategy
    {
        /// <summary>
        /// The type of node this strategy handles
        /// </summary>
        string NodeType { get; }

        /// <summary>
        /// Phase 1: Preprocessing - Validate inputs and setup execution context
        /// </summary>
        Task<NodeExecutionResult> PreprocessAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Phase 2: Execute - Perform the core node operation
        /// </summary>
        Task<NodeExecutionResult> ExecuteAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Phase 3: Postprocessing - Transform output data and validate results
        /// </summary>
        Task<NodeExecutionResult> PostprocessAsync(
            NodeExecutionContext context, 
            NodeExecutionResult executionResult, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Phase 4: Finalization - Cleanup resources and persist state
        /// </summary>
        Task FinalizationAsync(
            NodeExecutionContext context, 
            NodeExecutionResult executionResult, 
            CancellationToken cancellationToken = default);
    }
}
