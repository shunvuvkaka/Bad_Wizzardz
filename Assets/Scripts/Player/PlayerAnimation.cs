using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimation : MonoBehaviour
{
    public Animator playerAnimator;
    public static PlayerAnimation Instance;

    void Awake()
    {
        Instance = this;
    }
    public void Hit()
    {
        playerAnimator.SetTrigger("Hit");
    }
    public void Dead()
    {
        playerAnimator.SetTrigger("Dead");
    }
    public void Spell()
    {
        playerAnimator.SetTrigger("Spell");
    }
    public void Object(bool isSelecting)
    {
        playerAnimator.SetBool("Object", isSelecting);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(PlayerAnimation))]
public class PlayerAnimationEditor : Editor
{
    private bool isSelecting; 
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerAnimation playerAnimation = target as PlayerAnimation;

        EditorGUILayout.Toggle(isSelecting);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Hit"))
            playerAnimation.Hit();
        
        if (GUILayout.Button("Dead"))
            playerAnimation.Dead();
        
        if (GUILayout.Button("Spell"))
            playerAnimation.Spell();
        
        if (GUILayout.Button("Beign"))
            playerAnimation.Object(true);

        if (GUILayout.Button("End"))
            playerAnimation.Object(false);

        EditorGUILayout.EndHorizontal();

    }
}
#endif
