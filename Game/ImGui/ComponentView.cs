using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Leopotam.Ecs;

namespace Lib
{

public class ComponentView
{
    private Type[] _componentTypes = null!;
    private object[] _componentValues = null!;

    public void Render(ref EcsEntity entity)
    {
        entity.GetComponentTypes(ref _componentTypes);
        entity.GetComponentValues(ref _componentValues);
        for (int index = 0; index < _componentTypes.Length; index++)
        {
            Type componentType = _componentTypes[index];
            object invoke = _componentValues[index];

            ImGui.TextColored(new Vector4(0.4f, 1f, 0.4f, 1f), componentType.Name);
            foreach (MemberInfo memberInfo in
                componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Cast<MemberInfo>()
                    .Union(
                        componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ))
            {
                string? typeString = memberInfo.Name;
                object? value = null;

                bool isPrivate = false;
                if (memberInfo is PropertyInfo propertyInfo)
                {
                    value = propertyInfo.GetValue(invoke);
                    isPrivate = propertyInfo?.GetMethod?.IsPublic ?? false;
                }
                else if (memberInfo is FieldInfo fieldInfo)
                {
                    if (Attribute.IsDefined(memberInfo, typeof(CompilerGeneratedAttribute)))
                        continue;

                    value = fieldInfo.GetValue(invoke);
                    isPrivate = fieldInfo.IsPrivate;
                }


                if (typeString != "Value")
                    ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), $"{(isPrivate ? "(private) " : "")} {typeString}");

                string? desc = $"    {value}";
                ImGui.Text(desc);
                // Console.WriteLine($"{typeString} {desc}");
            }
        }
    }
}

}