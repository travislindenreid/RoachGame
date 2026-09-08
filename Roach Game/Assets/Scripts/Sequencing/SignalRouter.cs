using UnityEngine;

public class SignalRouter : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private Renderer[] _apartmentRenderers;
    [SerializeField] private Vector3 _bubbleZoomOffset = Vector3.zero;
    [SerializeField] private Transform _bubble;
    [SerializeField] private Renderer _bubbleRenderer;
    [SerializeField] private float _lookAtBubbleSpeed = 1.0f;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    // timeline signal callback
    public void PlayMusicBackwards ()
    {
        AudioController._Instance.PlayMusicBackwards();
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartApartmentDissolve ()
    {
        PropEffectsController._Instance.StartApartmentDoorDissolve(_apartmentRenderers);
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void EndSequence ()
    {
        SequenceController._Instance.EndCurrentSequence();
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void FadeOut ()
    {
        PostEffectController._Instance.StartFadeOut();
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartFadeIn ()
    {
        PostEffectController._Instance.StartFadeIn();
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartMainLightFadeIn ()
    {
        LightingController._Instance.FadeInMainLight();
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void AnimateBubbleZoomIn ()
    {
        CameraCinematics._Instance.CameraZoomIn(_bubble, _bubbleZoomOffset);
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void CameraZoomOut ()
    {
        CameraCinematics._Instance.AnimateRoachZoomOut();
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartBubbleDissolve ()
    {
        PropEffectsController._Instance.StartDissolve(_bubbleRenderer);
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void PlayerLookAtBubble ()
    {
        Player._Instance.TurnAndLookAt(_bubble, _lookAtBubbleSpeed);
    }
}
