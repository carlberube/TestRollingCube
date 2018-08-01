using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalCubeObjectiveTracker : ObjectiveTracker {

    public TotalCubesObjective objective;
    public int totalCurrentCubes = 0;
    public int objectiveTotalCubes;

	// Use this for initialization
	void OnEnable () {
        objectiveTotalCubes = objective.totalCubes;
    }

    public void UpdateCount()
    {
        totalCurrentCubes++;
        if(totalCurrentCubes == objectiveTotalCubes)
        {
            objectiveCompleted = true;
            objectiveCompletedEvent.Raised();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
