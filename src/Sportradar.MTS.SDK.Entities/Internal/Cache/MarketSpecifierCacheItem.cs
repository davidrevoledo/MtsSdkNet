﻿/*
 * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
 */
using System.Diagnostics.Contracts;
using Sportradar.MTS.SDK.Entities.Internal.REST.Dto;

namespace Sportradar.MTS.SDK.Entities.Internal.Cache
{
    public class MarketSpecifierCacheItem
    {
        internal string Name { get; }

        internal string Type { get; }

        internal MarketSpecifierCacheItem(MarketSpecifierDTO dto)
        {
            Contract.Requires(dto != null);

            Type = dto.Type;
            Name = dto.Name;
        }
    }

    public class MarketAttributeCacheItem
    {
        internal string Name { get; }

        internal string Description { get; }

        internal MarketAttributeCacheItem(MarketAttributeDTO dto)
        {
            Contract.Requires(dto != null);

            Name = dto.Name;
            Description = dto.Description;
        }
    }
}