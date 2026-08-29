using UnityEngine;

/// <summary>集中保存运行时实体 Prefab，避免 GameManager 逐个 AddComponent 创建对象。</summary>
[CreateAssetMenu(fileName = "RoguelikePrefabCatalog", menuName = "Roguelike/Prefab 目录")]
public sealed class RoguelikePrefabCatalog : ScriptableObject
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject experienceOrbPrefab;
    [SerializeField] private GameObject weaponPickupPrefab;

    public GameObject PlayerPrefab => playerPrefab;
    public GameObject EnemyPrefab => enemyPrefab;
    public GameObject ExperienceOrbPrefab => experienceOrbPrefab;
    public GameObject WeaponPickupPrefab => weaponPickupPrefab;

#if UNITY_EDITOR
    public void Configure(GameObject player, GameObject enemy, GameObject orb, GameObject weaponPickup)
    {
        playerPrefab = player;
        enemyPrefab = enemy;
        experienceOrbPrefab = orb;
        weaponPickupPrefab = weaponPickup;
    }
#endif

    public bool IsComplete => playerPrefab != null && enemyPrefab != null &&
        experienceOrbPrefab != null && weaponPickupPrefab != null;
}
