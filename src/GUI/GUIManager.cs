using UnityEngine;
using UnityEngine.UI;

public static class GUIManager {
    private static readonly int Trigger = Animator.StringToHash("trigger");
    private static Texture _reloadTexture = Resources.Load<Texture2D>("Status/reload");
    private static Texture _meleeAttackTexture = Resources.Load<Texture2D>("Status/melee_attack");
    private static Texture _rangedAttackTexture = Resources.Load<Texture2D>("Status/ranged_attack");
    private static Texture _captureTexture = Resources.Load<Texture2D>("Status/capture");
    private static Texture _defendTexture = Resources.Load<Texture2D>("Status/defend");
    private static Texture _patrolTexture = Resources.Load<Texture2D>("Status/patrol");
    private static Texture _healTexture = Resources.Load<Texture2D>("Status/heal");
    private static Texture _escapeTexture = Resources.Load<Texture2D>("Status/escape");
    private static Texture _idleTexture = Resources.Load<Texture2D>("Status/idle");
    private static Texture _deadTexture = Resources.Load<Texture2D>("Status/dead");
    private static Texture _manualTexture = Resources.Load<Texture2D>("Status/manual");
    private static Texture _roamTexture = Resources.Load<Texture2D>("Status/roam");
    private static Texture _scoutMeleeKill = Resources.Load<Texture2D>("Kill Feed/scout_melee_kill");
    private static Texture _scoutRangedKill = Resources.Load<Texture2D>("Kill Feed/scout_ranged_kill");
    private static Texture _heavyMeleeKill = Resources.Load<Texture2D>("Kill Feed/heavy_melee_kill");
    private static Texture _heavyRangedKill = Resources.Load<Texture2D>("Kill Feed/heavy_ranged_kill");
    private static Texture _medicMeleeKill = Resources.Load<Texture2D>("Kill Feed/medic_melee_kill");
    private static Texture _medicRangedKill = Resources.Load<Texture2D>("Kill Feed/medic_ranged_kill");
    private static Texture _sniperMeleeKill = Resources.Load<Texture2D>("Kill Feed/sniper_melee_kill");
    private static Texture _sniperRangedKill = Resources.Load<Texture2D>("Kill Feed/sniper_ranged_kill");
    private static GameObject _floatingText = Resources.Load("FloatingText") as GameObject;

    public static void TriggerAnimation(Animator animator) {
        animator.SetTrigger(Trigger);
    }

    public static void UpdateStateIcon(RawImage statusImage, State currentState) {
        Texture texture = null;
        switch (currentState.ToString()) {
            case "Reload":
                texture = _reloadTexture;
                break;
            case "MeleeAttack":
                texture = _meleeAttackTexture;
                break;
            case "RangedAttack":
                texture = _rangedAttackTexture;
                break;
            case "Dead":
                texture = _deadTexture;
                break;
            case "Defend":
                texture = _defendTexture;
                break;
            case "Capture":
                texture = _captureTexture;
                break;
            case "Idle":
                texture = _idleTexture;
                break;
            case "Heal":
                texture = _healTexture;
                break;
            case "Patrol":
                texture = _patrolTexture;
                break;
            case "Escape":
                texture = _escapeTexture;
                break;
            case "User":
                texture = _manualTexture;
                break;
            case "Roam":
                texture = _roamTexture;
                break;
        }
        statusImage.texture = texture;
    }

    public static void UpdateKillIcon(RawImage icon, NPC.UnitType unitType, bool melee) {

        switch (unitType) {
            case NPC.UnitType.Scout:
                if (melee)
                    icon.texture = _scoutMeleeKill;
                else
                    icon.texture = _scoutRangedKill;
                break;
            case NPC.UnitType.Heavy:
                if (melee)
                    icon.texture = _heavyMeleeKill;
                else
                    icon.texture = _heavyRangedKill;
                break;
            case NPC.UnitType.Medic:
                if (melee)
                    icon.texture = _medicMeleeKill;
                else
                    icon.texture = _medicRangedKill;
                break;
            case NPC.UnitType.Sniper:
                if (melee)
                    icon.texture = _sniperMeleeKill;
                else
                    icon.texture = _sniperRangedKill;
                break;
        }
        icon.SetNativeSize();
    }

    public static void ShowHealthChange(NPC npc, int amount) {
        var npcTransform = npc.transform;
        var go = Object.Instantiate(_floatingText, npcTransform.position, Quaternion.Euler(90, 0, 0), npcTransform);
        var textMesh = go.GetComponent<TextMesh>();
        textMesh.text = amount.ToString();
        if (amount < 0) 
            textMesh.color = new Color(0.82f, 0.27f, 0.27f);
        else 
            textMesh.color = new Color(0.19f, 0.82f, 0.16f);
    }

}
