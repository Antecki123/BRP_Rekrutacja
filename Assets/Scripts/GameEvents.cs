public static class GameEvents
{
    public delegate void OnEnemyKilled(IEnemy enemy);
    public static OnEnemyKilled EnemyKilled;

    public delegate void OnScoredPoints(int points);
    public static OnScoredPoints ScoredPoints;

    public delegate void OnScoreUpdated();
    public static OnScoreUpdated ScoreUpdated;
}

