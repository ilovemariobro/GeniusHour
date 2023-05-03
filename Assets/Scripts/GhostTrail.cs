using UnityEngine;
using DG.Tweening;

public class GhostTrail : MonoBehaviour
{
    private PlayerController move;
    private Animator anim;
    private SpriteRenderer sr;
    public Transform ghostsParent;
    public Color trailColor;
    public Color fadeColor;
    public float ghostInterval;
    public float fadeTime;

    private void Start()
    {
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        anim = FindObjectOfType<Animator>();
        move = FindObjectOfType<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void ShowGhost()
    {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < ghostsParent.childCount; i++)
        {
            Transform currentGhost = ghostsParent.GetChild(i);
            SpriteRenderer currentSr = currentGhost.GetComponent<SpriteRenderer>();
            s.AppendCallback(()=> currentGhost.position = move.transform.position);
            s.AppendCallback(() => currentSr.flipX = move.sr.flipX);
            s.AppendCallback(()=>currentSr.sprite = move.sr.sprite);
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
