using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lib.Render
{

internal static class VertexFormatStore
{
    private static readonly Dictionary<Type, Action<INeedsFormat>> _data = new();

    static VertexFormatStore()
    {
        foreach (Type implementation in GetImplementations())
        {
            const string methodName = nameof(IVertex.SetLayout);

            MethodInfo? info = implementation.GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Static | BindingFlags.Instance );

            if (info is null)
                throw new NotImplementedException($"{implementation} missing static Method {methodName}");

            if (!info.IsStatic)
                throw new NotImplementedException($"{implementation} Method {methodName} needs to be static");

            ParameterInfo[] parameterInfos = info.GetParameters().ToArray();
            if (parameterInfos.Length == 0 ||
                parameterInfos.Length > 1 ||
                parameterInfos[0].ParameterType != typeof(INeedsFormat))
                throw new NotImplementedException($"{implementation} Method {methodName} has incorrect signature");

            var action = (Action<INeedsFormat>) Delegate.CreateDelegate(typeof(Action<INeedsFormat>), info);
            _data.Add(implementation, action);
        }
    }

    internal static void AddVertex(Type type, Action<INeedsFormat> setterMethod)
    {
        _data.Add(type, setterMethod);
    }

    internal static bool Has(Type type)
    {
        return _data.ContainsKey(type);
    }

    internal static void Set(Type vertexType, INeedsFormat needsFormat)
    {
        _data[vertexType].Invoke(needsFormat);
    }


    private static IEnumerable<Type> GetImplementations()
    {
        return Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(IVertex)));
    }

    internal static void AssureImplementations()
    {
        foreach (Type implementation in GetImplementations())
            if (!Has(implementation))
                throw new NotImplementedException($"{implementation} does not provide {nameof(VertexFormatStore)} with Implementation");
    }
}

}