﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Shiny.Caching;
using Shiny.Infrastructure;
using Shiny.Logging;
using Shiny.Settings;


namespace Shiny
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// WARNING: this will not catch startup issues as the connection isn't ready until after startup - it will catch all delegates though
        /// </summary>
        /// <param name="services"></param>
        /// <param name="enableCrashes"></param>
        /// <param name="enableEvents"></param>
        public static void UseSqliteLogging(this IServiceCollection services, bool enableCrashes = true, bool enableEvents = false)
        {
            services.AddIfNotRegistered<ShinySqliteConnection>();
            services.RegisterPostBuildAction(sp =>
            {
                var conn = sp.GetService<ShinySqliteConnection>();
                Log.AddLogger(new SqliteLog(conn), enableCrashes, enableEvents);
            });
        }


        public static void UseSqliteStorage(this IServiceCollection services)
        {
            services.AddIfNotRegistered<ShinySqliteConnection>();
            services.AddSingleton<IRepository, SqliteRepository>();
        }


        public static void UseSqliteCache(this IServiceCollection services)
        {
            services.AddIfNotRegistered<ShinySqliteConnection>();
            services.AddSingleton<ICache, SqliteCache>();
        }


        public static void UseSqliteSettings(this IServiceCollection services)
        {
            services.AddIfNotRegistered<ShinySqliteConnection>();
            services.AddSingleton<ISettings, SqliteSettings>();
        }
    }
}
