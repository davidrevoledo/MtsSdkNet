﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System.Diagnostics.Contracts;
using System.Net;
using System.Text.RegularExpressions;
using Sportradar.MTS.SDK.Entities.Interfaces;
using Sportradar.MTS.SDK.Entities.Internal;

namespace Sportradar.MTS.SDK.Entities.Contracts
{
    [ContractClassFor(typeof(IEndCustomer))]
    internal abstract class EndCustomerContract : IEndCustomer
    {
        public IPAddress Ip => Contract.Result<IPAddress>();

        public string LanguageId {
            get
            {
                Contract.Ensures(Contract.Result<string>() == null ||
                                 Contract.Result<string>().Length == 2);
                return Contract.Result<string>();
            }
        }

        public string DeviceId
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() == null ||
                                 (Regex.IsMatch(Contract.Result<string>(), TicketHelper.IdPattern)
                                  && Contract.Result<string>().Length >= 1
                                  && Contract.Result<string>().Length <= 36));
                return Contract.Result<string>();
            }
        }

        public string Id {
            get
            {
                Contract.Ensures(Contract.Result<string>() == null ||
                                 (Regex.IsMatch(Contract.Result<string>(), TicketHelper.IdPattern)
                                  && Contract.Result<string>().Length >= 1
                                  && Contract.Result<string>().Length <= 36));
                return Contract.Result<string>();
            }
        }

        public long Confidence {
            get
            {
                Contract.Ensures(Contract.Result<long>() >= 0);
                return Contract.Result<long>();
            }
        }
    }
}
