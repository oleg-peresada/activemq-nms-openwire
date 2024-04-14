using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;

ConnectionFactory connectionFactory = new();
using IConnection connection = await connectionFactory.CreateConnectionAsync("artemis", "artemis");

// ISession session = await connection.CreateSessionAsync();
// var consumer = session.CreateConsumer(SessionUtil.GetQueue(session, "INT.PAYROLL"));
// consumer.Listener += message =>
// {
//
// };
//
await connection.StartAsync();

var r = "";