using UnityEngine;

namespace ShooterDem
{
// "Ficha" de un tipo de enemigo para el spawner (data-driven, como WeaponData).
// De momento describe QUE prefab y con que probabilidad aparece; las stats viven en
// los componentes del prefab. Mas adelante puede crecer (vida/velocidad por aqui).
// Create: Assets > Create > Shooter > Enemy Data.
[CreateAssetMenu(fileName = "EnemyData", menuName = "Shooter/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Enemy";
    public GameObject prefab;       // prefab del tipo (con su EnemyAttack y stats)
    public float spawnWeight = 1f;  // probabilidad relativa en las oleadas
}
}
