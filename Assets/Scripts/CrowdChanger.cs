using UnityEngine;

public class CrowdChanger : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] crowdSprites;
    private int currentSpriteIndex;
    private int lastBeat = -1;


    private void Start()
    {
        currentSpriteIndex = Random.Range(0, crowdSprites.Length);
        spriteRenderer.sprite = crowdSprites[currentSpriteIndex];
    }

    private void Update()
    {
        if (IsNewBeat())
        {
            ChangeCrowd();
        }
    }

    void ChangeCrowd()
    {
        currentSpriteIndex = (currentSpriteIndex + 1) % crowdSprites.Length;
        spriteRenderer.sprite = crowdSprites[currentSpriteIndex];
    }

    bool IsNewBeat()
    {
        int currentBeat = RhythmManager.Instance.GetCurrentBeat();

        if (currentBeat != lastBeat)
        {
            lastBeat = currentBeat;
            return true;
        }

        return false;
    }
}
