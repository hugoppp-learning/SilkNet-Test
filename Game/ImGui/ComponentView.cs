using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Leopotam.Ecs;
using Lib.Components;

namespace Lib
{

public class ComponentView
{
    private Type?[] _componentTypes = null!;
    private object?[] _componentValues = null!;

    public void Render(ref EcsEntity entity)
    {
        entity.GetComponentTypes(ref _componentTypes);
        entity.GetComponentValues(ref _componentValues);
        for (int i = 0; i < _componentTypes.Length && _componentTypes[i] != null; i++)
        {
            HandleComponent(ref entity, _componentTypes[i]!, _componentValues[i]!);
        }

        Array.Clear(_componentTypes, 0, entity.GetComponentsCount());
        Array.Clear(_componentValues, 0, entity.GetComponentsCount());
    }

    private void HandleComponent(ref EcsEntity entity, Type componentType, object componentValue)
    {
        ImGui.TextColored(new Vector4(0.4f, 1f, 0.4f, 1f), componentType.Name);
        foreach (MemberInfo componentMemberInfo in
            componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Cast<MemberInfo>()
                .Union(
                    componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ))
        {
            if (!GetInfo(componentMemberInfo, componentValue, out object? fieldValue, out bool isPrivate, out Type fieldType) ||
                fieldValue is null)
                continue;

            string typeString = componentMemberInfo.Name;
            if (typeString != "Value")
                ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), $"{(isPrivate ? "(private) " : "")} {typeString}");

            if (fieldType == typeof(Vector3))
            {
                Vector3 vector3 = (Vector3) fieldValue;
                ImGui.DragFloat3("", ref vector3);

                if (componentMemberInfo is PropertyInfo pi)
                {
                    ImGui.Text("Setting properties not supported");
                }

                //todo make this generic => requires reflection of ref param method
                //https://limbioliong.wordpress.com/2011/07/22/passing-a-reference-parameter-to-type-memberinvoke/
                if (componentMemberInfo is FieldInfo fi && componentType == typeof(Position))
                {
                    SetFieldOfEntity<Position, Vector3>(ref entity, fi, vector3);
                }
            }
            else
            {
                string desc = $"    {fieldValue}";

                ImGui.Text(desc);
            }

            // Console.WriteLine($"{typeString} {desc}");
        }
    }

    private static void SetFieldOfEntity<TComponentType, TValueType>(ref EcsEntity entity, FieldInfo fi, TValueType value)
        where TComponentType : struct
    {
        ref var position = ref entity.Get<TComponentType>();
        TypedReference reference = __makeref(position);
        fi.SetValueDirect(reference, value);
    }

    private static bool GetInfo(MemberInfo memberInfo,
        object componentValue,
        out object? value,
        out bool isPrivate,
        out Type fieldType)
    {
        if (memberInfo is PropertyInfo propertyInfo)
        {
            value = propertyInfo.GetValue(componentValue);
            isPrivate = propertyInfo.GetMethod?.IsPublic ?? false;
            fieldType = propertyInfo.PropertyType;
        }
        else if (memberInfo is FieldInfo fieldInfo)
        {
            value = fieldInfo.GetValue(componentValue);

            isPrivate = fieldInfo.IsPrivate;
            fieldType = fieldInfo.FieldType;

            if (Attribute.IsDefined(memberInfo, typeof(CompilerGeneratedAttribute)))
                return false;
        }
        else throw new ArgumentException();

        return true;
    }

    private void Replace<T>(ref EcsEntity entity, T value)
    {
        MethodInfo? methodInfo = typeof(EcsEntityExtensions).GetMethod(nameof(EcsEntityExtensions.Replace));
        methodInfo.MakeGenericMethod(typeof(T));
        // methodInfo.Invoke()
    }
}

}