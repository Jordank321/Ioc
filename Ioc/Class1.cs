using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ioc
{
    public class IocContainer
    {
        private Dictionary<Type, Func<object>> _resolvers = new Dictionary<Type, Func<object>>();

        public void Register<T>()
        {
            _resolvers.Add(typeof(T), ()=>Construct(typeof(T)));
        }

        public void Register<T>(Func<T> builderFunc)
        {
            _resolvers.Add(typeof(T), ()=>builderFunc());
        }

        public void Register<T, TConcrete>() where TConcrete : T
        {
            _resolvers.Add(typeof(T), () => Construct(typeof(TConcrete)));
        }

        public void Register<T>(Action<T> builderAction)
        {
            _resolvers.Add(typeof(T), () =>
            {
                var instance = Construct(typeof(T));
                builderAction((T)instance);
                return instance;
            });
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type t)
        {
            if (_resolvers.ContainsKey(t))
            {
                return _resolvers[t]();
            }
            else
            {
                if (!t.IsValueType) return null;
                return Activator.CreateInstance(t);
            }
        }

        public bool HasRegistration(Type t)
        {
            return _resolvers.ContainsKey(t);
        }

        private object Construct(Type targetType)
        {
            // look for constructors
            var constructors = targetType.GetConstructors()
                .Where(c => c.IsPublic)
                .OrderBy(c => c.GetParameters().Count());

            /*
            // select ones we like
            var possibleConstructors = constructors.Where(c => c.IsPublic);

            // order by parameter count - easiest first
            var orderedConstructors = possibleConstructors.OrderBy(c => c.GetParameters().Count());
            */
            foreach(var constructor in constructors)
            {
                // try and resolve any parameters for the constructor
                // using ourselves...
                var parameters = constructor.GetParameters();
                var parameterValues = new List<object>();

                foreach(var parameter in parameters)
                {
                    var parameterType = parameter.ParameterType;
                    if (!HasRegistration(parameterType))
                    {
                        continue;
                    }
                    parameterValues.Add(Resolve(parameterType));
                }

                if (parameterValues.Count() != parameters.Length)
                {
                    continue;
                }
                else
                {
                    return constructor.Invoke(parameterValues.ToArray());
                }
            }

            throw new InvalidOperationException($"Unable to resolve constructor of type {targetType.FullName}");
        }
    }
}
