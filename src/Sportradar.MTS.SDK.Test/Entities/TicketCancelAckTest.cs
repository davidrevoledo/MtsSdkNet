﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.MTS.SDK.Entities.Enums;
using Sportradar.MTS.SDK.Entities.Internal.Builders;
using Sportradar.MTS.SDK.Entities.Internal.TicketImpl;
using SR = Sportradar.MTS.SDK.Test.Helpers.StaticRandom;

namespace Sportradar.MTS.SDK.Test.Entities
{
    [TestClass]
    public class TicketCancelAckTest
    {
        [TestMethod]
        public void BuildTicketCancelAckTest()
        {
            var ticket = new TicketCancelAck("ticket-" + SR.I1000P, SR.I1000, TicketCancelAckStatus.Cancelled, 100, "message");

            Assert.IsNotNull(ticket);
            Assert.IsTrue(ticket.Timestamp > DateTime.Today);
            Assert.AreEqual(ticket.Version, "2.0");
            Assert.IsTrue(!string.IsNullOrEmpty(ticket.TicketId));
        }

        [TestMethod]
        public void BuildTicketAckFromTicketTest()
        {
            var ticket = TicketCancelBuilder.Create().SetTicketId("ticket-" + SR.I1000P).SetBookmakerId(SR.I1000).SetCode(TicketCancellationReason.BookmakerBackofficeTriggered).BuildTicket();

            var ticketAck = new TicketCancelAck(ticket, TicketCancelAckStatus.Cancelled, 100, "message");

            Assert.IsNotNull(ticketAck);
            Assert.IsTrue(ticketAck.Timestamp > DateTime.Today.ToUniversalTime());
            Assert.AreEqual(ticketAck.Version, "2.0");
            Assert.IsTrue(!string.IsNullOrEmpty(ticketAck.TicketId));
            Assert.AreEqual(ticketAck.TicketId, ticket.TicketId);
            Assert.AreEqual(ticket.BookmakerId, ticketAck.BookmakerId);
        }

        [TestMethod]
        public void BuildTicketCancelAckAcceptanceTest()
        {
            var ticket = new TicketCancelAck("ticket-" + SR.I1000P, SR.I1000, TicketCancelAckStatus.NotCancelled, 100, "message");

            Assert.IsNotNull(ticket);
            Assert.AreEqual(ticket.TicketCancelStatus, TicketCancelAckStatus.NotCancelled);
        }
    }
}