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
        var constructor = serviceType.GetConstructors().First(c => c.IsPublic);
        var parameters = constructor.GetParameters()
            .Select(p => Inject(p.ParameterType))
            .ToArray();
        var instance = Activator.CreateInstance(serviceType, parameters) ?? throw new InvalidOperationException();
        Instances[serviceType] = instance;
        return instance;
    }

    public static object Inject(Type serviceType)
    {
        if (!Instances.TryGetValue(serviceType, out var instance))
            throw new InvalidOperationException("Unknown service type");
        return instance ?? CreateInstance(serviceType);
    }

    public static T Inject<T>()
    {
        return (T)Inject(typeof(T));
    }
}