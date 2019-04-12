using System;
using Nekoyume.Data.Table;

namespace Nekoyume.Model
{
    [Serializable]
    public class Monster : CharacterBase
    {
        public Character data;

        public Monster(Character data, int monsterLevel, Player player)
        {
            var stats = data.GetStats(monsterLevel);
            currentHP = stats.HP;
            atk = stats.Damage;
            def = stats.Defense;
            luck = stats.Luck;
            targets.Add(player);
            Simulator = player.Simulator;
            this.data = data;
            level = monsterLevel;
            ATKElement = Game.Elemental.Create(data.elemental);
            DEFElement = Game.Elemental.Create(data.elemental);
        }

        protected override void OnDead()
        {
            base.OnDead();
            var player = (Player) targets[0];
            player.RemoveTarget(this);
        }
    }
}
