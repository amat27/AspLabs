// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Proxy
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Proxy Middleware
    /// </summary>
    public class ProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BackendPoolOption _options;

        private readonly BufferBlock<ProxyOptions> proxies = new BufferBlock<ProxyOptions>();

        public ProxyMiddleware(RequestDelegate next, IOptions<BackendPoolOption> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Value == null)
            {
                throw new ArgumentNullException(nameof(options.Value));
            }

            foreach (var option in options.Value.Options)
            {
                if (option.Scheme == null)
                {
                    throw new ArgumentException("Options parameter must specify scheme.", nameof(options));
                }
                if (!option.Host.HasValue)
                {
                    throw new ArgumentException("Options parameter must specify host.", nameof(options));
                }
            }

            _next = next;
            _options = options.Value;
            if (_options.Throttling)
            {
                foreach (var proxy in _options.Options)
                {
                    for(int i = 0; i != 2000; ++i)
                    proxies.Post(proxy);
                }
            }
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ProxyOptions option = null;
            if (_options.Throttling)
            {
                //Console.WriteLine("Throttling mode");
                option = await proxies.ReceiveAsync();
                //Console.WriteLine($"Choose {option.Host}, proxies length {proxies.Count}");
            }
            else
            {
                option = _options.Pick();
            }

            try
            {
                var uri = new Uri(UriHelper.BuildAbsolute(option.Scheme, option.Host, option.PathBase, context.Request.Path, context.Request.QueryString.Add(option.AppendQuery)));
                await context.ProxyRequest(uri);
            }
            finally
            {

                if (_options.Throttling)
                {
                    await proxies.SendAsync(option);
                    //Console.WriteLine($"Return {option.Host}, proxies length {proxies.Count}");
                }
            }
        }
    }
}
