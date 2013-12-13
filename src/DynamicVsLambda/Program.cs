using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicVsLambda
{
    public class Event1 { }
    public class Event2 { }
    public class Event3 { }
    public class Event4 { }
    public class Event5 { }
    public class Event6 { }

    public class Handler
    {
        public bool Handle(Event1 x) { return true; }
        public bool Handle(Event2 x) { return true; }
        public bool Handle(Event3 x) { return true; }
        public bool Handle(Event4 x) { return true; }
        public bool Handle(Event5 x) { return true; }
        public bool Handle(Event6 x) { return true; }
    }

    class Program
    {
        public static int counter = 0;

        static Dictionary<Type, LateBoundMethod> calls = new Dictionary<Type, LateBoundMethod>();

        static void Main(string[] args)
        {
            //List<object> events = new List<object>() { new Event1(), new Event2(), new Event3(), new Event4(), new Event5(), new Event6() };
            Dictionary<object, Type> events = new Dictionary<object, Type>()
            {
                {new Event1(),typeof(Event1)},
            {new Event2(),typeof(Event2)},
            {new Event3(),typeof(Event3)},
            {new Event4(),typeof(Event4)},
            {new Event5(),typeof(Event5)},
            {new Event6(),typeof(Event6)}
           
            };
            var obj = new Handler();
            var mis = obj.GetType().GetMethods().Where(x => x.Name == "Handle");

            foreach (var mi in mis)
            {
                LateBoundMethod callback = DelegateFactory.Create(mi);
                var parType = mi.GetParameters().First().ParameterType;
                calls.Add(parType, callback);
            }

            var dynamicResults = MeasureExecutionTime.Start(() =>
            {
                dynamic handler = FastActivator.CreateInstance(typeof(Handler));
                foreach (var evnt in events)
                {
                    handler.Handle((dynamic)evnt.Key);
                }

            }, 1000000);

            var lambdaResults = MeasureExecutionTime.Start(() =>
            {
                var handler = FastActivator.CreateInstance(typeof(Handler));
                foreach (var evnt in events)
                {

                    calls[evnt.Value](handler, new object[] { evnt.Key });
                }

            }, 1000000);

            Console.WriteLine(dynamicResults);
            Console.WriteLine(lambdaResults);
            Console.ReadLine();
        }
    }

    public static class FastActivator
    {
        static System.Collections.Concurrent.ConcurrentDictionary<Type, ObjectActivator> activators = new System.Collections.Concurrent.ConcurrentDictionary<Type, ObjectActivator>();

        public delegate object ObjectActivator(params object[] args);

        static ObjectActivator GetActivator(ConstructorInfo ctor)
        {
            ParameterInfo[] paramsInfo = ctor.GetParameters();
            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
            Expression[] argsExp = new Expression[paramsInfo.Length];
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }
            NewExpression newExp = Expression.New(ctor, argsExp);
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);
            return (ObjectActivator)lambda.Compile();
        }

        public static void WarmInstanceConstructor(Type type)
        {
            if (!activators.ContainsKey(type))
            {
                var constructors = type.GetConstructors();
                if (constructors.Length == 1)
                {
                    ConstructorInfo ctor = constructors.First();
                    activators.TryAdd(type, GetActivator(ctor));
                }
            }
        }

        public static object CreateInstance(Type type, params object[] args)
        {
            ObjectActivator activator;
            if (!activators.TryGetValue(type, out activator))
            {
                var constructors = type.GetConstructors();
                if (constructors.Length == 1)
                {
                    ConstructorInfo ctor = type.GetConstructors().First();
                    activator = GetActivator(ctor);
                    activators.TryAdd(type, activator);
                }
                else
                {
                    activator = (a) => Activator.CreateInstance(type, a);
                }

            }
            return activator(args);
        }
    }

    public delegate object LateBoundMethod(object target, object[] arguments);

    public static class DelegateFactory
    {
        public static LateBoundMethod Create(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<LateBoundMethod> lambda = Expression.Lambda<LateBoundMethod>(
              Expression.Convert(call, typeof(object)),
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
              Expression.Convert(
                Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
        }
    }

    public static class MeasureExecutionTime
    {
        public static string Start(System.Action action)
        {
            string result = string.Empty;

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            action();
            stopWatch.Stop();
            System.TimeSpan ts = stopWatch.Elapsed;
            result = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            return result;
        }

        public static string Start(System.Action action, int repeat, bool showTicksInfo = false)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < repeat; i++)
            {
                action();
            }
            stopWatch.Stop();
            System.TimeSpan total = stopWatch.Elapsed;
            System.TimeSpan average = new System.TimeSpan(stopWatch.Elapsed.Ticks / repeat);

            System.Text.StringBuilder perfResultsBuilder = new System.Text.StringBuilder();
            perfResultsBuilder.AppendLine("--------------------------------------------------------------");
            perfResultsBuilder.AppendFormat("  Total Time => {0}\r\nAverage Time => {1}", Align(total), Align(average));
            perfResultsBuilder.AppendLine();
            perfResultsBuilder.AppendLine("--------------------------------------------------------------");
            if (showTicksInfo)
                perfResultsBuilder.AppendLine(TicksInfo());
            return perfResultsBuilder.ToString();
        }

        static string Align(System.TimeSpan interval)
        {
            string intervalStr = interval.ToString();
            int pointIndex = intervalStr.IndexOf(':');

            pointIndex = intervalStr.IndexOf('.', pointIndex);
            if (pointIndex < 0) intervalStr += "        ";
            return intervalStr;
        }

        static string TicksInfo()
        {
            System.Text.StringBuilder ticksInfoBuilder = new System.Text.StringBuilder("\r\n\r\n");
            ticksInfoBuilder.AppendLine("Ticks Info");
            ticksInfoBuilder.AppendLine("--------------------------------------------------------------");
            const string numberFmt = "{0,-22}{1,18:N0}";
            const string timeFmt = "{0,-22}{1,26}";

            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Field", "Value"));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "-----", "-----"));

            // Display the maximum, minimum, and zero TimeSpan values.
            ticksInfoBuilder.AppendLine(System.String.Format(timeFmt, "Maximum TimeSpan", Align(System.TimeSpan.MaxValue)));
            ticksInfoBuilder.AppendLine(System.String.Format(timeFmt, "Minimum TimeSpan", Align(System.TimeSpan.MinValue)));
            ticksInfoBuilder.AppendLine(System.String.Format(timeFmt, "Zero TimeSpan", Align(System.TimeSpan.Zero)));
            ticksInfoBuilder.AppendLine();

            // Display the ticks-per-time-unit fields.
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per day", System.TimeSpan.TicksPerDay));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per hour", System.TimeSpan.TicksPerHour));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per minute", System.TimeSpan.TicksPerMinute));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per second", System.TimeSpan.TicksPerSecond));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per millisecond", System.TimeSpan.TicksPerMillisecond));
            ticksInfoBuilder.AppendLine("--------------------------------------------------------------");
            return ticksInfoBuilder.ToString();
        }
    }
}
