namespace AsyncWork.AsynkWorkWithRabbitMq
{
    public interface IProcessState
    {
        void ProcessWork(object state);
    }
}