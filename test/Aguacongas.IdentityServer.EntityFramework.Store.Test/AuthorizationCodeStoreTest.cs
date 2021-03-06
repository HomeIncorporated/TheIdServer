﻿using Aguacongas.IdentityServer.Store.Entity;
using IdentityModel;
using IdentityServer4.Stores.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.IdentityServer.EntityFramework.Store.Test
{
    public class AuthorizationCodeStoreTest
    {
        [Fact]
        public void Constructor_should_validate_parameters()
        {
            var builder = new ServiceCollection()
                .AddOperationalEntityFrameworkStores(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                .BuildServiceProvider();
            Assert.Throws<ArgumentNullException>(() => new AuthorizationCodeStore(null, null, null));
            Assert.Throws<ArgumentNullException>(() => new AuthorizationCodeStore(builder.GetRequiredService<OperationalDbContext>(), null, null));
            Assert.Throws<ArgumentNullException>(() =>
                new DeviceFlowStore(builder.GetRequiredService<OperationalDbContext>(),
                builder.GetRequiredService<IPersistentGrantSerializer>(), null));
        }

        [Fact]
        public async Task GetAuthorizationCodeAsync_should_return_code()
        {
            CreateSut(out IServiceScope scope, out OperationalDbContext context, out AuthorizationCodeStore sut);

            using (scope)
            {
                var id = GenerateId();
                await context.AuthorizationCodes.AddAsync(new AuthorizationCode
                {
                    Id = id,
                    ClientId = GenerateId(),
                    Data = "{}"                
                });
                await context.SaveChangesAsync();

                Assert.NotNull(await sut.GetAuthorizationCodeAsync(id).ConfigureAwait(false));
                Assert.Null(await sut.GetAuthorizationCodeAsync(GenerateId()).ConfigureAwait(false));
            }

        }

        [Fact]
        public async Task RemoveAuthorizationCodeAsync_should_delete_code()
        {
            CreateSut(out IServiceScope scope, out OperationalDbContext context, out AuthorizationCodeStore sut);

            using (scope)
            {
                var id = GenerateId();
                await context.AuthorizationCodes.AddAsync(new AuthorizationCode
                {
                    Id = id,
                    ClientId = GenerateId(),
                    Data = "{}"
                });
                await context.SaveChangesAsync();

                await sut.RemoveAuthorizationCodeAsync(id).ConfigureAwait(false);
                await sut.RemoveAuthorizationCodeAsync(id).ConfigureAwait(false);

                Assert.Null(await context.DeviceCodes.FirstOrDefaultAsync(d => d.Id == id));
            }

        }

        [Fact]
        public async Task StoreAuthorizationCodeAsync_should_add_data_in_db()
        {
            CreateSut(out IServiceScope scope, out OperationalDbContext context, out AuthorizationCodeStore sut);

            var authorizationCode = new IdentityServer4.Models.AuthorizationCode
            {                
                ClientId = GenerateId(),
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new [] { new Claim(JwtClaimTypes.Subject, "test") }))
            };
            await sut.StoreAuthorizationCodeAsync(authorizationCode);

            using (scope)
            {
                Assert.NotNull(await context.AuthorizationCodes.FirstOrDefaultAsync(d => d.ClientId == authorizationCode.ClientId));
            }
        }

        private static void CreateSut(out IServiceScope scope, out OperationalDbContext context, out AuthorizationCodeStore sut)
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .AddOperationalEntityFrameworkStores(options =>
                    options.UseInMemoryDatabase(GenerateId()))
                .BuildServiceProvider();
            scope = provider.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            context = serviceProvider.GetRequiredService<OperationalDbContext>();
            sut = serviceProvider.GetRequiredService<AuthorizationCodeStore>();
        }

        private static string GenerateId()
            => Guid.NewGuid().ToString();

    }
}
