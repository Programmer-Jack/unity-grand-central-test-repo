using UnityEngine;

public class YKillThreshold : MonoBehaviour
{
    [SerializeField] private int _yThreshold = -25;
    /// <summary>
    /// Set a custom spawnpoint. If none selected, then
    /// spawnpoint will be the player's location at the start of runtime
    /// </summary>
    [SerializeField] private Transform _spawnpointOverride;
    private Vector3 _cameraLocalRotationRespawn;

    private void Start()
    {
        if (_spawnpointOverride == null)
        {
            _spawnpointOverride = new GameObject("Spawnpoint").transform; // Instantiate new GameObject
            _spawnpointOverride.position = transform.position; // Copy player transform values
            _spawnpointOverride.rotation = transform.rotation;

            _cameraLocalRotationRespawn = Camera.main.transform.localEulerAngles;
        }
    }

    private void FixedUpdate()
    {
        if (transform.position.y < _yThreshold)
        {
            transform.position = _spawnpointOverride.position;
            transform.rotation = _spawnpointOverride.rotation;
            Camera.main.transform.localEulerAngles = _cameraLocalRotationRespawn;
        }
    }
}
