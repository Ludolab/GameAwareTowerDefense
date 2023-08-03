using GameAware;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Towers;
using TowerDefense.Affectors;
using Newtonsoft.Json.Linq;
using System;
using ActionGameFramework.Health;
using System.Transactions;
using System.Linq;

public class TrackableTower : MetaDataTrackable {
    private static Dictionary<string, int> TOWER_TYPE_COUNTERS = new Dictionary<string, int>();

    private static string GenerateObjectKey(string enemyType)
    {
        if (TOWER_TYPE_COUNTERS.ContainsKey(enemyType))
        {
            TOWER_TYPE_COUNTERS[enemyType]++;
        }
        else
        {
            TOWER_TYPE_COUNTERS[enemyType] = 0;
        }
        return enemyType + TOWER_TYPE_COUNTERS[enemyType].ToString();
    }


    private static int Tower_Counter = 0;

    public string TowerType;
    private Tower tower;
    private TowerLevel currentTowerLevel;
    private JObject towerStats = new JObject();
    private List<TrackableEnemy> currentTargets = new List<TrackableEnemy>();


    // Start is called before the first frame update
    override protected void Start() {
        tower = GetComponent<Tower>();
        tower.towerLevelChanged += OnTowerLevelChanged;

        OnTowerLevelChanged(0, tower.currentTowerLevel);

        Tower_Counter++;
        objectKey = GenerateObjectKey(TowerType);
        frameType = MetaDataFrameType.Inbetween;
        screenRectStyle = ScreenSpaceReference.Collider;
        base.Start();
    }

    private void OnTowerLevelChanged(int newLevelIndex, TowerLevel newTowerLevel) {
        if (currentTowerLevel != null) {
            foreach (Affector affector in currentTowerLevel.Affectors) {
                if (affector is AttackAffector attackAffector) {
                    attackAffector.targetter.acquiredTarget -= OnTargetAcquired;
                    attackAffector.targetter.lostTarget -= OnLostTarget;
                }
                else if (affector is SlowAffector slowAffector) {
                    slowAffector.targetter.targetEntersRange -= OnTargetEntersRange;
                    slowAffector.targetter.targetExitsRange -= OnTargetExitsRange;
                }
            }
        }

        currentTargets = new List<TrackableEnemy>();

        currentTowerLevel = newTowerLevel;

        towerStats = new JObject {
            {"level", newLevelIndex+1 },
            {"description", newTowerLevel.description },
            {"maxHealth", newTowerLevel.maxHealth },
            {"startingHealth", newTowerLevel.startingHealth },
            {"dps", newTowerLevel.GetTowerDps() },
            {"cost", newTowerLevel.cost },
            {"sellValue", newTowerLevel.sell }
        };

        JArray affectors = new JArray();
        foreach (Affector affector in newTowerLevel.Affectors) {
            if (affector is AttackAffector attackAffector) {
                affectors.Add(new JObject {
                    {"type", "attack" },
                    {"description", affector.description },
                    {"projectileDamage", attackAffector.GetProjectileDamage() },
                    {"fireRate", attackAffector.fireRate },
                    {"causesSplashDamage", attackAffector.isMultiAttack }
                });
                attackAffector.targetter.acquiredTarget += OnTargetAcquired;
                attackAffector.targetter.lostTarget += OnLostTarget;
            }
            else if (affector is SlowAffector slowAffector) {
                affectors.Add(new JObject {
                    {"type", "slow" },
                    {"description", affector.description },
                    {"slowFactor", slowAffector.slowFactor }
                });
                slowAffector.targetter.targetEntersRange += OnTargetEntersRange;
                slowAffector.targetter.targetExitsRange += OnTargetExitsRange;
            }
            else if (affector is CurrencyAffector currencyAffector) {
                affectors.Add(new JObject {
                    {"type", "currency" },
                    {"description", affector.description },
                    {"currencyIncrement", currencyAffector.currencyGainer.constantCurrencyAddition },
                    {"currencyRate", currencyAffector.currencyGainer.constantCurrencyGainRate }
                });
            }
            else {
                Debug.LogWarningFormat("Unknown AFfector Type: {0}", affector.GetType());
            }
        }
        towerStats["effectDetails"] = affectors;

    }

    private void OnTargetExitsRange(Targetable obj) {
        TrackableEnemy enemy = obj.GetComponent<TrackableEnemy>();
        if(enemy != null) {
            currentTargets.Remove(enemy);
        }
    }

    private void OnTargetEntersRange(Targetable obj) {
        TrackableEnemy enemy = obj.GetComponent<TrackableEnemy>();
        if (enemy != null) {
            currentTargets.Add(enemy);
        }
    }

    private void OnLostTarget() {
        currentTargets.Clear();
    }

    private void OnTargetAcquired(Targetable obj) {
        TrackableEnemy enemy = obj.GetComponent<TrackableEnemy>();
        if (enemy != null) {
            currentTargets.Add(enemy);
        }
    }

    public override JObject InbetweenData() {
        JObject ret = base.KeyFrameData();
        ret["currentHealth"] = tower.configuration.currentHealth;
        ret["currentTargets"] = new JArray() { from enemy in currentTargets select enemy.ObjectKey };
        return ret;
    }

    public override JObject KeyFrameData() {
        JObject ret = InbetweenData();
        ret["type"] = TowerType;
        ret["stats"] = towerStats;
        return ret;
    }
}
