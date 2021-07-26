# Correlation manager

This manager can help you control correlationId and do structure logging.

## CorrelationManager.Core ![Nuget](https://img.shields.io/nuget/v/CorrelationManager.Core)

For add correlation service use `AddCorrelationManager()` method in your DI registration logic.

```c#
public void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddCorrelationManager();
    // ...
}
```

This service will add `x-correlation-id` header for all HTTP requests.

### Configuration

You can use custom correlation header.


#### application.json
```json
{
    "CorrelationManager.Core": {
        "CorrelationHeaderName": "RequestId"
    }
}
```

#### service options
```c#
public void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddCorrelationManager(options =>
    {
        options.CorrelationHeaderName = "RequestId";
    });
    // ...
}
```

## CorrelationManager.Logger ![Nuget](https://img.shields.io/nuget/v/CorrelationManager.Logger)

### JsonFormatter

#### Using

To use json logging, add `AddConsoleCorrelationJsonLogger` in `Program.cs` to `ILoggerBuilder`

```c#
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.AddConsoleCorrelationJsonLogger();
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}
```

#### Configure

Set formatter name `"FormatterName": "correlationJson"`
For configure formatter using [JsonConsoleFormatterOptions](https://docs.microsoft.com/ru-ru/dotnet/api/microsoft.extensions.logging.console.jsonconsoleformatteroptions?view=dotnet-plat-ext-5.0) 

###### Example application.json
```json
{
  "Logging": {
    "Console": {
      "FormatterName": "correlationJson",
      "FormatterOptions": {
        "TimestampFormat": "HH:mm:ss",
        "UseUtcTimestamp": true,
        "JsonWriterOptions": {
          "Indented": true
        }
      },
      "LogLevel": {
        "Microsoft": "Warning",
        "Default": "Information"
      }
    }
  },
  "AllowedHosts": "*"
}
```
### Middlewares
#### CorrelationLoggerMiddleware

This middleware will get correlation id header from client and start logger scope with this correlation id.
```c#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
{
    // ...
    
    app.UseCorrelationLogger();
    
    // ...
}
```

#### RequestLogger
This middleware will log request information

```c#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
{
    // ...
    
    app.UseCorrelationLogger();
    app.UseRequestLogger();
    
    // ...
}
```
## CorrelationManager.Extensions.RabbitMq ![Nuget](https://img.shields.io/nuget/v/CorrelationManager.Extensions.RabbitMq)

### Consumer correlation

For get correlation id from `correlation_id` property use `EventingCorrelationConsumer`

```c#
public class Consumer: BackgroundService
{
    private readonly ICorrelationManagerFactory _factory;
    private readonly IModel _model;
    private readonly ILogger<Consumer> _logger;

    public Consumer(ILogger<Consumer> logger, ICorrelationManagerFactory factory)
    {
        _logger = logger;
        _factory = factory;
        var connectionFactory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        var connection = connectionFactory.CreateConnection();
        _model = connection.CreateModel();

        _model.QueueDeclare("TEST_QUEUE", exclusive: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingCorrelationConsumer(_model, _factory);
        consumer.Received += (_, args) => CallBack(args);

        _model.BasicConsume("TEST_QUEUE", true, consumer);
        
        return Task.CompletedTask;
    }

    private void CallBack(BasicDeliverEventArgs args)
    {
        _logger.LogInformation("Got message from TEST_QUEUE");
    }
}
```

### Producer correlation
For send correlationId to `correlation_id` property use extension method `BasicPublish`
```c#
public class Producer
{
    private readonly ICorrelationManagerFactory _factory;

    public Producer(ICorrelationManagerFactory factory)
    {
        _factory = factory;
    }

    public void Send()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        var connection = factory.CreateConnection();
        var model = connection.CreateModel();

        model.QueueDeclare("TEST_QUEUE", exclusive: false);
        model.BasicPublish(_factory, "", "TEST_QUEUE", null, ReadOnlyMemory<byte>.Empty);
    }
}
```



