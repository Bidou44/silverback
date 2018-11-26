﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

// TODO: Fix

//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Abstractions;
//using NUnit.Framework;
//using Silverback.Messaging.Messages;
//using Silverback.Messaging.Publishing;
//using Silverback.Messaging.Subscribers;
//using Silverback.Tests.TestTypes;
//using Silverback.Tests.TestTypes.Messages;
//using Silverback.Tests.TestTypes.Subscribers;

//namespace Silverback.Tests.Messaging.Publishing
//{
//    [TestFixture]
//    public class CommandPublisherTests
//    {
//        private IPublisher _publisher;
//        private TestSubscriber _subscriber;

//        [SetUp]
//        public void Setup()
//        {
//            _subscriber = new TestSubscriber();
//            _publisher = new Publisher(TestServiceProvider.Create<ISubscriber>(_subscriber), NullLoggerFactory.Instance.CreateLogger<Publisher>());
//        }

//        [Test]
//        public void SendCommandTest()
//        {
//            var publisher = new CommandPublisher(_publisher);

//            publisher.Send(new TestCommandOne());
//            publisher.Send(new TestCommandTwo());

//            Assert.That(_subscriber.ReceivedMessagesCount, Is.EqualTo(2));
//        }

//        [Test]
//        public async Task SendCommandAsync()
//        {
//            var publisher = new CommandPublisher(_publisher);

//            await publisher.SendAsync(new TestCommandOne());
//            await publisher.SendAsync(new TestCommandTwo());

//            Assert.That(_subscriber.ReceivedMessagesCount, Is.EqualTo(2));
//        }

//        [Test]
//        public async Task SendSpecificCommandAsyncTest()
//        {
//            var publisher = new CommandPublisher<TestCommandTwo>(_publisher);

//            await publisher.SendAsync(new TestCommandTwo());

//            Assert.That(_subscriber.ReceivedMessagesCount, Is.EqualTo(1));
//        }
//    }
//}