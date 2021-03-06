﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using Sportradar.MTS.SDK.Entities.Interfaces;

namespace Sportradar.MTS.SDK.Entities.Internal.TicketImpl
{
    /// <summary>
    /// Class AutoAcceptedOdds
    /// </summary>
    /// <seealso cref="IAutoAcceptedOdds" />
    public class AutoAcceptedOdds : IAutoAcceptedOdds
    {
        /// <summary>
        /// Selection index from 'ticket.selections' array (zero based)
        /// </summary>
        /// <returns>Selection index from 'ticket.selections' array (zero based)</returns>
        public int SelectionIndex { get; }

        /// <summary>
        /// Odds with which the ticket was placed
        /// </summary>
        /// <returns>Odds with which the ticket was placed</returns>
        public int RequestedOdds { get; }

        /// <summary>
        /// Odds with which the ticket was accepted
        /// </summary>
        /// <returns>Odds with which the ticket was accepted</returns>
        public int UsedOdds { get; }

        public AutoAcceptedOdds(int index, int requestedOdds, int usedOdds)
        {
            SelectionIndex = index;
            RequestedOdds = requestedOdds;
            UsedOdds = usedOdds;
        }
    }
}
