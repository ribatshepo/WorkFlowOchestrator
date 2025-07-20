using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Application.Workflows.NodeExecution.Configurations;
using WorkflowPlatform.Application.Workflows.NodeExecution.Strategies;
using WorkflowPlatform.Domain.Workflows.NodeExecution;
using Xunit;

namespace WorkflowPlatform.Tests.Unit.Application.NodeExecution
{
    public class HttpRequestNodeStrategyTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IRetryPolicy> _mockRetryPolicy;
        private readonly Mock<ICircuitBreaker> _mockCircuitBreaker;
        private readonly Mock<ILogger<HttpRequestNodeStrategy>> _mockLogger;
        private readonly Mock<IMetricsCollector> _mockMetrics;
        private readonly HttpClient _httpClient;
        private readonly HttpRequestNodeStrategy _strategy;

        public HttpRequestNodeStrategyTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockRetryPolicy = new Mock<IRetryPolicy>();
            _mockCircuitBreaker = new Mock<ICircuitBreaker>();
            _mockLogger = new Mock<ILogger<HttpRequestNodeStrategy>>();
            _mockMetrics = new Mock<IMetricsCollector>();
            
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            
            _strategy = new HttpRequestNodeStrategy(
                _httpClient,
                _mockRetryPolicy.Object,
                _mockCircuitBreaker.Object,
                _mockLogger.Object,
                _mockMetrics.Object);
        }

        [Fact]
        public void NodeType_Should_Return_HttpRequest()
        {
            // Act
            var nodeType = _strategy.NodeType;

            // Assert
            nodeType.Should().Be("HttpRequest");
        }

        [Fact]
        public async Task ValidateInputsAsync_WithValidConfiguration_Should_ReturnSuccess()
        {
            // Arrange
            var context = CreateValidExecutionContext();

            // Act
            var result = await _strategy.ValidateInputsAsync(context, CancellationToken.None);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateInputsAsync_WithMissingUrl_Should_ReturnFailure()
        {
            // Arrange
            var config = new HttpRequestNodeConfiguration
            {
                Url = "", // Missing URL
                Method = "GET"
            };
            var context = CreateExecutionContext(config);

            // Act
            var result = await _strategy.ValidateInputsAsync(context, CancellationToken.None);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("URL is required");
        }

        [Fact]
        public async Task ValidateInputsAsync_WithInvalidUrl_Should_ReturnFailure()
        {
            // Arrange
            var config = new HttpRequestNodeConfiguration
            {
                Url = "invalid-url",
                Method = "GET"
            };
            var context = CreateExecutionContext(config);

            // Act
            var result = await _strategy.ValidateInputsAsync(context, CancellationToken.None);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("URL must be a valid absolute URI");
        }

        [Fact]
        public async Task ValidateInputsAsync_WithInvalidHttpMethod_Should_ReturnFailure()
        {
            // Arrange
            var config = new HttpRequestNodeConfiguration
            {
                Url = "https://example.com",
                Method = "INVALID"
            };
            var context = CreateExecutionContext(config);

            // Act
            var result = await _strategy.ValidateInputsAsync(context, CancellationToken.None);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("HTTP method 'INVALID' is not supported");
        }

        [Fact]
        public async Task ExecuteAsync_WithValidRequest_Should_ReturnSuccess()
        {
            // Arrange
            var context = CreateValidExecutionContext();
            var expectedResponse = "{ \"result\": \"success\" }";

            SetupHttpClientMock(HttpStatusCode.OK, expectedResponse);
            SetupRetryPolicyMock();
            SetupCircuitBreakerMock();

            // Act
            var result = await _strategy.ExecuteAsync(context, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.OutputData.Should().NotBeNull();
            _mockMetrics.Verify(m => m.RecordNodeExecution("HttpRequest", NodeExecutionStatus.Completed, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithHttpError_Should_ReturnFailure()
        {
            // Arrange
            var context = CreateValidExecutionContext();

            SetupHttpClientMock(HttpStatusCode.InternalServerError, "Server Error");
            SetupRetryPolicyMock();
            SetupCircuitBreakerMock();

            // Act
            var result = await _strategy.ExecuteAsync(context, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(NodeExecutionStatus.Failed);
            _mockMetrics.Verify(m => m.RecordNodeExecution("HttpRequest", NodeExecutionStatus.Failed, It.IsAny<TimeSpan>()), Times.Once);
            _mockMetrics.Verify(m => m.RecordNodeExecutionError("HttpRequest", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithCancellation_Should_ReturnCancelled()
        {
            // Arrange
            var context = CreateValidExecutionContext();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act
            var result = await _strategy.ExecuteAsync(context, cancellationTokenSource.Token);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Status.Should().Be(NodeExecutionStatus.Cancelled);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public async Task ExecuteAsync_WithDifferentHttpMethods_Should_Work(string httpMethod)
        {
            // Arrange
            var config = new HttpRequestNodeConfiguration
            {
                Url = "https://api.example.com/test",
                Method = httpMethod,
                Timeout = TimeSpan.FromSeconds(30)
            };
            var context = CreateExecutionContext(config);

            SetupHttpClientMock(HttpStatusCode.OK, "{ \"success\": true }");
            SetupRetryPolicyMock();
            SetupCircuitBreakerMock();

            // Act
            var result = await _strategy.ExecuteAsync(context, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        private NodeExecutionContext CreateValidExecutionContext()
        {
            var config = new HttpRequestNodeConfiguration
            {
                Url = "https://api.example.com/test",
                Method = "GET",
                Timeout = TimeSpan.FromSeconds(30),
                Headers = new Dictionary<string, string>
                {
                    { "Accept", "application/json" }
                }
            };

            return CreateExecutionContext(config);
        }

        private NodeExecutionContext CreateExecutionContext(HttpRequestNodeConfiguration config)
        {
            var configuration = new Dictionary<string, object>
            {
                { "HttpConfig", config }
            };

            return new NodeExecutionContext(
                Guid.NewGuid(),
                "HttpRequest",
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                configuration);
        }

        private void SetupHttpClientMock(HttpStatusCode statusCode, string content)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }

        private void SetupRetryPolicyMock()
        {
            _mockRetryPolicy
                .Setup(p => p.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<CancellationToken, Task<string>>, CancellationToken>((func, ct) => func(ct));
        }

        private void SetupCircuitBreakerMock()
        {
            _mockCircuitBreaker
                .Setup(cb => cb.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<CancellationToken, Task<string>>, CancellationToken>((func, ct) => func(ct));
        }
    }
}
