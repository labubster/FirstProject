﻿using System.Linq;
using Libplanet.Assets;
using Nekoyume.State;
using Nekoyume.TableData;
using UniRx;

namespace Nekoyume.UI.Model
{
    public class RuneItem
    {
        public RuneListSheet.Row Row { get; }
        public RuneCostSheet.RuneCostData Cost { get; }

        public FungibleAssetValue RuneStone { get; set; }
        public int Level { get; }
        public bool IsMaxLevel { get; }
        public bool EnoughRuneStone { get; }
        public bool EnoughCrystal { get; }
        public bool EnoughNcg { get; }
        public bool HasNotification => EnoughRuneStone && EnoughCrystal && EnoughNcg;

        public readonly ReactiveProperty<bool> IsSelected = new();

        public RuneItem(RuneListSheet.Row row, int level)
        {
            Row = row;
            Level = level;

            var costSheet = Game.Game.instance.TableSheets.RuneCostSheet;
            if (!costSheet.TryGetValue(row.Id, out var costRow))
            {
                return;
            }

            Cost = costRow.Cost.FirstOrDefault(x => x.Level == level + 1);
            IsMaxLevel = level == costRow.Cost.Count;

            if (Cost is null)
            {
                return;
            }

            if (!States.Instance.RuneStoneBalance.ContainsKey(Cost.RuneStoneId))
            {
                return;
            }

            RuneStone = States.Instance.RuneStoneBalance[Cost.RuneStoneId];
            EnoughRuneStone = RuneStone.MajorUnit >= Cost.RuneStoneId;
            EnoughCrystal = States.Instance.CrystalBalance.MajorUnit >= Cost.CrystalQuantity;
            EnoughNcg = States.Instance.GoldBalanceState.Gold.MajorUnit >= Cost.NcgQuantity;
        }
    }
}