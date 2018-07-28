using UnityEngine;

public class ObjectiveObject : ScriptableObject
{
    public string objectiveDescription;
}

public class TotalRollsObjective : ObjectiveObject
{
    public int totalRolls;
}

public class StarCollectionObjective : ObjectiveObject
{
    public Vector3[] positions;
}

public class TotalCubesObjecitve : ObjectiveObject
{
    public int totalCubes;
}

public class TimeLimitObjective : ObjectiveObject
{
    public float totalTime;
}