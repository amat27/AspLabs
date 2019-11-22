namespace GrpcGreeterClient
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Grpc.Core;
    using Grpc.Net.Client;

    using GrpcGreeter;

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // The port number(5001) must match the port of the gRPC server.
                //var channel = GrpcChannel.ForAddress("https://localhost:5002");
                var channel = GrpcChannel.ForAddress("https://localhost:31512");
                //var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new Greeter.GreeterClient(channel);
                Stopwatch sw = Stopwatch.StartNew();
                int msg = 1;
                var tasks = Enumerable.Range(0, msg).Select(_ => SayHelloSteamingAsync()).ToArray();
                // Task[] tasks = new Task[msg];
                // for (int i = 0; i < msg; i++)
                // {
                //     tasks[i] = SayHelloAsync();
                // }
                await Task.WhenAll(tasks);

                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);

                async Task SayHelloAsync()
                {
                    var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
                    // Console.WriteLine("Greeting: " + reply.Message);
                }

                async Task SayHelloSteamingAsync()
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(3.5));

                    using (var call = client.SayHellos(new HelloRequest { Name = "GreeterClient" }, cancellationToken: cts.Token))
                    {
                        try
                        {
                            await foreach (var message in call.ResponseStream.ReadAllAsync())
                            {
                                Console.WriteLine("Greeting: " + message.Message);
                            }
                        }
                        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                        {
                            Console.WriteLine("Stream cancelled.");
                        }
                    }
                }

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}