using UnityEngine;

namespace Scream2D.Interfaces
{
    public interface IScreamListener
    {
        void OnScreamHit(Vector2 sourcePosition, float power);
    }
}
