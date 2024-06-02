using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Link
{
    public GameObject startSurface;
    public GameObject endSurface;
    public GameObject bridge;
}

public class BridgeParameters
{
    public Vector3 start;
    public Vector3 end;
}

public class Map
{
    private BridgeParameters[,] adjacencyMatrix;

    public Map(int size, List<Link> links)
    {
        adjacencyMatrix = new BridgeParameters[size, size];
        foreach(var link in links)
        {
            var startIdx = link.startSurface.GetComponent<SurfaceScript>().Index;
            var endIdx = link.endSurface.GetComponent<SurfaceScript>().Index;
            var bridge = link.bridge.GetComponent<NavMeshLink>();
            adjacencyMatrix[startIdx, endIdx] = new BridgeParameters()
            {
                start = bridge.transform.position + bridge.startPoint,
                end = bridge.transform.position + bridge.endPoint
            };
            adjacencyMatrix[endIdx, startIdx] = new BridgeParameters()
            {
                start = bridge.transform.position + bridge.endPoint,
                end = bridge.transform.position + bridge.startPoint
            };
        }
    }

    public List<Vector3> GetPath(int startSurface, int endSurface)
    {
        var globalPath = findGlobalPath(startSurface, endSurface);
        var fullPath = new List<Vector3>();
        for(var i = 0; i < globalPath.Count - 1; ++i)
        {
            var bridge = adjacencyMatrix[globalPath[i], globalPath[i + 1]];
            fullPath.Add(bridge.start);
            fullPath.Add(bridge.end);
        }
        return fullPath;
    }

    private List<int> findGlobalPath(int startSurface, int endSurface)
    {
        var nodes = new Queue<int>();
        var visited = new HashSet<int>();
        var parents = new int[adjacencyMatrix.GetLength(0)];
        nodes.Enqueue(startSurface);
        visited.Add(startSurface);
        parents[startSurface] = -1;
        while (nodes.Count != 0)
        {
            var current = nodes.Dequeue();
            if (current == endSurface) break;
            foreach (var neighbor in getNeighbors(current))
            {
                if (visited.Contains(neighbor))
                    continue;
                nodes.Enqueue(neighbor);
                visited.Add(neighbor);
                parents[neighbor] = current;
            }
        }

        var globalPath = new List<int>();
        int currentNode = endSurface;
        while(currentNode != -1)
        {
            globalPath.Add(currentNode);
            currentNode = parents[currentNode];
        }
        globalPath.Reverse();
        return globalPath;
    }

    private List<int> getNeighbors(int idx)
    {
        var neighbors = new List<int>();
        for (var i = 0; i < adjacencyMatrix.GetLength(0); i++)
        {
            if (adjacencyMatrix[idx, i] != null)
                neighbors.Add(i);
        }
        return neighbors;
    }
}

public class PathFinder : MonoBehaviour
{
    public List<Link> links;

    private Map map;

    void Awake()
    {
        var surfacesNumber = prepareSurfaces();
        map = new Map(surfacesNumber, links);
    }

    public List<Vector3> GetPath(NavMeshAgent agent, NavMeshAgent target)
    {
        var agentObject = agent.navMeshOwner as Component;
        var targetObject = target.navMeshOwner as Component;
        if (agentObject == null || targetObject == null)
            return null;
        var agentSurface = agentObject.GetComponent<SurfaceScript>().Index;
        var targetSurface = targetObject.GetComponent<SurfaceScript>().Index;
        var path = new List<Vector3>();
        if (agentSurface != targetSurface)
            path = map.GetPath(agentSurface, targetSurface);
        path.Add(target.gameObject.transform.position);
        return path;
    }

    private int prepareSurfaces()
    {
        var idx = 0;
        foreach (var surface in links.Select(l => new List<GameObject>() { l.startSurface, l.endSurface })
            .SelectMany(x => x).Distinct().Select(g => g.GetComponent<SurfaceScript>())) 
        {
            surface.Index = idx++;
        }
        return idx;
    }
}
