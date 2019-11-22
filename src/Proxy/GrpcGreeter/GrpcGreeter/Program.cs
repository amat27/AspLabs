using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrpcGreeter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var first = args.FirstOrDefault();
            int port = 0;
            if (!string.IsNullOrEmpty(first))
            {
                int.TryParse(first, out port);
            }

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                            if (port != 0)
                            {
                                webBuilder.UseUrls("https://localhost:" + port);
                            }
                        });
        }
    }
}