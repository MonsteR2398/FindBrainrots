using UnityEngine;
using UnityEngine.UI;

public class GymStatPopupPool : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GymStatPopupView popupPrefab;
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private int poolSize = 3;

    [Header("Animation")]
    [SerializeField] private float inTime = 0.10f;
    [SerializeField] private float outTime = 0.45f;
    [SerializeField] private float floatUp = 40f;

    private GymStatPopupView[] _pool;
    private int _cursor;

    private void Awake()
    {
        poolSize = Mathf.Clamp(poolSize, 1, 5);
        _pool = new GymStatPopupView[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            var v = Instantiate(popupPrefab, spawnPoint);
            v.gameObject.SetActive(false);
            _pool[i] = v;
        }
    }

    public void Show(float delta)
    {
        var v = _pool[_cursor];
        _cursor = (_cursor + 1) % _pool.Length;
        v.Play(delta, inTime, outTime, floatUp);
    }
}