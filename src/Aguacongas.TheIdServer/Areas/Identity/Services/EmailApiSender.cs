﻿using Aguacongas.IdentityServer.Admin.Models;
using Aguacongas.IdentityServer.Http.Store;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aguacongas.TheIdServer.Areas.Identity.Services
{
    public class EmailApiSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly IdentityServerOptions _options;

        public EmailApiSender(HttpClient httpClient, IdentityServerOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var content = new StringContent(JsonSerializer.Serialize(new Email
            {
                Addresses = new string[] { email },
                Message = htmlMessage,
                Subject = subject
            }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(_options.ApiUrl, content)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
    }
}
