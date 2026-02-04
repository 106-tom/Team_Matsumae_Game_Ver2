using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionManager : MonoBehaviour
{
    public static MotionManager Instance { get; private set; }

    [SerializeField] private Attack _attack;
    [SerializeField] private Block _block;
    [SerializeField] private Break _Break;
    [SerializeField] private Buff _buff;
    [SerializeField] private Draw _draw;
    [SerializeField] private Heal _heal;
    [SerializeField] private Summon _summon;
    [SerializeField] private Spell _spell;
    public Attack attack => _attack;
    public Block block => _block;
    public Break _break => _Break;
    public Buff buff => _buff;
    public Draw draw => _draw;
    public Heal heal => _heal;
    public Summon summon => _summon;
    public Spell spell => _spell;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
}
