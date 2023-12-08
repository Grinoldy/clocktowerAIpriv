﻿using Clocktower.Agent;

namespace Clocktower.Game
{
    internal class Kills
    {
        public Kills(IStoryteller storyteller, Grimoire grimoire)
        {
            this.storyteller = storyteller;
            this.grimoire = grimoire;
        }

        public void DayKill(Player player)
        {
            HandleDayDeath(player);
            player.Kill();
        }

        public void NightKill(Player player)
        {
            HandleNightDeath(player);
            player.Tokens.Add(Token.DiedAtNight);
        }

        public void DemonKill(Player player)
        {
            HandleNightDeath(player);
            player.Tokens.Add(Token.KilledByDemon);
        }

        private void HandleDayDeath(Player player)
        {
            grimoire.RemoveTokensForCharacter(player.Character);
            ProcessDayTriggersForOtherCharacters(player);
            ProcessTriggersForOtherCharacters(player);
        }

        private void HandleNightDeath(Player player)
        {
            grimoire.RemoveTokensForCharacter(player.Character);
            ProcessTriggersForOtherCharacters(player);
        }

        private void ProcessDayTriggersForOtherCharacters(Player dyingPlayer)
        {
            // Godfather
            if (dyingPlayer.CharacterType == CharacterType.Outsider)
            {
                foreach (var godfather in grimoire.GetLivingPlayers(Character.Godfather))
                {
                    godfather.Tokens.Add(Token.GodfatherKillsTonight);
                }
            }
        }

        private void ProcessTriggersForOtherCharacters(Player dyingPlayer)
        {
            // Scarlet Woman
            if (dyingPlayer.CharacterType == CharacterType.Demon && grimoire.Players.Count(player => player.Alive) >= 5)
            {
                var scarletWoman = grimoire.GetLivingPlayers(Character.Scarlet_Woman).FirstOrDefault(player => player.Alive && !player.DrunkOrPoisoned); // shouldn't be more than 1 Scarlet Woman
                if (scarletWoman != null)
                {
                    storyteller.ScarletWomanTrigger(dyingPlayer, scarletWoman);
                    grimoire.ChangeCharacter(scarletWoman, dyingPlayer.Character);
                }
            }
        }

        private readonly IStoryteller storyteller;
        private readonly Grimoire grimoire;
    }
}
