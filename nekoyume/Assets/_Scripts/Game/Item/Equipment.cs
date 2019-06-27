using System;
using System.Linq;
using Nekoyume.Data;
using Nekoyume.Data.Table;
using Nekoyume.Game.Skill;
using Nekoyume.Model;

namespace Nekoyume.Game.Item
{
    [Serializable]
    public class Equipment : ItemUsable
    {
        public bool equipped = false;

        public Equipment(Data.Table.Item data, SkillBase skillBase = null)
            : base(data, skillBase)
        {
            if (!string.IsNullOrEmpty(Data.ability1)
                && Data.value1 > 0)
            {
                Stats.SetStatValue(Data.ability1, Data.value1);
            }

            if (!string.IsNullOrEmpty(Data.ability2)
                && Data.value2 > 0)
            {
                Stats.SetStatValue(Data.ability2, Data.value2);
            }

            //TODO 논의후 테이블에 제대로 설정되야함.
            Stats.SetStatValue("turnSpeed", Data.turnSpeed);
            //TODO 장비대신 스킬별 사거리를 사용해야함.
            Stats.SetStatValue("attackRange", Data.attackRange);
        }

        public bool Equip()
        {
            equipped = true;
            return true;
        }

        public bool Unequip()
        {
            equipped = false;
            return true;
        }
    }
}
