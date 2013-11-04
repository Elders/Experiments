using System;
namespace AsyncWork.AsynkWorkWithRabbitMq
{
    public interface IUpdateState
    {
        object UpdateState(object state);
    }
}