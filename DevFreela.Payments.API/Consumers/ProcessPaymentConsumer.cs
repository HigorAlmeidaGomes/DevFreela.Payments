using DevFreela.Payments.API.Model;
using DevFreela.Payments.API.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DevFreela.Payments.API.Consumers
{
    public class ProcessPaymentConsumer : BackgroundService
    {
        private const string QUEUE = "Payments";
        private const string PAYMENT_APROVED_QUEUE = "PaymentsApproved";
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        public ProcessPaymentConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            QueueDeclare();

        }

        private void QueueDeclare()
        {
            _channel.QueueDeclare(queue: QUEUE, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: PAYMENT_APROVED_QUEUE, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (sender, eventArgs) =>
                {
                    var byteArry = eventArgs.Body.ToArray();
                    var paymentInfoJson = Encoding.UTF8.GetString(byteArry);

                    var paymentInfo = JsonSerializer.Deserialize<PaymentsInfoInputModel>(paymentInfoJson);

                    ProcessPayment(paymentInfo);

                    var paymentApproved = new PaymentApprovedIntegrationEvent(paymentInfo.IdProject);
                    var paymentApprovedJson = JsonSerializer.Serialize(paymentApproved);
                    var paymentApprovedBytes = Encoding.UTF8.GetBytes(paymentApprovedJson);

                    _channel.BasicPublish(exchange: "",
                        routingKey: PAYMENT_APROVED_QUEUE,
                        basicProperties: null,
                        body: paymentApprovedBytes);


                    _channel.BasicAck(eventArgs.DeliveryTag, false);

                };
                _channel.BasicConsume(QUEUE, false, consumer);

                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                _connection.HandleConnectionBlocked(ex.Message);
                throw;
            }
        }

        private void ProcessPayment(PaymentsInfoInputModel paymentInfo)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentServices>();
                paymentService.Process(paymentInfo);
            }
        }
    }
}
