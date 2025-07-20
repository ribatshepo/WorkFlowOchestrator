using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Application.Workflows.NodeExecution;
using WorkflowPlatform.Domain.Workflows.NodeExecution;
using Xunit;

namespace WorkflowPlatform.Tests.Unit.Application.NodeExecution
{
    public class NodeExecutionEngineTests
    {
        private readonly Mock<INodeStrategyFactory> _mockStrategyFactory;
        private readonly Mock<ILogger<NodeExecutionEngine>> _mockLogger;
        private readonly Mock<IMetricsCollector> _mockMetrics;
        private readonly Mock<INodeExecutionStrategy> _mockStrategy;
        private readonly NodeExecutionEngine _engine;

        public NodeExecutionEngineTests()
        {
            _mockStrategyFactory = new Mock<INodeStrategyFactory>();
            _mockLogger = new Mock<ILogger<NodeExecutionEngine>>();
            _mockMetrics = new Mock<IMetricsCollector>();
            _mockStrategy = new Mock<INodeExecutionStrategy>();

            _engine = new NodeExecutionEngine(
                _mockStrategyFactory.Object,
                _mockLogger.Object,
                _mockMetrics.Object);
        }

        [Fact]
        public async Task ExecuteNodeAsync_WithValidStrategy_Should_ExecuteAllPhases()
        {
            // Arrange
            var context = CreateValidExecutionContext();
            var expectedResult = NodeExecutionResult.Success("test output");

            _mockStrategyFactory
                .Setup(f => f.GetStrategy("TestNode"))
                .Returns(_mockStrategy.Object);

            _mockStrategy
                .Setup(s => s.PreprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(NodeExecutionResult.Success());

            _mockStrategy
                .Setup(s => s.ExecuteAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            _mockStrategy
                .Setup(s => s.PostprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            _mockStrategy
                .Setup(s => s.FinalizationAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _engine.ExecuteNodeAsync(context, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.OutputData.Should().Be("test output");

            // Verify all phases were called
            _mockStrategy.Verify(s => s.PreprocessAsync(context, It.IsAny<CancellationToken>()), Times.Once);
            _mockStrategy.Verify(s => s.ExecuteAsync(context, It.IsAny<CancellationToken>()), Times.Once);
            _mockStrategy.Verify(s => s.PostprocessAsync(context, It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockStrategy.Verify(s => s.FinalizationAsync(context, It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()), Times.Once);

            // Verify metrics were recorded
            _mockMetrics.Verify(m => m.IncrementNodeExecutionAttempts("TestNode"), Times.Once);
            _mockMetrics.Verify(m => m.RecordNodeExecution("TestNode", NodeExecutionStatus.Completed, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteNodeAsync_WithUnknownNodeType_Should_ReturnFailure()
        {
            // Arrange
            var context = CreateValidExecutionContext();

            _mockStrategyFactory
                .Setup(f => f.GetStrategy("TestNode"))
                .Returns((INodeExecutionStrategy?)null);

            // Act
            var result = await _engine.ExecuteNodeAsync(context, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("No strategy found for node type 'TestNode'");
        }

        [Fact]
        public async Task ExecuteNodeAsync_WithPreprocessingFailure_Should_StopExecution()
        {
            // Arrange
            var context = CreateValidExecutionContext();
            var preprocessingFailure = NodeExecutionResult.Failed("Preprocessing failed");

            _mockStrategyFactory
                .Setup(f => f.GetStrategy("TestNode"))
                .Returns(_mockStrategy.Object);

            _mockStrategy
                .Setup(s => s.PreprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(preprocessingFailure);

            _mockStrategy
                .Setup(s => s.FinalizationAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _engine.ExecuteNodeAsync(context, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Preprocessing failed");

            // Verify only preprocessing and finalization were called
            _mockStrategy.Verify(s => s.PreprocessAsync(context, It.IsAny<CancellationToken>()), Times.Once);
            _mockStrategy.Verify(s => s.ExecuteAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockStrategy.Verify(s => s.PostprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockStrategy.Verify(s => s.FinalizationAsync(context, It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteNodeAsync_WithExecutionFailure_Should_ContinueToPostprocessing()
        {
            // Arrange
            var context = CreateValidExecutionContext();
            var executionFailure = NodeExecutionResult.Failed("Execution failed");

            _mockStrategyFactory
                .Setup(f => f.GetStrategy("TestNode"))
                .Returns(_mockStrategy.Object);

            _mockStrategy
                .Setup(s => s.PreprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(NodeExecutionResult.Success());

            _mockStrategy
                .Setup(s => s.ExecuteAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(executionFailure);

            _mockStrategy
                .Setup(s => s.PostprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(executionFailure);

            _mockStrategy
                .Setup(s => s.FinalizationAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _engine.ExecuteNodeAsync(context, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Execution failed");

            // Verify all phases were called
            _mockStrategy.Verify(s => s.PreprocessAsync(context, It.IsAny<CancellationToken>()), Times.Once);
            _mockStrategy.Verify(s => s.ExecuteAsync(context, It.IsAny<CancellationToken>()), Times.Once);
            _mockStrategy.Verify(s => s.PostprocessAsync(context, It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockStrategy.Verify(s => s.FinalizationAsync(context, It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteNodeAsync_WithCancellation_Should_ReturnCancelled()
        {
            // Arrange
            var context = CreateValidExecutionContext();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            _mockStrategyFactory
                .Setup(f => f.GetStrategy("TestNode"))
                .Returns(_mockStrategy.Object);

            // Act
            var result = await _engine.ExecuteNodeAsync(context, cancellationTokenSource.Token);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(NodeExecutionStatus.Cancelled);
        }

        [Fact]
        public async Task ExecuteNodeWithRetryAsync_WithSuccess_Should_ReturnImmediately()
        {
            // Arrange
            var context = CreateValidExecutionContext();
            var expectedResult = NodeExecutionResult.Success("test output");

            _mockStrategyFactory
                .Setup(f => f.GetStrategy("TestNode"))
                .Returns(_mockStrategy.Object);

            SetupSuccessfulExecution(expectedResult);

            // Act
            var result = await _engine.ExecuteNodeWithRetryAsync(context, maxRetries: 3);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.OutputData.Should().Be("test output");

            // Should only be called once (no retries needed)
            _mockStrategy.Verify(s => s.PreprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteNodeWithRetryAsync_WithTransientFailure_Should_RetryAndSucceed()
        {
            // Arrange
            var context = CreateValidExecutionContext();
            var failure = NodeExecutionResult.Failed(new TimeoutException("Timeout"));
            var success = NodeExecutionResult.Success("test output");

            _mockStrategyFactory
                .Setup(f => f.GetStrategy("TestNode"))
                .Returns(_mockStrategy.Object);

            var executionCount = 0;
            _mockStrategy
                .Setup(s => s.PreprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(NodeExecutionResult.Success());

            _mockStrategy
                .Setup(s => s.ExecuteAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => ++executionCount == 1 ? failure : success);

            _mockStrategy
                .Setup(s => s.PostprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync<NodeExecutionContext, NodeExecutionResult, CancellationToken>((ctx, res, ct) => res);

            _mockStrategy
                .Setup(s => s.FinalizationAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _engine.ExecuteNodeWithRetryAsync(context, maxRetries: 3, retryDelay: TimeSpan.FromMilliseconds(10));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.OutputData.Should().Be("test output");

            // Should be called twice (initial attempt + 1 retry)
            _mockStrategy.Verify(s => s.ExecuteAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _mockMetrics.Verify(m => m.RecordRetryAttempt("TestNode", 1), Times.Once);
        }

        private NodeExecutionContext CreateValidExecutionContext()
        {
            return new NodeExecutionContext(
                Guid.NewGuid(),
                "TestNode",
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                new Dictionary<string, object>());
        }

        private void SetupSuccessfulExecution(NodeExecutionResult expectedResult)
        {
            _mockStrategy
                .Setup(s => s.PreprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(NodeExecutionResult.Success());

            _mockStrategy
                .Setup(s => s.ExecuteAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            _mockStrategy
                .Setup(s => s.PostprocessAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            _mockStrategy
                .Setup(s => s.FinalizationAsync(It.IsAny<NodeExecutionContext>(), It.IsAny<NodeExecutionResult>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
    }
}
