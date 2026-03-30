using UnityEngine;

public class BeatToPulse : MonoBehaviour
{
    [SerializeField] private float _pulseScale = 1.25f;
    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void PulseScale()
    {
        transform.localScale = _originalScale * _pulseScale;
    }
    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.deltaTime * 5f);
    }
}
