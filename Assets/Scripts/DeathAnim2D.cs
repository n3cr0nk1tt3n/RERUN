using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class DeathAnim2D : MonoBehaviour
{
    Animator anim;
    SpriteRenderer sr;

    public float extraLifetime = 0.05f; // small buffer after animation

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void InitializeFrom(SpriteRenderer source, bool facingLeft)
    {
        if (source != null)
        {
            sr.sortingLayerID = source.sortingLayerID;
            sr.sortingOrder   = source.sortingOrder + 1;
        }
        sr.flipX = facingLeft;

        var ctrl = anim.runtimeAnimatorController;
        if (ctrl != null && ctrl.animationClips.Length > 0)
        {
            float len = ctrl.animationClips[0].length;
            Destroy(gameObject, len + extraLifetime);
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }
}