﻿using System;
using System.Collections.Generic;
using System.Text;
using Arcus.Messaging.Pumps.Abstractions.MessageHandling;
using Arcus.Messaging.Tests.Unit.Fixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Arcus.Messaging.Tests.Unit.Pumps.ServiceBus.Extensions
{
    public class MessageIServiceCollectionExtensionsTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithServiceBusMessageHandler_WithMessageBodyFilter_UsesFilter(bool matchesBody)
        {
            // Arrange
            var services = new ServiceCollection();
            var expectedMessage = new TestMessage();
            var expectedHandler = new TestServiceBusMessageHandler();

            // Act
            services.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                messageBodyFilter: body =>
                {
                    Assert.Same(expectedMessage, body);
                    return matchesBody;
                });

            // Assert
            IServiceProvider provider = services.BuildServiceProvider();
            IEnumerable<MessageHandler> handlers = MessageHandler.SubtractFrom(provider, NullLogger.Instance);
            Assert.NotNull(handlers);
            MessageHandler handler = Assert.Single(handlers);
            Assert.NotNull(handler);
            Assert.NotSame(expectedHandler, handler);
            Assert.Equal(matchesBody, handler.CanProcessMessageBasedOnMessage(expectedMessage));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithMessageBodyFilter_Fails()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => services.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    messageBodyFilter: null));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithServiceBusMessageHandler_WithMessageBodyFilterWithImplementationFactory_UsesFilter(bool matchesBody)
        {
            // Arrange
            var services = new ServiceCollection();
            var expectedMessage = new TestMessage();
            var expectedHandler = new TestServiceBusMessageHandler();

            // Act
            services.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                messageBodyFilter: body =>
                {
                    Assert.Same(expectedMessage, body);
                    return matchesBody;
                },
                implementationFactory: serviceProvider => expectedHandler);

            // Assert
            IServiceProvider provider = services.BuildServiceProvider();
            IEnumerable<MessageHandler> handlers = MessageHandler.SubtractFrom(provider, NullLogger.Instance);
            Assert.NotNull(handlers);
            MessageHandler handler = Assert.Single(handlers);
            Assert.NotNull(handler);
            Assert.Same(expectedHandler, handler.GetMessageHandlerInstance());
            Assert.Equal(matchesBody, handler.CanProcessMessageBasedOnMessage(expectedMessage));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithoutMessageBodyFilterWithImplementationFactory_Fails()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => services.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    messageBodyFilter: null,
                    implementationFactory: serviceProvider => new TestServiceBusMessageHandler()));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithMessageBodyFilterWithoutImplementationFactory_Fails()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => services.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    messageBodyFilter: body => true,
                    implementationFactory: null));
        }
    }
}
