using UnityEngine;
using DG.Tweening;

public class GhostTrail : MonoBehaviour
{
    private PlayerController pc;
    private Animator anim;
    private SpriteRenderer sr;
    private readonly float directionOffset = 153.0f;
    public Transform ghostsParent;
    public GameObject player;
    public Color trailColor;
    public Color fadeColor;
    public float ghostInterval;
    public float fadeTime;

    private void Start()
    {
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        anim = FindObjectOfType<Animator>();
        pc = FindObjectOfType<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void ShowGhost()
    {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < ghostsParent.childCount; i++)
        {
            // Debug.Log($"Dashing {i}/3");
            Transform currentGhost = ghostsParent.GetChild(i);
            SpriteRenderer currentSr = currentGhost.GetComponent<SpriteRenderer>();
            s.AppendCallback(()=> currentGhost.position = pc.transform.position);
            s.AppendCallback(() => currentSr.flipX = !pc.xInputIsRight);
            s.AppendCallback(()=>currentSr.sprite = pc.sr.sprite);
            s.Append(currentSr.material.DOColor(trailColor, 0));
            s.AppendCallback(() => FadeSprite(currentGhost));
            s.AppendInterval(ghostInterval);
        }
    }

    public void FadeSprite(Transform current)
    {
        current.GetComponent<SpriteRenderer>().material.DOKill();
        current.GetComponent<SpriteRenderer>().material.DOColor(fadeColor, fadeTime);
    }

}
