namespace GrpcGreeter
{
    using System;
    using System.Threading.Tasks;

    using Grpc.Core;

    using Microsoft.Extensions.Logging;

    public class GreeterService : Greeter.GreeterBase
    {
        private static Guid guid = Guid.NewGuid();

        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            await Task.Delay(100);
            return new HelloReply
            {
                Message = "Hello " + request.Name + " from " + guid.ToString()
            };
        }

        public override async Task SayHellos(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            var i = 0;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var message = $"How are you {request.Name}? {++i}";
                _logger.LogInformation($"Sending greeting {message}.");

                await responseStream.WriteAsync(new HelloReply { Message = message });

                // Gotta look busy
                await Task.Delay(1000);
            }
        }
    }
}
