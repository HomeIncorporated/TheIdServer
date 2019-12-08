﻿using Aguacongas.IdentityServer.Admin.Http.Store;
using Aguacongas.IdentityServer.Store;
using Aguacongas.IdentityServer.Store.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServer4HttpStores(this IServiceCollection services, Func<IServiceProvider, Task<HttpClient>> getHttpClient)
        {
            var entityTypeList = GetEntityTypes();

            foreach (var entityType in entityTypeList)
            {
                var iAdminStoreType = typeof(IAdminStore<>)
                    .MakeGenericType(entityType.GetTypeInfo()).GetTypeInfo();

                services.AddTransient(iAdminStoreType, provider =>
                {
                    return CreateStore(getHttpClient, provider, entityType);
                });
            }

            return services.AddTransient<IIdentityUserStore<IdentityUser>>(p => 
                    new IdentityUserStore(getHttpClient.Invoke(p), p.GetRequiredService<ILogger<IdentityUserStore>>()))
                .AddTransient<IAdminStore<IdentityUser>>(p => p.GetRequiredService<IIdentityUserStore<IdentityUser>>())
                .AddTransient<IIdentityRoleStore<IdentityRole>>(p =>
                    new IdentityRoleStore(getHttpClient.Invoke(p), p.GetRequiredService<ILogger<IdentityRoleStore>>()))
                .AddTransient<IAdminStore<IdentityRole>>(p => p.GetRequiredService<IIdentityRoleStore<IdentityRole>>())
                .AddTransient<IIdentityProviderStore>(p => 
                    new IdentityProviderStore(getHttpClient.Invoke(p), p.GetRequiredService<ILogger<IdentityProviderStore>>()));
        }

        private static IEnumerable<Type> GetEntityTypes()
        {
            var assembly = typeof(IEntityId).GetTypeInfo().Assembly;
            var entityTypeList = assembly.GetTypes().Where(t => t.IsClass &&
                !t.IsAbstract &&
                t.GetInterface("IEntityId") != null);
            return entityTypeList;
        }

        private static object CreateStore(Func<IServiceProvider, Task<HttpClient>> getHttpClient, IServiceProvider provider, Type entityType)
        {
            var adminStoreType = typeof(EntityAdminStore<>)
                        .MakeGenericType(entityType.GetTypeInfo()).GetTypeInfo();

            var loggerType = typeof(ILogger<>).MakeGenericType(adminStoreType);
            return adminStoreType.GetConstructors()[0]
                .Invoke(new object[] { getHttpClient.Invoke(provider), provider.GetRequiredService(loggerType) });
        }
    }
}
