﻿using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneCore.Actions;
using SabberStoneCore.Enchants;
using SabberStoneCore.Model;
using SabberStoneCore.Collections;

namespace SabberStoneCore.Tasks.SimpleTasks
{
    public class PotionGenerating : SimpleTask
    {
        private ISet<Card> _kazakusPotionSpells;

        private ISet<Card> KazakusPotionSpells => _kazakusPotionSpells ?? (_kazakusPotionSpells = GetKazakusPotionSpells());

        public PotionGenerating(List<int> scriptTags = null)
        {
            ScriptTags = scriptTags;
        }

        public List<int> ScriptTags { get; set; }

        public override ETaskState Process()
        {
            
            var minion = Source as Minion;
            if (minion != null && ScriptTags == null)
            {
                Generic.CreateChoiceCards.Invoke(Controller, Source, null, EChoiceType.GENERAL, EChoiceAction.KAZAKUS, LightWeightOrderedSet<Card>.Build(minion.Card.Entourage.Select(Cards.FromId)), null);
                return ETaskState.COMPLETE;

            }

            Game.Log(ELogLevel.INFO, EBlockType.PLAY, "PotionGenerating", $"Current scripttags = {string.Join(",", ScriptTags)}");

            if (ScriptTags.Count < 3)
            {

				int cost = KazakusPotionSpells.First(p => 
                    p[EGameTag.TAG_SCRIPT_DATA_NUM_1] == ScriptTags[0]).Cost;
                var cardIdList = KazakusPotionSpells.Where(p => 
                    p[EGameTag.TAG_SCRIPT_DATA_NUM_1] < 1000 && p.Cost == cost &&
                    (ScriptTags.Count != 2 || p[EGameTag.TAG_SCRIPT_DATA_NUM_1] != ScriptTags[1])).ToList();

                var cardList = new OrderedHashSet<Card>();
                while (cardList.Count() < 3)
                {
					Card card = Util.Choose(cardIdList);
                    cardList.Add(card);
                    cardIdList.Remove(card);
                }

                Generic.CreateChoiceCards.Invoke(Controller, Source, null, EChoiceType.GENERAL, EChoiceAction.KAZAKUS, cardList, null);
                return ETaskState.COMPLETE;
            }

            // create card ...
            var baseCard = KazakusPotionSpells.First(p => p[EGameTag.TAG_SCRIPT_DATA_NUM_1] == ScriptTags[0]);
            var spell1 = KazakusPotionSpells.First(p => p.Cost == baseCard.Cost && p[EGameTag.TAG_SCRIPT_DATA_NUM_1] == ScriptTags[1]);
            var spell2 = KazakusPotionSpells.First(p => p.Cost == baseCard.Cost && p[EGameTag.TAG_SCRIPT_DATA_NUM_1] == ScriptTags[2]);
            baseCard.Text = "(1) " + spell1.Text + "(2) " + spell2.Text;
            baseCard.Enchantments = new List<Enchantment>();
            baseCard.Enchantments.AddRange(spell1.Enchantments);
            spell1.PlayRequirements.ToList().ForEach(p =>
            {
                if (!baseCard.PlayRequirements.ContainsKey(p.Key))
                    baseCard.PlayRequirements.Add(p.Key, p.Value);
            });
            baseCard.Enchantments.AddRange(spell2.Enchantments);
            spell2.PlayRequirements.ToList().ForEach(p =>
            {
                if (!baseCard.PlayRequirements.ContainsKey(p.Key))
                    baseCard.PlayRequirements.Add(p.Key, p.Value);
            });

            var task = new AddCardTo(baseCard, EEntityType.HAND)
            {
                Game = Controller.Game,
                Controller = Controller,
                Source = Source as IPlayable,
                Target = Target as IPlayable
            };
            Controller.Game.TaskQueue.Enqueue(task);

            // remove tag script from used kazakus entities
            foreach (var playables in Controller.Setaside.GetAll.Where(p => p.Card.Id.StartsWith("CFM_621")))
            {
                playables[EGameTag.TAG_SCRIPT_DATA_NUM_1] = 0;
            }

            return ETaskState.COMPLETE;
        }

        private static ISet<Card> GetKazakusPotionSpells()
        {
			IEnumerable<Card> enumerable = Cards.All.Where(p => p.Id.StartsWith("CFM_621t") 
            && !p.Id.Equals("CFM_621t")
            //&& !p.Id.Equals("CFM_621t11")
            //&& !p.Id.Equals("CFM_621t12")
            //&& !p.Id.Equals("CFM_621t13")
            && !p.Id.Equals("CFM_621t14")
            && !p.Id.Equals("CFM_621t15")
            );

            return new OrderedHashSet<Card>(enumerable);
        }

        private void ProcessSplit(List<Card>[] cardsToDiscover, EChoiceAction choiceAction)
        {
 
        }

        public override ISimpleTask Clone()
        {
            var clone = new PotionGenerating(ScriptTags);
            clone.Copy(this);
            return clone;
        }
    }
}