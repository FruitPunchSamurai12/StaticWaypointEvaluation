using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using System;
using System.Linq;

public class Enemy : MonoBehaviour
{
    [SerializeField]Explosion deathEffect;
    [SerializeField]Weapon enemyStartWeapon;  
    [SerializeField]BehaviorTree bt;
    public float attackRange = 10f;
    public float minHideTime = 1f;
    public float maxHideTime = 3f;
    public float healthPercentageThresholdToHide = 30f;
    WeaponUser weaponUser;
    List<ISensor> sensors = new List<ISensor>();
    Dictionary<GameObject, GameObject> bestWaypointForAttackForEachTarget;
    Health health;
    CellTracker cellTracker;
    Blackboard blackboard;
    public bool HasToken { get; private set; }
    bool alreadyAskedForToken = false;
    bool isDead;

    [SerializeField] MeshRenderer rend;
    [SerializeField] Material noToken;
    [SerializeField] Material yesToken;

    private void Awake()
    {
        cellTracker = GetComponent<CellTracker>();
        health = GetComponent<Health>();
        weaponUser = GetComponent<WeaponUser>();
        blackboard = GetComponent<Blackboard>();
        weaponUser.onNeedReload += HandleNeedReload;
        weaponUser.onReloadComplete+=HandleReloadComplete;
        bt.SetVariableValue("AttackRange", attackRange);
        if (enemyStartWeapon != null)
        {
            SetStartWeapon(enemyStartWeapon);
        }
        bt.SetVariableValue("Health", health.maxHealth);
        bt.SetVariableValue("MinHideTime", minHideTime);
        bt.SetVariableValue("MaxHideTime", maxHideTime);
        health.onHealthModified += HandleDamageTaken;
        health.onDeath += HandleDeath;
        
        foreach (var sensor in GetComponents<ISensor>())
        {
            sensors.Add(sensor);
        }
    }

    public void SetStartWeapon(Weapon weapon)
    {
        weaponUser.EquipWeapon(weapon);
        bt.SetVariableValue("EquippedWeapon", weapon);
        bt.SetVariableValue("ReloadTime", weapon.reloadTime);
    }

    private void Start()
    {
        bestWaypointForAttackForEachTarget = new Dictionary<GameObject,GameObject>();
        EnemyManager.Instance.NewEnemy(this);
    }

    private void FixedUpdate()
    {
        foreach (var sensor in sensors)
        {
            sensor.SenseTarget();
        }
    }

    public void SetPatrolWaypoints(List<GameObject> patrolWaypoints)
    {
        bt.SetVariableValue("BestWaypoints", patrolWaypoints);
    }

    public void TryAmbush(GameObject target)
    {
        EnemyManager.Instance.AskToAmbush(this,target);
    }

    public GameObject GetBestWaypointToAttackTarget(GameObject target)
    {
        var wp = bestWaypointForAttackForEachTarget.ContainsKey(target)?bestWaypointForAttackForEachTarget[target]:null;
        if (wp != null)
            EnemyManager.Instance.AssignWaypoint(cellTracker, wp.GetComponent<Waypoint>());
        return wp;
    }

    public Waypoint GetCoverWaypoint(GameObject targetTakingCoverFrom)
    {
        Waypoint coverWaypoint = null;
        Waypoint targetWaypoint = targetTakingCoverFrom.GetComponent<CellTracker>().closestWaypoint;
        Waypoint myWaypoint = cellTracker.closestWaypoint;
        coverWaypoint = EnemyManager.Instance.GetClosestWaypointToWaypointAWithoutLineOfSightToWaypointB(myWaypoint,targetWaypoint,cellTracker);

        return coverWaypoint==null?null:coverWaypoint;
    }

    public void SetBestWaypointToAttackTarget(GameObject target, GameObject bestWaypointGameObject)
    {
     if (bestWaypointForAttackForEachTarget.ContainsKey(target))
            bestWaypointForAttackForEachTarget[target] = bestWaypointGameObject;
        else
            bestWaypointForAttackForEachTarget.Add(target, bestWaypointGameObject);
    }

