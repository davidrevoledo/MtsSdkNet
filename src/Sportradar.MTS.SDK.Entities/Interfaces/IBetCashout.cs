﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System.Diagnostics.Contracts;
using Sportradar.MTS.SDK.Entities.Contracts;

namespace Sportradar.MTS.SDK.Entities.Interfaces
{
    /// <summary>
    /// Defines a contract for bet-level cashout
    /// </summary>
    [ContractClass(typeof(BetCashoutContract))]
    public interface IBetCashout
    {
        /// <summary>
        /// Gets the id of the bet
        /// </summary>
        string BetId { get; }

        /// <summary>
        /// Gets the cashout stake of the assigned bet
        /// </summary>
        /// <value>The cashout stake</value>
        long CashoutStake { get; }

        /// <summary>
        /// Gets the cashout percent of the assigned bet
        /// </summary>
        /// <value>The cashout percent</value>
        int? CashoutPercent { get; }
    }
}
