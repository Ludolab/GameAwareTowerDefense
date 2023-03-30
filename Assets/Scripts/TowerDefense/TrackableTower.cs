using GameAware;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Towers;

public class TrackableTower : MetaDataTrackable {
    private static int Tower_Counter = 0;

    private Tower tower;

    // Start is called before the first frame update
    override protected void Start() {
        tower = GetComponent<Tower>();
        Tower_Counter++;
        objectKey = "Tower" + Tower_Counter;
        frameType = MetaDataFrameType.KeyFrame;
        screenRectStyle = ScreenSpaceReference.Collider;
        base.Start();
    }

    // Update is called once per frame
    void Update() {

    }
}
