﻿namespace KafkaConsumerWorker.Application.Message
{
    public class StudentMessage
    {
        public string Type { get; set; }
        public Student Student { get; set; }
    }
}