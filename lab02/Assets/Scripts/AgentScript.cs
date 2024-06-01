using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentScript : MonoBehaviour
{
    public GameObject targetObject;
    public GameObject pathFinderObject;
    private NavMeshAgent agent;
    private NavMeshAgent target;
    private PathFinder pathFinder;
    public float epsilon = 0.000000001f;

    public bool targetIsReached = false;
    public int steps = 0;
    private float leftLegAngle = 9f;
    [SerializeField] private GameObject leftLeg = null;
    [SerializeField] private GameObject rightLeg = null;
    [SerializeField] private GameObject leftLegJoint = null;
    [SerializeField] private GameObject rightLegJoint = null;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        target = targetObject.GetComponent<NavMeshAgent>();
        pathFinder = pathFinderObject.GetComponent<PathFinder>();
    }

    void Start()
    {
        var path = pathFinder.GetPath(agent, target);
        if (path != null)
            StartCoroutine(Execute(path));
    }

    private void FixedUpdate()
    {
        if (agent.velocity.magnitude > 0)
            MoveLegs();
    }

    void MoveLegs()
    {
        if (steps >= 6)
        {
            leftLegAngle = -leftLegAngle;
            steps = -6;
        }
        steps++;
        leftLeg.transform.RotateAround(leftLegJoint.transform.position, transform.right, leftLegAngle);
        rightLeg.transform.RotateAround(rightLegJoint.transform.position, transform.right, -leftLegAngle);
    }

    IEnumerator Execute(List<Vector3> path)
    {
        agent.ResetPath();
        foreach(var localTarget in path)
        {
            NavMeshPath navMeshPath = new();
            while (!(agent.CalculatePath(localTarget, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete))
                yield return new WaitForFixedUpdate();
            agent.SetPath(navMeshPath);
            while (agent.remainingDistance > epsilon)
                yield return new WaitForFixedUpdate();
        }
        agent.enabled = false;
    }
}