    public void SetUpAmbush(Waypoint waypointToGo, Waypoint waypointToLookAt)
    {
        bt.SetVariableValue("WaypointToGo", waypointToGo.gameObject);
        bt.SetVariableValue("WaypointToLookAt", waypointToLookAt.gameObject);
        bt.SetVariableValue("WantToAmbush", true);
    }

    void HandleDamageTaken(GameObject hitInstigator,Vector3 firedFromPosition)
    {
        bt.SetVariableValue("Health", health.currentHealth);
        bt.SetVariableValue("TookDamage", true);
        StopCoroutine(ResetTookDamage());
        StartCoroutine(ResetTookDamage());
        blackboard.SetTarget(hitInstigator);
        bt.SetVariableValue("AttackTarget", hitInstigator);
        bt.SetVariableValue("TargetLastKnownPosition", firedFromPosition);
        bt.SetVariableValue("WantToAmbush", false);
        bt.SetVariableValue("UpdateWaypoint", true);
        if (health.GetPercentage() < healthPercentageThresholdToHide)
        {
            bt.SetVariableValue("NeedCover", true);
            StartCoroutine(GetBackOutThere());
        }
        else
        {
            bt.SetVariableValue("LookForTarget", true);
        }
    }

    public void GetAlerted(GameObject target)
    {
        blackboard.SetTarget(target);
        foreach (var targetSeenMemory in blackboard.targetsSeen)
        {
            if (targetSeenMemory.Key != target && targetSeenMemory.Value.InSight)
                return;
        }
        bt.SetVariableValue("AttackTarget", target);
        bt.SetVariableValue("TargetLastKnownPosition", target.transform.position);
        bt.SetVariableValue("WantToAmbush", false);
        bt.SetVariableValue("InAmbush", false);
        bt.SetVariableValue("UpdateWaypoint", true);
        if (!(bool)bt.GetVariable("NeedCover").GetValue())
            bt.SetVariableValue("LookForTarget", true);
    }

    IEnumerator ResetTookDamage()
    {
        yield return new WaitForSeconds(5f);
        bt.SetVariableValue("TookDamage", false);
    }

    IEnumerator GetBackOutThere()
    {
        float randomWait = UnityEngine.Random.Range(minHideTime, maxHideTime);
        yield return new WaitForSeconds(randomWait);
        bt.SetVariableValue("NeedCover", false);
    }

    void HandleDeath()
    {
        if (!isDead)
        {
            isDead = true;
            bt.SetVariableValue("IsDead", true);
            EnemyManager.Instance.EnemyToDelete(this);
            deathEffect.Get<Explosion>(new Vector3(transform.position.x,1,transform.position.z), Quaternion.identity);
            gameObject.SetActive(false);
            Destroy(gameObject, .5f);
        }
    }




    public void AskForToken()
    {
        if(!HasToken)
        {
            if(!EnemyManager.Instance.AllocateToken(this))
            {
                if(!alreadyAskedForToken)
                {
                    EnemyManager.Instance.onUnallocatedToken += AskForToken;
                    alreadyAskedForToken = true;
                }
            }
        }
    }

    void HandleNeedReload()
    {
        bt.SetVariableValue("NeedReload", true);
        bt.SetVariableValue("NeedCover", true);
        bt.SetVariableValue("UpdateWaypoint", true);
        EnemyManager.Instance.UnAllocateToken(this);
    }

    void HandleReloadComplete()
    {
        bt.SetVariableValue("NeedReload", false);
        bt.SetVariableValue("NeedCover", false);
        bt.SetVariableValue("UpdateWaypoint", true);
    }

    public void GiveToken()
    {
        HasToken = true;
        alreadyAskedForToken = false;
        EnemyManager.Instance.onUnallocatedToken -= AskForToken;
        bt.SetVariableValue("NeedCover", false);
        rend.material = yesToken;
    }
    public void RemoveToken()
    {
        HasToken = false;
        rend.material = noToken;
    }

}
