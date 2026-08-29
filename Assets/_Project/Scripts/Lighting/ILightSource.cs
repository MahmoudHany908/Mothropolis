using UnityEngine;

namespace Mothropolis.Lighting
{
    public interface ILightSource
    {
        float Radius { get; }
        int MothPoolWeight { get; }
        float ExposureFillRate { get; }
        bool IsActive { get; }
        Vector2 Position { get; }
    }
}
