using UnityEngine;

public class FlankingPathCalculator : IPathCalculator
{
    public Vector3 CalculateDestination(Vector3 playerPosition, Vector3 playerForward,Vector3 enemyPosition, IFollowSettings settings)
    {
        Vector3 behindPlayer = playerPosition - playerForward * settings.FollowDistance;
        Vector3 directionToBehind = (behindPlayer - enemyPosition).normalized;
        Vector3 curvedDirection = Quaternion.Euler(0, settings.LateralAngle, 0) * directionToBehind;
        return behindPlayer + curvedDirection * settings.ApproachDistance;
    }
}
