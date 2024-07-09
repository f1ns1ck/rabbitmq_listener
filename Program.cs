using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Runtime.InteropServices;


namespace Read { 
    class Reading { 
        
        private ConnectionFactory? factory; private IConnection? connection;  
        private IModel? channel;
        public string? queue_name {get; set;} = "Test";
        public string? message {get; set;}
        public string? routing_key {get; set;}
        public void ConnectServer() 
        { 
            try {
            factory = new ConnectionFactory {HostName = "localhost"};
            connection = factory.CreateConnection(); 
            channel = connection.CreateModel();
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed connection to RabbitMQ. {ex.Message}");
                throw;
            }
        }

        public void ReadData() 
        { 

            channel.QueueDeclare(queue: queue_name, durable: true, exclusive: false, arguments: null, autoDelete: false);
            Console.WriteLine("Waiting for messages..."); 

            var consumer = new EventingBasicConsumer(channel); 
            consumer.Received += (model, ea) => 
            {
                var data = new Reading();
                var body = ea.Body.ToArray();
                data.message = Encoding.UTF8.GetString(body);
                data.routing_key = ea.RoutingKey;
                data.queue_name = queue_name;

                string json = JsonConvert.SerializeObject(data);

                Console.WriteLine(json);

                Console.WriteLine($"queue: {data.queue_name}\nrouting-key: {data.routing_key}\nmessage: {data.message}\n");
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