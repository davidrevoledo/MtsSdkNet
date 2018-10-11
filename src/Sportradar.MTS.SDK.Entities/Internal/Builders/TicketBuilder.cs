﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System;
using System.Collections.Generic;
using Sportradar.MTS.SDK.Entities.Builders;
using Sportradar.MTS.SDK.Entities.Enums;
using Sportradar.MTS.SDK.Entities.Interfaces;
using Sportradar.MTS.SDK.Entities.Internal.TicketImpl;

namespace Sportradar.MTS.SDK.Entities.Internal.Builders
{
    /// <summary>
    /// Implementation of the <see cref="ITicketBuilder" />
    /// </summary>
    /// <seealso cref="ITicketBuilder" />
    public class TicketBuilder : ITicketBuilder
    {
        /// <summary>
        /// The ticket identifier
        /// </summary>
        private string _ticketId;

        /// <summary>
        /// The reoffer identifier
        /// </summary>
        private string _reofferId;

        /// <summary>
        /// The alt stake reference identifier
        /// </summary>
        private string _altStakeRefId;

        /// <summary>
        /// The is test
        /// </summary>
        private bool _isTest;

        /// <summary>
        /// The odds change type
        /// </summary>
        private OddsChangeType? _oddsChangeType;

        /// <summary>
        /// The sender
        /// </summary>
        private ISender _sender;

        /// <summary>
        /// The bets
        /// </summary>
        private IEnumerable<IBet> _bets;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketBuilder"/> class
        /// </summary>
        internal TicketBuilder()
        {
        }

        #region Obsolete_members
        /// <summary>
        /// Creates the <see cref="ITicketBuilder"/>
        /// </summary>
        /// <returns>Return a new instance of <see cref="ITicketBuilder"/></returns>
        [Obsolete("Method Create(...) is obsolete. Please use the appropriate method on IBuilderFactory interface which can be obtained through MtsSdk instance")]
        public static ITicketBuilder Create()
        {
            return new TicketBuilder();
        }
        #endregion

        /// <summary>
        /// Sets the ticket id
        /// </summary>
        /// <param name="ticketId">The ticket id</param>
        /// <returns>Returns a <see cref="ITicketBuilder" /></returns>
        public ITicketBuilder SetTicketId(string ticketId)
        {
            _ticketId = ticketId;
            return this;
        }

        /// <summary>
        /// Sets the reoffer id
        /// </summary>
        /// <param name="reofferId">The reoffer id</param>
        /// <returns>Returns a <see cref="ITicketBuilder" /></returns>
        public ITicketBuilder SetReofferId(string reofferId)
        {
            _reofferId = reofferId;
            return this;
        }

        /// <summary>
        /// Sets the alternative stake reference ticket id
        /// </summary>
        /// <param name="altStakeRefId">The alt stake reference id</param>
        /// <returns>Returns a <see cref="ITicketBuilder" /></returns>
        public ITicketBuilder SetAltStakeRefId(string altStakeRefId)
        {
            _altStakeRefId = altStakeRefId;
            return this;
        }

        /// <summary>
        /// Sets the test source
        /// </summary>
        /// <param name="isTest">if set to <c>true</c> [is test]</param>
        /// <returns>Returns a <see cref="ITicketBuilder" /></returns>
        public ITicketBuilder SetTestSource(bool isTest)
        {
            _isTest = isTest;
            return this;
        }

        /// <summary>
        /// Sets the odds change
        /// </summary>
        /// <param name="type">The <see cref="OddsChangeType" /> to be set</param>
        /// <returns>Returns a <see cref="ITicketBuilder" /></returns>
        public ITicketBuilder SetOddsChange(OddsChangeType type)
        {
            _oddsChangeType = type;
            return this;
        }

        /// <summary>
        /// Sets the sender
        /// </summary>
        /// <param name="sender">The ticket sender</param>
        /// <returns>Returns a <see cref="ITicketBuilder" /></returns>
        public ITicketBuilder SetSender(ISender sender)
        {
            _sender = sender;
            return this;
        }

        /// <summary>
        /// Gets the bets
        /// </summary>
        /// <returns>Returns all the bets</returns>
        public IEnumerable<IBet> GetBets() => _bets;

        /// <summary>
        /// Builds the <see cref="ITicket" />
        /// </summary>
        /// <returns>Returns a <see cref="ITicket" /></returns>
        public ITicket BuildTicket()
        {
            var ticket = new Ticket(_ticketId, _sender, _bets, _reofferId, _altStakeRefId, _isTest, _oddsChangeType);
            _ticketId = null;
            _sender = null;
            _bets = null;
            _reofferId = null;
            _altStakeRefId = null;
            _isTest = false;
            _oddsChangeType = null;
            return ticket;
        }

        /// <summary>
        /// Adds the <see cref="IBet" />
        /// </summary>
        /// <param name="bet">A <see cref="IBet" /> to be added to this ticket</param>
        /// <returns>Returns a <see cref="ITicketBuilder" /></returns>
        public ITicketBuilder AddBet(IBet bet)
        {
            var bets = _bets as List<IBet> ?? new List<IBet>();
            bets.Add(bet);
            _bets = bets;
            return this;
        }
    }
}