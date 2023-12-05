﻿using Clocktower.Agent;
using Clocktower.Game;

namespace Clocktower
{
    public partial class StorytellerForm : Form, IGameObserver
    {
        public StorytellerForm()
        {
            InitializeComponent();
        }

        public void AssignCharacter(Player player)
        {
            if (player.RealCharacter == null || player.Character == null || player.RealAlignment == null || player.Alignment == null)
            {
                throw new InvalidOperationException("Player has not been assigned a character/alignment");
            }
            if (player.Character == player.RealCharacter)
            {
                outputText.AppendFormattedText("%p is the %c.\n", player, player.Character);
            }
            else
            {
                outputText.AppendFormattedText("%p believes they are the %c but they are actually the %c.\n", player, player.Character, player.RealCharacter);
            }
        }

        public void Night(int nightNumber)
        {
            outputText.AppendBoldText($"\nNight {nightNumber}\n\n");
        }

        public void Day(int dayNumber)
        {
            outputText.AppendBoldText($"\nDay {dayNumber}\n\n");
        }

        public void PlayerDiedAtNight(Player newlyDeadPlayer)
        {
            outputText.AppendFormattedText("%p died in the night.\n", newlyDeadPlayer, StorytellerView);
        }

        public void PlayerIsExecuted(Player executedPlayer, bool playerDies)
        {
            if (playerDies)
            {
                outputText.AppendFormattedText("%p is executed and dies.\n", executedPlayer, StorytellerView);
            }
            else if (executedPlayer.Alive)
            {
                outputText.AppendFormattedText("%p is executed but does not die.\n", executedPlayer, StorytellerView);
            }
            else
            {
                outputText.AppendFormattedText("%p's corpse is executed.\n", executedPlayer, StorytellerView);
            }
        }

        public void DayEndsWithNoExecution()
        {
            outputText.AppendText("There is no execution and the day ends.\n");
        }

        public void MinionInformation(Player minion, Player demon, IReadOnlyCollection<Player> fellowMinions)
        {
            outputText.AppendFormattedText($"%p learns that %p is their demon and that their fellow {(fellowMinions.Count > 1 ? "minions are" : "minion is")} %P.\n", minion, demon, fellowMinions, StorytellerView);
        }

        public void DemonInformation(Player demon, IReadOnlyCollection<Player> minions, IReadOnlyCollection<Character> notInPlayCharacters)
        {
            outputText.AppendFormattedText($"%p learns that %P {(minions.Count > 1 ? "are their minions" : "is their minion")}, and that the following characters are not in play: %C.\n", demon, minions, notInPlayCharacters, StorytellerView);
        }

        public void NotifyGodfather(Player godfather, IReadOnlyCollection<Character> outsiders)
        {
            if (outsiders.Count == 0)
            {
                outputText.AppendFormattedText("%p learns that there are no outsiders in play.\n", godfather, StorytellerView);
                return;
            }
            outputText.AppendFormattedText("%p learns that the following outsiders are in play: %C\n", godfather, outsiders, StorytellerView);
        }

        public void NotifyLibrarian(Player librarian, Player playerA, Player playerB, Character character)
        {
            outputText.AppendFormattedText("%p learns that %p or %p is the %c.\n", librarian, playerA, playerB, character, StorytellerView);
        }

        public void NotifyInvestigator(Player investigator, Player playerA, Player playerB, Character character)
        {
            outputText.AppendFormattedText("%p learns that %p or %p is the %c.\n", investigator, playerA, playerB, character, StorytellerView);
        }

        public void NotifySteward(Player steward, Player goodPlayer)
        {
            outputText.AppendFormattedText("%p learns that %p is a good player.\n", steward, goodPlayer, StorytellerView);
        }

        public void NotifyEmpath(Player empath, Player neighbourA, Player neighbourB, int evilCount)
        {
            outputText.AppendFormattedText($"%p learns that %b of their living neighbours (%p and %p) {(evilCount == 1 ? "is" : "are")} evil.\n", empath, evilCount, neighbourA, neighbourB, StorytellerView);
        }

        public void ChoiceFromImp(Player imp, Player target)
        {
            outputText.AppendFormattedText("%p has chosen to kill %p.\n", imp, target, StorytellerView);
        }

        public void ChoiceFromRavenkeeper(Player ravenkeeper, Player target, Character character)
        {
            outputText.AppendFormattedText("%p chooses %p and learns that they are the %c.\n", ravenkeeper, target, character, StorytellerView);
        }

        public void AnnounceNomination(Player nominator, Player nominee)
        {
            outputText.AppendFormattedText("%p nominates %p.\n", nominator, nominee, StorytellerView);
        }

        public void AnnounceVote(Player voter, Player nominee, bool votedToExecute)
        {
            if (votedToExecute)
            {
                outputText.AppendFormattedText("%p votes to execute %p.\n", voter, nominee, StorytellerView);
            }
            else
            {
                outputText.AppendFormattedText("%p does not vote.\n", voter, nominee, StorytellerView);
            }
        }

        public void AnnounceVoteResult(Player nominee, int voteCount, bool beatsCurrent, bool tiesCurrent)
        {
            if (beatsCurrent)
            {
                outputText.AppendFormattedText("%p received %b votes. That is enough to put them on the block.\n", nominee, voteCount, StorytellerView);
            }
            else if (tiesCurrent)
            {
                outputText.AppendFormattedText("%p received %b votes which is a tie. No one is on the block.\n", nominee, voteCount, StorytellerView);
            }
            else
            {
                outputText.AppendFormattedText("%p received %b votes which is not enough.\n", nominee, voteCount, StorytellerView);
            }
        }

        private const bool StorytellerView = true;
    }
}
