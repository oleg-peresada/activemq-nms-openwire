using Apache.NMS;
using Apache.NMS.ActiveMQ;

ConnectionFactory connectionFactory = new();
using IConnection connection = await connectionFactory.CreateConnectionAsync("artemis", "artemis");
await connection.StartAsync();

var r = "";