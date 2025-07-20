using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Workflows.NodeExecution
{
    /// <summary>
    /// Factory for creating node execution strategies based on node type
    /// </summary>
    public class NodeStrategyFactory : INodeStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _strategyTypes;

        public NodeStrategyFactory(IServiceProvider serviceProvider, IEnumerable<INodeExecutionStrategy> strategies)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            
            // Build a dictionary of node type to strategy type for efficient lookup
            _strategyTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var strategy in strategies)
            {
                if (!string.IsNullOrWhiteSpace(strategy.NodeType))
                {
                    _strategyTypes[strategy.NodeType] = strategy.GetType();
                }
            }
        }

        /// <summary>
        /// Get a strategy for the specified node type
        /// </summary>
        public INodeExecutionStrategy? GetStrategy(string nodeType)
        {
            if (string.IsNullOrWhiteSpace(nodeType))
                return null;

            if (!_strategyTypes.TryGetValue(nodeType, out var strategyType))
                return null;

            // Use service provider to resolve strategy instance
            // This ensures proper dependency injection
            return _serviceProvider.GetService(strategyType) as INodeExecutionStrategy;
        }

        /// <summary>
        /// Check if a strategy is available for the specified node type
        /// </summary>
        public bool SupportsNodeType(string nodeType)
        {
            return !string.IsNullOrWhiteSpace(nodeType) && 
                   _strategyTypes.ContainsKey(nodeType);
        }

        /// <summary>
        /// Get all supported node types
        /// </summary>
        public string[] GetSupportedNodeTypes()
        {
            return _strategyTypes.Keys.ToArray();
        }
    }
}
