using UnityEngine;

// Game sounds driven by the plane's state: power charge, launch, stall, landing/crash,
// an occasional car horn, and menu/flight music. Added by LaunchPowerHUD.
public class GameAudio : MonoBehaviour
{
    public PaperPlaneController plane;
    public LaunchPowerHUD hud;

    AudioSource charge;
    bool wasFlying, wasLanded, wasStalled, wasSweet;
    float hornTimer;

    void Start()
    {
        charge = SoundManager.Loop("power_charge_loop");
        hornTimer = Random.Range(6f, 14f);
    }

    void Update()
    {
        AudioListener.pause = hud.Paused;   // in-game sounds stop with the game; UI sounds and music don't
        SoundManager.PlayMusic(hud.Started ? "music_flight_loop" : "music_menu_loop");
        float dt = Time.unscaledDeltaTime;

        // Stretching rubber band while Space is held; higher as the power rises, a tick at the sweet spot.
        charge.volume = Mathf.MoveTowards(charge.volume, plane.IsCharging ? 0.45f * SoundManager.EffectsMix : 0f, 4f * dt);
        charge.pitch = 0.8f + 0.7f * plane.Charge;
        bool sweet = plane.IsCharging && plane.Charge >= 0.9f;
        if (sweet && !wasSweet) SoundManager.Play("power_sweet", 0.6f);
        wasSweet = sweet;

        if (plane.IsFlying && !wasFlying) SoundManager.Play("launch_whoosh", 0.8f);
        wasFlying = plane.IsFlying;

        if (plane.Stalled && !wasStalled) SoundManager.Play("stall_warning", 0.7f);
        wasStalled = plane.Stalled;

        if (plane.HasLanded && !wasLanded) SoundManager.Play(plane.Crashed ? "crash_crumple" : "land_slide", 0.9f);
        wasLanded = plane.HasLanded;


        // Now and then a car below honks.
        if (plane.IsFlying && (hornTimer -= Time.deltaTime) <= 0f)
        {
            hornTimer = Random.Range(8f, 18f);
            SoundManager.Play("car_horn", 0.3f, Random.Range(0.9f, 1.1f));
        }
    }
}
