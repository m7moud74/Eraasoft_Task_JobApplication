using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace JobApplication.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly RedisSettings _settings;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly Lazy<IConnectionMultiplexer?> _lazyConnection;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(IOptions<RedisSettings> options, ILogger<RedisCacheService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        _lazyConnection = new Lazy<IConnectionMultiplexer?>(() =>
        {
            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.ConnectionString))
            {
                _logger.LogInformation("Redis cache is disabled or connection string is empty.");
                return null;
            }

            try
            {
                var configOptions = ConfigurationOptions.Parse(_settings.ConnectionString);
                configOptions.AbortOnConnectFail = false;
                configOptions.ConnectTimeout = 3000;
                configOptions.SyncTimeout = 3000;
                return ConnectionMultiplexer.Connect(configOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to Redis at startup. Falling back to non-cached execution.");
                return null;
            }
        });
    }

    private IDatabase? GetDatabase()
    {
        try
        {
            var multiplexer = _lazyConnection.Value;
            if (multiplexer is not null && multiplexer.IsConnected)
            {
                return multiplexer.GetDatabase();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis connection is currently unavailable.");
        }

        return null;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return default;
        }

        try
        {
            var db = GetDatabase();
            if (db is null)
            {
                return default;
            }

            var value = await db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading key '{Key}' from Redis cache. Falling back to database.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key) || value is null)
        {
            return;
        }

        try
        {
            var db = GetDatabase();
            if (db is null)
            {
                return;
            }

            var serialized = JsonSerializer.Serialize(value, JsonOptions);
            var ttl = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes > 0 ? _settings.DefaultExpirationMinutes : 5);

            await db.StringSetAsync(key, serialized, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error storing key '{Key}' into Redis cache.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        try
        {
            var db = GetDatabase();
            if (db is null)
            {
                return;
            }

            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing key '{Key}' from Redis cache.", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return;
        }

        try
        {
            var multiplexer = _lazyConnection.Value;
            if (multiplexer is null || !multiplexer.IsConnected)
            {
                return;
            }

            var db = multiplexer.GetDatabase();
            var endpoints = multiplexer.GetEndPoints();

            foreach (var endpoint in endpoints)
            {
                try
                {
                    var server = multiplexer.GetServer(endpoint);
                    if (!server.IsConnected) continue;

                    var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                    if (keys.Length > 0)
                    {
                        await db.KeyDeleteAsync(keys);
                        _logger.LogInformation("Invalidated {Count} Redis cache keys matching prefix '{Prefix}'.", keys.Length, prefix);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed scanning keys on Redis endpoint '{Endpoint}' for prefix '{Prefix}'.", endpoint, prefix);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cache invalidation by prefix '{Prefix}'.", prefix);
        }
    }
}
