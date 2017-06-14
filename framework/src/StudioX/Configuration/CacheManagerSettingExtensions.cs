﻿using System.Collections.Generic;
using StudioX.Runtime.Caching;

namespace StudioX.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="ICacheManager"/> to get setting caches.
    /// </summary>
    public static class CacheManagerSettingExtensions
    {
        /// <summary>
        /// Gets application settings cache.
        /// </summary>
        public static ITypedCache<string, Dictionary<string, SettingInfo>> GetApplicationSettingsCache(this ICacheManager cacheManager)
        {
            return cacheManager
                .GetCache<string, Dictionary<string, SettingInfo>>(StudioXCacheNames.ApplicationSettings);
        }

        /// <summary>
        /// Gets tenant settings cache.
        /// </summary>
        public static ITypedCache<int, Dictionary<string, SettingInfo>> GetTenantSettingsCache(this ICacheManager cacheManager)
        {
            return cacheManager
                .GetCache<int, Dictionary<string, SettingInfo>>(StudioXCacheNames.TenantSettings);
        }

        /// <summary>
        /// Gets user settings cache.
        /// </summary>
        public static ITypedCache<string, Dictionary<string, SettingInfo>> GetUserSettingsCache(this ICacheManager cacheManager)
        {
            return cacheManager
                .GetCache<string, Dictionary<string, SettingInfo>>(StudioXCacheNames.UserSettings);
        }
    }
}