// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Proxy
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

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
        }

        public Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var option = _options.Pick();

            var uri = new Uri(UriHelper.BuildAbsolute(option.Scheme, option.Host, option.PathBase, context.Request.Path, context.Request.QueryString.Add(option.AppendQuery)));
            return context.ProxyRequest(uri);
        }
    }
}
