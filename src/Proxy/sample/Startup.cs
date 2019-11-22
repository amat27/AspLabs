// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Proxy
{
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddProxy(options =>
            // {
            //     options.PrepareRequest = (originalRequest, message) =>
            //     {
            //         message.Headers.Add("X-Forwarded-Host", originalRequest.Host.Host);
            //         return Task.FromResult(0);
            //     };
            // });
            services.AddProxy();
        }

        public void Configure(IApplicationBuilder app)
        {
            //app.UseWebSockets().RunProxy(new Uri("https://example.com"));
            app.RunProxy(new Uri("https://localhost:5001"));
        }

        public static void Main(string[] args)
        {
            // var host = new WebHostBuilder()
            //     .UseKestrel()
            //     .UseIISIntegration()
            //     .UseStartup<Startup>()
            //     .Build();
            // 
            // host.Run();



            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    })
                
                .Build().Run();


        }
    }
}

