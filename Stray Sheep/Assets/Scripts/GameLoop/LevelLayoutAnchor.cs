using UnityEngine;

public class LevelLayoutAnchor : MonoBehaviour
{
    [Tooltip("The index of this layout in the level progression.")]
    public int layoutIndex;
    
    [Tooltip("The center point of this layout for camera focus. If not set, uses this transform's position.")]
    [SerializeField] private Transform layoutCenter;
    
    [Tooltip("Spawn points specific to this layout. If empty, uses the spawner's default spawn points.")]
    [SerializeField] private Transform[] layoutSpawnPoints;

    public Vector3 GetLayoutCenterPosition()
    {
        if (layoutCenter != null)
            return layoutCenter.position;
        return transform.position;
    }

    public Transform[] GetLayoutSpawnPoints()
    {
        return layoutSpawnPoints;
    }
}
