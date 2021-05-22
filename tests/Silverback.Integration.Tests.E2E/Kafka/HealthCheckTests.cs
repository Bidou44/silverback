// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Messages;
using Silverback.Tests.Integration.E2E.TestHost;
using Xunit;
using Xunit.Abstractions;

namespace Silverback.Tests.Integration.E2E.Kafka
{
    public class HealthCheckTests : KafkaTestFixture
    {
        public HealthCheckTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task ConsumerHealthCheck_AllConnectedViaSubscription_HealthyEventuallyReturned()
        {
            Host.ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .WithConnectionToMessageBroker(
                            options => options.AddMockedKafka(
                                mockedKafkaOptions =>
                                    mockedKafkaOptions.DelayPartitionsAssignment(
                                        TimeSpan.FromMilliseconds(100))))
                        .AddKafkaEndpoints(
                            endpoints => endpoints
                                .Configure(
                                    config =>
                                    {
                                        config.BootstrapServers = "PLAINTEXT://e2e";
                                    })
                                .AddOutbound<IIntegrationEvent>(
                                    endpoint => endpoint.ProduceTo(DefaultTopicName))
                                .AddInbound(
                                    endpoint => endpoint
                                        .ConsumeFrom("topic1")
                                        .Configure(
                                            config =>
                                            {
                                                config.GroupId = "consumer1";
                                            }))
                                .AddInbound(
                                    endpoint => endpoint
                                        .ConsumeFrom("topic2")
                                        .Configure(
                                            config =>
                                            {
                                                config.GroupId = "consumer1";
                                            }))
                                .AddInbound(
                                    endpoint => endpoint
                                        .ConsumeFrom("topic3")
                                        .Configure(
                                            config =>
                                            {
                                                config.GroupId = "consumer1";
                                            })))
                        .Services
                        .AddHealthChecks()
                        .AddConsumersCheck())
                .Run(waitUntilBrokerConnected: false);

            HttpResponseMessage response = await Host.HttpClient.GetAsync("/health");
            response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);

            var consumers = Helper.Broker.Consumers;
            await AsyncTestingUtil.WaitAsync(
                () => consumers.All(consumer => consumer.StatusInfo.Status > ConsumerStatus.Connected));

            response = await Host.HttpClient.GetAsync("/health");
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}