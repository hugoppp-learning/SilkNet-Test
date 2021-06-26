using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lib.Render
{

internal static class AttribPointerStore
{
    private static readonly Dictionary<Type, Action<IHasVertexAttribPointer>> _data = new();

    static AttribPointerStore()
    {
        foreach (Type implementation in GetImplementations())
        {
            const string methodName = "SetLayout";

            MethodInfo? info = implementation.GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (info is null)
                throw new NotImplementedException($"{implementation} missing static Method {methodName}");

            ParameterInfo[] parameterInfos = info.GetParameters().ToArray();
            if (parameterInfos.Length == 0 ||
                parameterInfos.Length > 1 ||
                parameterInfos[0].ParameterType != typeof(IHasVertexAttribPointer))
                throw new NotImplementedException($"{implementation} Method {methodName} has incorrect signature");

            var action = (Action<IHasVertexAttribPointer>) Delegate.CreateDelegate(typeof(Action<IHasVertexAttribPointer>), info);
            _data.Add(implementation, action);
        }
    }

    internal static void AddVertex(Type type, Action<IHasVertexAttribPointer> setterMethod)
    {
        _data.Add(type, setterMethod);
    }

    internal static bool Has(Type type)
    {
        return _data.ContainsKey(type);
    }

    internal static void Set(Type vertexType, IHasVertexAttribPointer hasVertexAttribPointer)
    {
        _data[vertexType].Invoke(hasVertexAttribPointer);
    }


    private static IEnumerable<Type> GetImplementations()
    {
        return Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(IVertex)));
    }

    internal static void AssureImplementations()
    {
        foreach (Type implementation in GetImplementations())
            if (!Has(implementation))
                throw new NotImplementedException($"{implementation} does not provide {nameof(AttribPointerStore)} with Implementation");
    }
}

}