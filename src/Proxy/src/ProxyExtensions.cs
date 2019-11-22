// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Proxy;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class ProxyExtensions
    {
        /// <summary>
        /// Runs proxy forwarding requests to the server specified by base uri.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="baseUri">Destination base uri</param>
        public static void RunProxy(this IApplicationBuilder app, IEnumerable<Uri> baseUris)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (baseUris == null || !baseUris.Any())
            {
                throw new ArgumentNullException(nameof(baseUris));
            }

            var options = new BackendPoolOption();

            options.Options = baseUris.Select(
                    baseUri => new ProxyOptions() { Scheme = baseUri.Scheme, Host = new HostString(baseUri.Authority), PathBase = baseUri.AbsolutePath, AppendQuery = new QueryString(baseUri.Query) })
                .ToArray();

            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// Runs proxy forwarding requests to the server specified by base uri.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="baseUri">Destination base uri</param>
        public static void RunProxy(this IApplicationBuilder app, Uri baseUri)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            var options = new BackendPoolOption(
                new ProxyOptions() { Scheme = baseUri.Scheme, Host = new HostString(baseUri.Authority), PathBase = baseUri.AbsolutePath, AppendQuery = new QueryString(baseUri.Query) });
            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// Runs proxy forwarding requests to the server specified by options.
        /// </summary>
        /// <param name="app"></param>
        public static void RunProxy(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseMiddleware<ProxyMiddleware>();
        }

        /// <summary>
        /// Runs proxy forwarding requests to the server specified by options.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options">Proxy options</param>
        public static void RunProxy(this IApplicationBuilder app, BackendPoolOption options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// Forwards current request to the specified destination uri.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationUri">Destination Uri</param>
        public static async Task ProxyRequest(this HttpContext context, Uri destinationUri)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (destinationUri == null)
            {
                throw new ArgumentNullException(nameof(destinationUri));
            }

            var proxyService = context.RequestServices.GetRequiredService<GrpcBrokerService>();

            using (var requestMessage = context.CreateProxyHttpRequest(destinationUri))
            {
                var prepareRequestHandler = proxyService.Options.PrepareRequest;
                if (prepareRequestHandler != null)
                {
                    await prepareRequestHandler(context.Request, requestMessage);
                }

                using (var responseMessage = await context.SendProxyHttpRequest(requestMessage))
                {
                    await context.CopyProxyHttpResponse(responseMessage);
                }
            }
        }
    }
}
