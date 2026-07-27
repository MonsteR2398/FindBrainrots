using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera _camera;
    public bool normalFacing;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        // Проверяем, есть ли камера
        if (_camera != null)
        {
            if (normalFacing)
            {
                // Поворачиваем объект к камере с дополнительным поворотом на 180 градусов
                Vector3 directionToCamera = _camera.transform.position - transform.position;
                Quaternion rotationToCamera = Quaternion.LookRotation(directionToCamera);
                transform.rotation = rotationToCamera * Quaternion.Euler(0, 180, 0);
            }
            else
            {
                transform.LookAt(_camera.transform);
            }
        }
    }
}
