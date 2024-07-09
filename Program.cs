using System;
using System.Text;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Read { 
    class Reading { 
        
        public ConnectionFactory? factory; private IConnection? connection;  
        private IModel? channel;
        public string queue_name = "Test";

        public void ConnectServer() 
        { 
            try {
            factory = new ConnectionFactory {
                                            HostName = "localhost"
                                            };
            connection = factory.CreateConnection(); 
            channel = connection.CreateModel();
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed connection to RabbitMQ. {ex.Message}");
                throw;
            }
        }

        private void ReadData() 
        { 
            
            channel.QueueDeclare(queue: queue_name, durable: true, exclusive: false, arguments: null, autoDelete: false);
            Console.WriteLine("Waiting for messages..."); 

            var consumer = new EventingBasicConsumer(channel); 
            consumer.Received += (model, ea) => 
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routing_key = ea.RoutingKey;
                Console.WriteLine($"queue: {queue_name}\nrouting-key: {routing_key}\nmessage: {message}\n");
            };
            channel.BasicConsume(queue: queue_name, autoAck: true, consumer: consumer);

            Console.ReadLine();
        }

        public static void Main() 
        { 
            Reading Start = new Reading();
            Start.ConnectServer();
            Start.ReadData();
            
        }
    }
}