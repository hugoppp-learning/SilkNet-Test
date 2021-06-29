using System;
using ImGuiNET;
using Leopotam.Ecs;
using Lib.Components;

namespace Lib
{

public class ImGuiEntityList : IEcsRunFixedSystem, IEcsRunSystem
#if DEBUG
        , IEcsWorldDebugListener
#endif
{
    public int EntityCount => Math.Min(EntityBuffer.Length, _entities.GetEntitiesCount());
    public string[] EntityBuffer = new string[0];

    public bool EntityBufferDirty = true;

#if DEBUG
#else
    private int _dirtCounter;
    private const int UpdateListEveryXWorldUpdates = 10;
#endif

    private readonly EcsFilter<Position> _entities = null!;
    private Utf8Buffer quadRendererCount = new("QuadRenderer Count: ", 32);

    private int _entitiesCurrentSelectedIndex;

    public void Run(double delta)
    {
        ImGui.Begin("Entities");
        using (quadRendererCount.Put(EntityCount))
            ImGuiExtensions.Text(quadRendererCount);
        ImGui.ListBox("", ref _entitiesCurrentSelectedIndex, EntityBuffer, EntityCount);
        ImGui.End();
    }

    public void RunFixed(double delta)
    {
        if (EntityBufferDirty)
        {
            UpdateEntityBuffer();
            EntityBufferDirty = false;
        }
    #if DEBUG
    #else
        _dirtCounter = (_dirtCounter + 1) % UpdateListEveryXWorldUpdates;
        EntityBufferDirty = _dirtCounter == 0;
    #endif
    }

    private void UpdateEntityBuffer()
    {
        int entitiesCount = _entities.GetEntitiesCount();
        if (entitiesCount > EntityBuffer.Length)
            EntityBuffer = new string[entitiesCount];
        for (int i = 0; i < entitiesCount; ++i)
        {
            EcsEntity ecsEntity = _entities.GetEntity(i);
            string name = ecsEntity.Has<Name>() ? ecsEntity.Get<Name>().Value : "Unnamed entity";
            EntityBuffer[i] = name;
        }
    }

#if DEBUG
        public void OnEntityCreated(EcsEntity entity)
        {
            EntityBufferDirty = true;
        }

        public void OnEntityDestroyed(EcsEntity entity)
        {
            EntityBufferDirty = true;
        }

        public void OnFilterCreated(EcsFilter filter)
        {
        }

        public void OnComponentListChanged(EcsEntity entity)
        {
        }

        public void OnWorldDestroyed(EcsWorld world)
        {
        }
#endif
}

}