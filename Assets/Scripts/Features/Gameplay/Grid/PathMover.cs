using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PathMover
{
    private List<Vector3> _path;
    private float _moveSpeed;

    public PathMover(List<Vector3> path, float moveSpeed)
    {
        _path = new List<Vector3>(path);
        _moveSpeed = moveSpeed;
    }

    public async Task Move(EntityPrefabController gameObject)
    {
        while (_path.Count > 0)
        {
            await MoveAlongPath(gameObject);
            _path.RemoveAt(0);
            await Task.Yield();
        }
    }
    
    private async Task MoveAlongPath(EntityPrefabController gameObject)
    {
        Vector3 targetPosition = new (_path[0].x, _path[0].y, 0);
        while(Vector3.Distance(gameObject.transform.position, targetPosition) > Mathf.Epsilon)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, targetPosition, _moveSpeed * Time.deltaTime);
            await Task.Yield();
        }
    }
}
