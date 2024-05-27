using UnityEngine;
using UnityEngine.Events;

public class DetectorController : MonoBehaviour
{
    public UnityEvent<Collider2D> OnDetect, OnUndetect;

    [SerializeField]
    string[] _layers;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        for(int i = 0; i < _layers.Length; ++i)
            if(LayerMask.LayerToName(collision.gameObject.layer) == _layers[i])
                OnDetect?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        for (int i = 0; i < _layers.Length; ++i)
            if (LayerMask.LayerToName(collision.gameObject.layer) == _layers[i])
                OnUndetect?.Invoke(collision);
    }
}
