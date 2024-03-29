﻿using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Outbox.Core;

namespace Outbox.Infrastructure;

public class DistributedMessageLocker : IMessageLocker
{
    private readonly IDistributedCache _cache;

    public DistributedMessageLocker(IDistributedCache cache)
    {
        _cache = cache;
    }

    public ValueTask<bool> LockMessage(Message message)
    {
        lock (_cache)
        {
            var valueString = _cache.GetString(message.Id.ToString());
            if (!string.IsNullOrWhiteSpace(valueString))
            {
                return new ValueTask<bool>(false);
            }

            var key = message.Id.ToString();
            var valueBytes = Encoding.UTF8.GetBytes(key);
            _cache.Set(key, valueBytes, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });
            return new ValueTask<bool>(true);
        }
    }
}