using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPointTrigger : MonoBehaviour
{
    public GameObject GoalTrig;
    public GameObject HalfLapTrig;

    private void OnTriggerEnter(Collider other)
    {
        GoalTrig.SetActive(true);
        HalfLapTrig.SetActive(false);
    }
}
