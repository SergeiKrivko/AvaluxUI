using System.Reflection;

namespace AvaluxUI.Utils;

public static class Injector
{
    private static readonly Dictionary<Type, object?> Instances = [];

    public static void AddService<TService>()
    {
        Instances[typeof(TService)] = null;
    }

    public static void AddService(Type serviceType)
    {
        Instances[serviceType] = null;
    }

    private static object CreateInstance(Type serviceType)
    {
        foreach (var constructor in serviceType.GetConstructors().Where(c => c.IsPublic))
        {
            try
            {
                var parameters = constructor.GetParameters()
                    .Select(p => Inject(p.ParameterType))
                    .ToArray();
                var instance = constructor.Invoke(parameters) ?? throw new InvalidOperationException();
                Instances[serviceType] = instance;
                return instance;
            }
            catch (TargetInvocationException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        throw new Exception("Cannot create an instance of " + serviceType.FullName);
    }

    public static object Inject(Type serviceType)
    {
        if (!Instances.TryGetValue(serviceType, out var instance))
        {
            var type = Instances.Keys.FirstOrDefault(serviceType.IsAssignableFrom) ??
                       throw new InvalidOperationException($"Unknown service type {serviceType.FullName}");
            return Instances[type] ?? CreateInstance(type);
        }

        return instance ?? CreateInstance(serviceType);
    }

    public static T Inject<T>()
    {
        return (T)Inject(typeof(T));
    }
}