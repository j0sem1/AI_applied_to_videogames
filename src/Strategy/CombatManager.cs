using UnityEngine;

public static class CombatManager {

    // Initialized from the GUIKillFeed itself so this class continues to stay static
    public static GUIKillFeed GuiKillFeed;
    
    public static void MeleeAttack(NPC attacker, NPC target) {
        int crit = Random.Range(0, 51);
        if (crit == 50) {
            // Critical attack
            target.Health -= attacker.MeleeCritDamage;
            GUIManager.TriggerAnimation(target.CriticalHitAnimator);
            if (target.Health == 0 && !target.IsDead)
                GuiKillFeed.AddKill(attacker, target, true, true);
        }
        else {
            // Default attack
            target.Health -= attacker.MeleeDamage;
            if (target.Health == 0 && !target.IsDead)
                GuiKillFeed.AddKill(attacker, target, true, false);
        }
    }

    public static void RangedAttack(NPC attacker, NPC target) {
        int crit = Random.Range(0, 51);
        if (crit == 50) {
            // Critical attack
            if (target.Team == attacker.Team) {
                target.Health += attacker.RangedCritDamage * attacker.AmmoPerShot;
            }
            else {
                target.Health -= attacker.RangedCritDamage * attacker.AmmoPerShot;
            }
            attacker.CurrentAmmo -= attacker.AmmoPerShot;
            GUIManager.TriggerAnimation(target.CriticalHitAnimator);
            if (target.Health == 0 && !target.IsDead)
                GuiKillFeed.AddKill(attacker, target, false, true);
        }
        else {
            // Default attack
            if (target.Team == attacker.Team) {
                target.Health += attacker.RangedDamage * attacker.AmmoPerShot;
            }
            else {
                target.Health -= attacker.RangedDamage * attacker.AmmoPerShot;
            }
            attacker.CurrentAmmo -= attacker.AmmoPerShot;
            if (target.Health == 0 && !target.IsDead)
                GuiKillFeed.AddKill(attacker, target, false, false);
        }
    }

    public static void Restart() {
        GuiKillFeed.Restart();
    }

}
