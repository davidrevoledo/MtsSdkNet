﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System;
using System.Diagnostics.Contracts;
using Sportradar.MTS.SDK.Entities.Enums;
using Sportradar.MTS.SDK.Entities.Interfaces;

namespace Sportradar.MTS.SDK.Entities.Internal.TicketImpl
{
    /// <summary>
    /// Implementation of <see cref="ITicketCancel"/>
    /// </summary>
    /// <seealso cref="ITicketCancel" />
    [Serializable]
    public class TicketCancel : ITicketCancel
    {
        /// <summary>
        /// Gets the timestamp of ticket placement (UTC)
        /// </summary>
        /// <value>The timestamp</value>
        public DateTime Timestamp { get; }
        /// <summary>
        /// Gets the ticket id
        /// </summary>
        /// <value>Unique ticket id (in the client's system)</value>
        public string TicketId { get; }
        /// <summary>
        /// Get the bookmaker id (client's id provided by Sportradar)
        /// </summary>
        /// <value>The bookmaker identifier</value>
        public int BookmakerId { get; }
        /// <summary>
        /// Gets the cancellation code
        /// </summary>
        /// <value>The code</value>
        public TicketCancellationReason Code { get; }
        /// <summary>
        /// Gets the ticket format version
        /// </summary>
        /// <value>The version</value>
        public string Version { get; }

        /// <summary>
        /// Gets the correlation identifier
        /// </summary>
        /// <value>The correlation identifier</value>
        /// <remarks>Only used to relate ticket with its response</remarks>
        public string CorrelationId { get; }

        public string ToJson()
        {
            var dto = EntitiesMapper.Map(this);
            return dto.Cancel.ToJson();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketCancel"/> class
        /// </summary>
        /// <param name="ticketId">The ticket identifier</param>
        /// <param name="bookmakerId">The bookmaker identifier</param>
        /// <param name="code">The code</param>
        public TicketCancel(string ticketId, int bookmakerId, TicketCancellationReason code)
        {
            Contract.Requires(!string.IsNullOrEmpty(ticketId));
            Contract.Requires(bookmakerId > 0);

            TicketId = ticketId;
            BookmakerId = bookmakerId;
            Code = code;
            Timestamp = DateTime.UtcNow;
            Version = TicketHelper.Version;
            CorrelationId = TicketHelper.GenerateTicketCorrelationId();
        }

        /// <summary>
        /// Defines invariant members of the class
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(TicketId));
            Contract.Invariant(BookmakerId > 0);
            Contract.Invariant(!string.IsNullOrEmpty(Version));
            Contract.Invariant(Timestamp > DateTime.MinValue);
        }
    }
}