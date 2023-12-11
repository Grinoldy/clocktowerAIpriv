﻿using Clocktower.Agent;
using Clocktower.Game;
using Clocktower.Observer;
using Clocktower.Storyteller;

namespace Clocktower.Events
{
    internal class Nominations : IGameEvent
    {
        public Player? PlayerToBeExecuted { get; private set; }

        public Nominations(IStoryteller storyteller, Grimoire grimoire, ObserverCollection observers, Random random)
        {
            this.storyteller = storyteller;
            this.grimoire = grimoire;
            this.observers = observers;
            this.random = random;
        }

        public async Task RunEvent()
        {
            while (true)
            {
                var nomination = await RequestNomination();
                if (!nomination.HasValue)
                {   // No more nominations.
                    return;
                }
                await HandleNomination(nomination.Value.nominator, nomination.Value.nominee);
            }
        }

        private async Task<(Player nominator, Player nominee)?> RequestNomination()
        {
            // Only alive players who haven't yet nominated can nominate.
            var players = grimoire.Players.Where(player => player.Alive)
                                          .Except(playersWhoHaveAlreadyNominated)
                                          .ToList();
            players.Shuffle(random);

            foreach (var player in players)
            {
                var nominee = await player.Agent.GetNomination(grimoire.Players.Except(playersWhoHaveAlreadyBeenNominated));
                if (nominee != null)
                {
                    return (player, nominee);
                }
            }
            return null;
        }

        private async Task HandleNomination(Player nominator, Player nominee)
        {
            playersWhoHaveAlreadyNominated.Add(nominator);
            playersWhoHaveAlreadyBeenNominated.Add(nominee);

            observers.AnnounceNomination(nominator, nominee);
            if (nominator == nominee)
            {
                var statement = await nominator.Agent.GetReasonForSelfNomination();
                if (!string.IsNullOrEmpty(statement))
                {
                    observers.PublicStatement(nominator, statement);
                }
            }
            else
            {
                var prosecution = await nominator.Agent.GetProsecution(nominee);
                if (!string.IsNullOrEmpty(prosecution))
                {
                    observers.PublicStatement(nominator, prosecution);
                }
                var defence = await nominee.Agent.GetDefence(nominator);
                if (!string.IsNullOrEmpty(defence))
                {
                    observers.PublicStatement(nominee, defence);
                }
            }

            int voteCount = await RunVote(nominee);

            int minVotesRequired = (grimoire.Players.Count(player => player.Alive) + 1) / 2;
            bool beatsCurrent = voteCount >= minVotesRequired && (!highestVoteCount.HasValue || voteCount > highestVoteCount.Value);
            bool tiesCurrent = highestVoteCount.HasValue && voteCount == highestVoteCount.Value;

            observers.AnnounceVoteResult(nominee, voteCount, beatsCurrent, tiesCurrent);

            if (tiesCurrent)
            {
                PlayerToBeExecuted = null;
            }
            else if (beatsCurrent)
            {
                PlayerToBeExecuted = nominee;
                highestVoteCount = voteCount;
            }
        }

        private async Task<int> RunVote(Player nominee)
        {
            int voteCount = 0;

            foreach (var player in grimoire.GetAllPlayersEndingWithPlayer(nominee))
            {
                if (player.Alive || player.HasGhostVote)
                {
                    bool votedToExecute = await player.Agent.GetVote(nominee);
                    observers.AnnounceVote(player, nominee, votedToExecute);
                    if (votedToExecute)
                    {
                        ++voteCount;
                        if (!player.Alive)
                        {
                            player.UseGhostVote();
                        }
                    }
                }
            }

            return voteCount;
        }

        private readonly IStoryteller storyteller;
        private readonly Grimoire grimoire;
        private readonly ObserverCollection observers;
        private readonly Random random;

        private int? highestVoteCount;

        private List<Player> playersWhoHaveAlreadyNominated = new();
        private List<Player> playersWhoHaveAlreadyBeenNominated = new();
    }
}
