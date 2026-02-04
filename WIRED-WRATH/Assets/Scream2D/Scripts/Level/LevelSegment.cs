using UnityEngine;

namespace Scream2D.Level
{
    public class LevelSegment : MonoBehaviour
    {
        [Tooltip("The point where the NEXT segment should connect to.")]
        public Transform EndPoint;

        private void OnDrawGizmos()
        {
            if (EndPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(EndPoint.position, 0.5f);
                Gizmos.DrawLine(transform.position, EndPoint.position);
            }
        }
    }
}
