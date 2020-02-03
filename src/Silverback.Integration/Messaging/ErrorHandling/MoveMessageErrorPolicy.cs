﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Messages;
using Silverback.Util;

namespace Silverback.Messaging.ErrorHandling
{
    /// <summary>
    ///     This policy moves the failed messages to the configured endpoint.
    /// </summary>
    public class MoveMessageErrorPolicy : ErrorPolicyBase
    {
        private readonly IProducer _producer;
        private readonly IEndpoint _endpoint;
        private readonly ILogger _logger;
        private readonly MessageLogger _messageLogger;

        private Func<object, Exception, object> _transformationFunction;
        private Func<MessageHeaderCollection, Exception, MessageHeaderCollection> _headersTransformationFunction;

        public MoveMessageErrorPolicy(
            IBroker broker,
            IProducerEndpoint endpoint,
            IServiceProvider serviceProvider,
            ILogger<MoveMessageErrorPolicy> logger,
            MessageLogger messageLogger)
            : base(serviceProvider, logger, messageLogger)
        {
            if (broker == null) throw new ArgumentNullException(nameof(broker));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            _producer = broker.GetProducer(endpoint);
            _endpoint = endpoint;
            _logger = logger;
            _messageLogger = messageLogger;
        }

        public MoveMessageErrorPolicy Transform(
            Func<object, Exception, object> transformationFunction,
            Func<MessageHeaderCollection, Exception, MessageHeaderCollection> headersTransformationFunction = null)
        {
            _transformationFunction = transformationFunction;
            _headersTransformationFunction = headersTransformationFunction;
            return this;
        }

        protected override ErrorAction ApplyPolicy(IReadOnlyCollection<IInboundEnvelope> envelopes, Exception exception)
        {
            _messageLogger.LogInformation(_logger,
                $"{envelopes.Count} message(s) will be  be moved to endpoint '{_endpoint.Name}'.", envelopes);

            envelopes.ForEach(envelope => PublishToNewEndpoint(envelope, exception));

            return ErrorAction.Skip;
        }

        private void PublishToNewEndpoint(IInboundEnvelope envelope, Exception exception)
        {
            envelope.Headers.AddOrReplace(MessageHeader.SourceEndpointKey, envelope.Endpoint?.Name);

            _producer.Produce(
                _transformationFunction?.Invoke(envelope.Message, exception) ?? envelope.Message ?? envelope.RawMessage,
                _headersTransformationFunction?.Invoke(envelope.Headers, exception) ?? envelope.Headers);
        }
    }
}