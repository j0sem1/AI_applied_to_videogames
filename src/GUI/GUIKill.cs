using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GUIKill : MonoBehaviour {

    // Defines a single kill and all its visual elements
    [SerializeField] private RectTransform _left;
    [SerializeField] private RectTransform _center;
    [SerializeField] private RectTransform _right;
    [SerializeField] private Text _killer;
    [SerializeField] private RawImage _icon;
    [SerializeField] private Text _victim;
    [SerializeField] private RectTransform _crit;
    private Color _redTextColor = new Color(0.82f, 0.27f, 0.27f);
    private Color _bluTextColor = new Color(0.44f, 0.59f, 0.67f);
    [SerializeField] private RectTransform _self;
    public float Width => _self.rect.width;
    
    public void Initialize(NPC killer, NPC victim, bool melee, bool crit) {
        _killer.text = killer.Name;
        _victim.text = victim.Name;

        if (killer.Team == NPC.UnitTeam.Red) {
            _killer.color = _redTextColor;
            _victim.color = _bluTextColor;
        }
        else {
            _killer.color = _bluTextColor;
            _victim.color = _redTextColor;
        }
        
        GUIManager.UpdateKillIcon(_icon, killer.Type, melee);

        float padding = 8f;
        float killerWidth = _killer.preferredWidth;
        float iconWidth = _icon.rectTransform.rect.width;
        float killedWidth = _victim.preferredWidth;
        float sideWidth = _left.rect.width;
        float centerWidth = killerWidth + iconWidth + 2*padding + killedWidth;
        
        _self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, centerWidth+sideWidth*2);
        _center.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, centerWidth);
        _killer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, killerWidth);
        _victim.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, killedWidth);
        
        Vector3 leftPosition = new Vector3(-centerWidth/2 - sideWidth/2 + 1, 0f, 0f);
        Vector3 rightPosition = new Vector3(centerWidth/2 + sideWidth/2 - 1, 0f, 0f);
        Vector3 killerPosition = new Vector3(-centerWidth/2 + killerWidth/2, 0f, 0f);
        Vector3 iconPosition = new Vector3(-centerWidth/2 + killerWidth + iconWidth/2 + padding, 0f, 0f);
        Vector3 killedPosition = new Vector3(-centerWidth/2 + killerWidth + iconWidth + killedWidth/2 + 2*padding, 0f, 0f);
        
        _left.localPosition = leftPosition;
        _right.localPosition = rightPosition;
        _killer.rectTransform.localPosition = killerPosition;
        _icon.rectTransform.localPosition = iconPosition;
        _victim.rectTransform.localPosition = killedPosition;
        _crit.localPosition = iconPosition;
        _crit.gameObject.SetActive(crit);
        
        gameObject.SetActive(true);
    }

}
