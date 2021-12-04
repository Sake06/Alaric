# Alaric

Application uses RabbitMq as Message Broker.

Application Configuration

First Instance :

dotnet run --urls "http://localhost:5100"  --BaseOptions:ApplicationId "7153c9e0-cc02-4c18-a634-4b81052ea9ba" --RabbitMq:Hostname "localhost" --RabbitMq:Port 5672 --RabbitMq:UserName "user" RabbitMq:Password "password" --RabbitMq:ConsumeQueueName "Queue2" --RabbitMq:PublishQueueName "Queue1"  

Second Instance :

dotnet run --urls "http://localhost:5000"  --BaseOptions:ApplicationId "3c852d14-a11d-4311-bd47-03db0b983f44" --RabbitMq:Hostname "localhost" --RabbitMq:Port 5672 --RabbitMq:UserName "user" RabbitMq:Password "password" --RabbitMq:ConsumeQueueName "Queue1" --RabbitMq:PublishQueueName "Queue2"  



