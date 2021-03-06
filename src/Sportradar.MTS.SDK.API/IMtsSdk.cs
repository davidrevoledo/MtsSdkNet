﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System;
using System.Diagnostics.Contracts;
using Sportradar.MTS.SDK.API.Contracts;
using Sportradar.MTS.SDK.Common;
using Sportradar.MTS.SDK.Entities.Builders;
using Sportradar.MTS.SDK.Entities.EventArguments;
using Sportradar.MTS.SDK.Entities.Interfaces;

namespace Sportradar.MTS.SDK.API
{
    /// <summary>
    /// Represents a root object of the MTS SDK (to create, send tickets and to handle responses)
    /// </summary>
    [ContractClass(typeof(MtsSdkContract))]
    public interface IMtsSdk : IOpenable, IDisposable
    {
        /// <summary>
        /// Raised when the current instance of <see cref="IMtsSdk"/> received <see cref="ITicketResponse"/>
        /// </summary>
        event EventHandler<TicketResponseReceivedEventArgs> TicketResponseReceived;

        /// <summary>
        /// Raised when the current instance of <see cref="IMtsSdk"/> did not receive <see cref="ITicketResponse"/> within timeout
        /// </summary>
        event EventHandler<TicketMessageEventArgs> TicketResponseTimedOut;

        /// <summary>
        /// Raised when the attempt to send ticket failed
        /// </summary>
        event EventHandler<TicketSendFailedEventArgs> SendTicketFailed;

        /// <summary>
        /// Raised when a message which cannot be parsed is received
        /// </summary>
        event EventHandler<UnparsableMessageEventArgs> UnparsableTicketResponseReceived;

        /// <summary>
        /// Gets the <see cref="IBuilderFactory"/> instance used to construct builders with some
        /// of the properties pre-loaded from the configuration
        /// </summary>
        /// <value>The builder factory</value>
        IBuilderFactory BuilderFactory { get; }

        /// <summary>
        /// Sends the ticket to the MTS server. The response will raise TicketResponseReceived event.
        /// </summary>
        /// <param name="ticket">A <see cref="ISdkTicket"/> to be send</param>
        void SendTicket(ISdkTicket ticket);

        /// <summary>
        /// Sends the ticket to the MTS server and wait for the response message on the feed
        /// </summary>
        /// <param name="ticket">A <see cref="ITicket"/> to be send</param>
        /// <returns>Returns a <see cref="ITicketResponse"/></returns>
        ITicketResponse SendTicketBlocking(ITicket ticket);

        /// <summary>
        /// Sends the cancel ticket to the MTS server and wait for the response message on the feed
        /// </summary>
        /// <param name="ticket">A <see cref="ITicketCancel"/> to be send</param>
        /// <returns>Returns a <see cref="ITicketCancelResponse"/></returns>
        ITicketCancelResponse SendTicketCancelBlocking(ITicketCancel ticket);

        /// <summary>
        /// Sends the cashout ticket to the MTS server and wait for the response message on the feed
        /// </summary>
        /// <param name="ticket">A <see cref="ITicketCashout"/> to be send</param>
        /// <returns>Returns a <see cref="ITicketCashoutResponse"/></returns>
        ITicketCashoutResponse SendTicketCashoutBlocking(ITicketCashout ticket);
    }
}
