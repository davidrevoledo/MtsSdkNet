﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System.Linq;
using Sportradar.MTS.SDK.API.Internal.Senders;
using Sportradar.MTS.SDK.API.Internal.TicketImpl;
using Sportradar.MTS.SDK.Entities.Interfaces;
using Sportradar.MTS.SDK.Entities.Internal.Dto;
using Sportradar.MTS.SDK.Entities.Internal.Dto.TicketResponse;
using Sportradar.MTS.SDK.Entities.Internal.TicketImpl;

namespace Sportradar.MTS.SDK.API.Internal.Mappers
{
    /// <summary>
    /// Implementation of <see cref="ITicketMapper{TIn,TOut}"/> for <see cref="ITicketResponse"/>
    /// </summary>
    public class TicketResponseMapper : ITicketResponseMapper<TicketResponseDTO, ITicketResponse>
    {
        /// <summary>
        /// The ticket ack sender
        /// </summary>
        private readonly ITicketSender _ticketSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketResponseMapper"/> class
        /// </summary>
        /// <param name="ticketSender">The ticket ack sender</param>
        public TicketResponseMapper(ITicketSender ticketSender)
        {
            _ticketSender = ticketSender;
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="source">The source</param>
        /// <param name="correlationId">The correlation id</param>
        /// <param name="orgJson">The original json string received from the mts</param>
        /// <returns>ITicketResponse</returns>
        public ITicketResponse Map(TicketResponseDTO source, string correlationId, string orgJson)
        {
            return new TicketResponse(_ticketSender,
                                      source.Result.TicketId,
                                      MtsTicketHelper.Convert(source.Result.Status), 
                                      new ResponseReason(source.Result.Reason.Code, source.Result.Reason.Message),
                                      source.Result.BetDetails?.ToList().ConvertAll(b => new BetDetail(b)),
                                      correlationId,
                                      source.Signature,
                                      source.ExchangeRate,
                                      source.Version,
                                      null,
                                      orgJson);
        }
    }
}