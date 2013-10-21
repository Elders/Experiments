using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Experiments
{
    /// <summary>
    /// Very fast way for creating objects.
    /// </summary>
    public static class FastActivatorWithParams
    {
        static ConcurrentDictionary<Type, TypeActivators> activators = new ConcurrentDictionary<Type, TypeActivators>();

        delegate object ObjectActivator(params object[] args);

        public static object CreateInstance(Type type, params object[] args)
        {
            TypeActivators typeActivators;

            if (!activators.TryGetValue(type, out typeActivators))
            {
                WarmInstanceConstructor(type);
                typeActivators = activators[type];
            }

            //if (typeActivators == null) return null;

            ObjectActivator activator = typeActivators.FindActivator(args);

            //if (activator == null) return null;

            return activator(args);
        }

        public static void WarmInstanceConstructor(Type type)
        {
            if (!activators.ContainsKey(type))
                activators.TryAdd(type, new TypeActivators(type));
        }

        class TypeActivators
        {
            ObjectActivator singleActivator;
            ConcurrentDictionary<int, ConcurrentDictionary<ConstructorInfo, ObjectActivator>> typeActivators = new ConcurrentDictionary<int, ConcurrentDictionary<ConstructorInfo, ObjectActivator>>();

            public TypeActivators(Type type)
            {
                var ctors = type.GetConstructors();

                var parameterlessCtor = ctors.Where(x => x.GetParameters().Length == 0).SingleOrDefault();
                if (parameterlessCtor != default(ConstructorInfo))
                {
                    singleActivator = BuildActivator(parameterlessCtor).Value;
                }
                foreach (ConstructorInfo ctor in type.GetConstructors())
                {
                    var fpCtor = ctor;
                    var activator = BuildActivator(fpCtor);

                    var newValue = new ConcurrentDictionary<ConstructorInfo, ObjectActivator>();
                    newValue.TryAdd(fpCtor, activator.Value);

                    Func<int, ConcurrentDictionary<ConstructorInfo, ObjectActivator>, ConcurrentDictionary<ConstructorInfo, ObjectActivator>> updateAction =
                        (key, val) => { val.AddOrUpdate(fpCtor, activator.Value, (k, v) => activator.Value); return val; };

                    typeActivators.AddOrUpdate(activator.Key, newValue, updateAction);
                }

            }

            public ObjectActivator FindActivator(params object[] args)
            {
                int argumentsCount = args.Length;
                if (argumentsCount == 0) return singleActivator;

                ConcurrentDictionary<ConstructorInfo, ObjectActivator> ctorActivators;

                if (typeActivators.TryGetValue(argumentsCount, out ctorActivators))
                {
                    if (ctorActivators.Count == 1)
                        return ctorActivators.Single().Value;
                    else
                    {
                        foreach (var ctorAct in ctorActivators)
                        {

                            var ctorParams = ctorAct.Key.GetParameters();
                            for (int i = 0; i < ctorParams.Length; i++)
                            {
                                if (args[i].GetType() != ctorParams[i].ParameterType)
                                    break;
                                if (i == ctorParams.Length - 1)
                                    return ctorAct.Value;
                            }
                        }
                    }
                }
                return null;
            }

            static KeyValuePair<int, ObjectActivator> BuildActivator(ConstructorInfo ctor)
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
                return new KeyValuePair<int, ObjectActivator>(paramsInfo.Count(), (ObjectActivator)lambda.Compile());
            }

        }
    }
}
