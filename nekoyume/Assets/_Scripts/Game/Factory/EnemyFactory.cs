using System.Collections.Generic;
using DG.Tweening;
using Nekoyume.Game.Character;
using Nekoyume.Model;
using UniRx.Toolkit;
using UnityEngine;


namespace Nekoyume.Game.Factory
{
    public class EnemyFactory : MonoBehaviour
    {
        public GameObject Create(int monsterId, Vector2 position, int power)
        {
            Data.Tables tables = this.GetRootComponent<Data.Tables>();
            Data.Table.Monster monsterData;
            if (!tables.Monster.TryGetValue(monsterId, out monsterData))
                return null;

            var objectPool = GetComponent<Util.ObjectPool>();
            var enemy = objectPool.Get<Character.Enemy>(position);
            if (enemy == null)
                return null;

            enemy.InitAI(monsterData);
            enemy.InitStats(monsterData, power);

            // sprite
            var render = enemy.GetComponent<SpriteRenderer>();
            var sprite = Resources.Load<Sprite>($"images/character_{monsterId}");
            if (sprite == null)
                sprite = Resources.Load<Sprite>("images/pet");
            render.sprite = sprite;
            render.sortingOrder = Mathf.FloorToInt(-position.y * 10.0f);

            return enemy.gameObject;
        }

        public GameObject CreateBoss(int bossId, Vector2 position, int power)
        {
            Data.Tables tables = this.GetRootComponent<Data.Tables>();
            Data.Table.Monster monsterData;
            if (!tables.Monster.TryGetValue(bossId, out monsterData))
                return null;

            var res = Resources.Load<GameObject>($"Prefab/Character/Boss_{bossId}/Boss_{bossId}");
            var bossObj = Instantiate(res, position, new Quaternion(), transform);
            if (bossObj == null)
                return null;

            var boss = bossObj.GetComponent<Character.Boss.BossBase>();
            boss.InitAI(monsterData);
            boss.InitStats(monsterData, power);

            return bossObj.gameObject;
        }

        public GameObject Create(Monster spawnCharacter, Vector2 position)
        {
            var objectPool = GetComponent<Util.ObjectPool>();
            if (ReferenceEquals(objectPool, null))
            {
                throw new NotFoundComponentException<Util.ObjectPool>();
            }

            var go = objectPool.Get("Enemy", true, position);
            //FIXME 애니메이터 재사용시 기존 투명도가 유지되는 문제가 있음.
//            var animator = objectPool.Get(spawnCharacter.data.Id.ToString(), true);
            var origin = Resources.Load<GameObject>($"Prefab/{spawnCharacter.data.Id}");
            var animator = Instantiate(origin, go.transform);
            var enemy = animator.GetComponent<Enemy>();
            if (ReferenceEquals(enemy, null))
            {
                throw new NotFoundComponentException<Enemy>();
            }
            enemy.Init(spawnCharacter);
            return go;
        }
    }
}
