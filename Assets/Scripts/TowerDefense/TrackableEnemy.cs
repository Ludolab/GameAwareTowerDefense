using Core.Health;
using Core.Utilities;
using GameAware;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TowerDefense.Agents;
using TowerDefense.Economy;
using UnityEngine;

public class TrackableEnemy : MetaDataTrackable {
    private static Dictionary<string, int> ENEMY_TYPE_COUNTERS = new Dictionary<string, int>();

    private static string GenerateObjectKey(string enemyType) {
        if (ENEMY_TYPE_COUNTERS.ContainsKey(enemyType)) {
            ENEMY_TYPE_COUNTERS[enemyType]++;
        }
        else {
            ENEMY_TYPE_COUNTERS[enemyType] = 0;
        }
        return enemyType + ENEMY_TYPE_COUNTERS[enemyType].ToString();
    }

    private Agent agent;
    private LootDrop lootDrop;
    private bool tracking = false;
    public string EnemyType;

    // Start is called before the first frame update
    override protected void Start() {
    //    objectKey 
        agent = GetComponent<Agent>();
        lootDrop = GetComponent<LootDrop>();

        agent.initialized += OnInitialized;
        agent.removed += OnDiedOrRemoved;
        agent.died += OnDiedOrRemoved;
        objectKey = GenerateObjectKey(EnemyType);

        frameType = MetaDataFrameType.Inbetween;
        screenRectStyle = ScreenSpaceReference.Collider;

        base.Start();
        tracking = true;
    }

    private void OnDiedOrRemoved(DamageableBehaviour obj) {
        if (tracking) {
            MetaDataTracker.Instance.RemoveTrackableObject(this);
            tracking = false;
        }
    }

    private void OnInitialized() {
        if (!tracking) {
            objectKey = GenerateObjectKey(EnemyType);
            MetaDataTracker.Instance.AddTrackableObject(this);
            tracking = true;
        }
    }

    public override JObject KeyFrameData() {
        JObject ret = base.KeyFrameData();
        ret["health"] = agent.configuration.currentHealth;
        ret["maxHealth"] = agent.configuration.maxHealth;
        ret["value"] = lootDrop.lootDropped;
        ret["type"] = EnemyType;
        //attackEnabled <- their attack affector is only turned on if their path is blocked
        //currentTarget <- tower they're currently targeting or the base
        //base damage? <- seems like they can have different damage to the base
        return ret;
    }

    public override JObject InbetweenData() {
        JObject ret = base.InbetweenData();
        ret["health"] = agent.configuration.currentHealth;
        return ret;
    }
}
