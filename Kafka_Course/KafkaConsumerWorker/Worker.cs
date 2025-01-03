namespace KafkaConsumerWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers      = configuration["KafkaConfig:BootstrapServers"],
                GroupId               = configuration["KafkaConfig:GroupId"],
                AutoOffsetReset       = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true,
                EnableAutoCommit      = false,
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe(configuration["KafkaConfig:Topic"]);
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(stoppingToken);
                    _logger.LogInformation($"Received message: {cr.Value}");

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();

                        // Deserialize the message to CourseMessage
                        var courseMessage = JsonSerializer.Deserialize<CourseMessage>(cr.Value);
               
                        var courseDTO = new CourseDTO
                        {
                            CourseId    = courseMessage.Course.CourseId,
                            CourseName  = courseMessage.Course.CourseName,
                            Description = courseMessage.Course.Description,
                            Tuition     = courseMessage.Course.Tuition,
                            StartDay    = courseMessage.Course.StartDay,
                            EndDay      = courseMessage.Course.EndDay
                        };

                        switch (courseMessage.Type)
                        {
                            case "Add":
                                await courseService.AddCourseAsync(courseDTO);
                                break;
                            case "Update":
                                await courseService.UpdateCourseAsync(courseDTO);
                                break;
                            case "Delete":
                                await courseService.DeleteCourseAsync(courseDTO.CourseId);
                                break;
                            default:
                                _logger.LogWarning($"Unknown action: {courseMessage.Type}");
                                break;
                        }
                    }

                    _consumer.Commit(cr); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the message.");
                }
            }
        }

    }
}
