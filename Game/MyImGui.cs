#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using ImGuiNET;
using Leopotam.Ecs;
using Lib.Components;
using Lib.Render;

namespace Lib
{

public class MyImGui : IEcsRunSystem
{
    private EcsFilter<Position> _filter = null!;
    private UpdateInfo _updateInfo = null!;
    private RenderInfo _renderInfo = null!;

    private string[] _buffer = new string[0];

    public void Run()
    {
        ImGui.Begin("Update Info");
        ImGui.Text($"Update CPU ms: {_updateInfo.CPUTime.Milliseconds}");
        ImGui.Text($"Render CPU ms: {_renderInfo.Delta.Milliseconds}");
        ImGui.Text($"FPS: {_renderInfo.FPS.ToString("F2")}");
        ImGui.Text("GC gen1: " + _updateInfo.GarbageGen0Count.ToString());
        ImGui.Text("GC gen2: " + _updateInfo.GarbageGen1Count.ToString());
        ImGui.Text("GC gen3: " + _updateInfo.GarbageGen2Count.ToString());
        ImGui.Text("DrawCalls: " + _renderInfo.DrawCalls);
        ImGui.End();

        ImGui.Begin("Entities");

        int entitiesCount = _filter.GetEntitiesCount();

        if (entitiesCount > _buffer.Length)
            _buffer = new string[entitiesCount];

        for (int i = 0; i < entitiesCount; ++i)
        {
            EcsEntity ecsEntity = _filter.GetEntity(i);
            string name = ecsEntity.Has<Name>() ? ecsEntity.Get<Name>().Value : "Unnamed entity";
            _buffer[i] = name;
        }


        int currentItem = 1;
        ImGui.ListBox("", ref currentItem, _buffer, _buffer.Length);

        ImGui.End();
    }
}

public struct Name
{
    public Name(string value) => Value = value;

    public string Value;
}

}