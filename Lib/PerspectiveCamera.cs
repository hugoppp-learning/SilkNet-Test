using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;

namespace Lib
{

public class CameraViewUpdateSystem : IEcsRunSystem
{
    private EcsFilter<OrthoCamera> _filterOrtho = null!;
    private EcsFilter<PerspectiveCamera> _filterPerspective = null!;

    public void Run(double delta)
    {
        UpdateCameras(in _filterOrtho);
        UpdateCameras(in _filterPerspective);
    }

    private void UpdateCameras<T>(in EcsFilter<T> filter) where T : struct, ICamera
    {
        foreach (int i in filter)
        {
            ref T cam = ref filter.Get1(i);
            if (cam.Active)
            {
                Vector3 position = _filterOrtho.GetEntity(i).Get<Position>().Value;

                cam.View = Matrix4x4.Identity;
                cam.View = Matrix4x4.CreateLookAt(position, new(0, 0, 1), new(0, 1, 0));
            }
        }
    }
}

interface ICamera
{
    Matrix4x4 View { get; set; }
    Matrix4x4 ViewProjection { get; }
    bool Active { get; set; }
}

public struct PerspectiveCamera : ICamera
{
    Matrix4x4 ICamera.View { get; set; }

    public Matrix4x4 ViewProjection { get; }
    public bool Active { get; set; }
}

public struct OrthoCamera : ICamera
{
    public float With { get => _with; set { _with = value; CalculateProjection(); } }

    public float Height { get => _height; set { _height = value; CalculateProjection(); } }

    public float Far { get => _far; set { _far = value; CalculateProjection(); } }

    public float Near { get => _near; set { _near = value; CalculateProjection(); } }

    public bool Active { get; set; }
    public Matrix4x4 ViewProjection => View * Projection;

    /// <summary>
    /// Updated by <see cref="CameraViewUpdateSystem"/>
    /// </summary>
    public Matrix4x4 View { get; set; }

    private Matrix4x4 Projection;

    private float _with;
    private float _height;
    private float _far;
    private float _near;

    private OrthoCamera CalculateProjection()
    {
        Projection = Matrix4x4.CreateOrthographic(_with, _height, _near, _far);
        return this;
    }

    private static int _counter = 1;

    public OrthoCamera(float with, float height, float far, float near, bool active) : this()
    {
        _with = with;
        _height = height;
        _far = far;
        _near = near;
        Active = active;
    }

    public static EcsEntity Create(EcsWorld world, float width, float height, float near, float far, bool active)
    {
        return world.NewEntity()
            .Replace(new Position(new Vector3(0, 0, 10)))
            .Replace(new OrthoCamera(active: true, with: width, height: height, near: near, far: far).CalculateProjection())
            .Replace(new Name($"Ortho Camera {_counter++}"));
    }
}

public static class EscWorldExtension
{
    public static EcsEntity AddCamera(this EcsWorld world,
        float width = 800,
        float height = 600f,
        bool active = true,
        float near = 0.1f,
        float far = 100f)
    {
        return OrthoCamera.Create(world, width, height, near, far, active);
    }
}

}