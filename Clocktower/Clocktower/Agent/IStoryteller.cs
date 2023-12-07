﻿using Clocktower.Game;
using Clocktower.Options;

namespace Clocktower.Agent
{
    internal interface IStoryteller
    {
        Task<IOption> GetDrunk(IReadOnlyCollection<IOption> drunkCandidates);

        void AssignCharacter(Player player);

        void MinionInformation(Player minion, Player demon, IReadOnlyCollection<Player> fellowMinions);
        void DemonInformation(Player demon, IReadOnlyCollection<Player> minions, IReadOnlyCollection<Character> notInPlayCharacters);

        void NotifyGodfather(Player godfather, IReadOnlyCollection<Character> outsiders);
        void NotifyLibrarian(Player librarian, Player playerA, Player playerB, Character character);
        void NotifyInvestigator(Player investigator, Player playerA, Player playerB, Character character);
        void NotifySteward(Player steward, Player goodPlayer);
        void NotifyEmpath(Player empath, Player neighbourA, Player neighbourB, int evilCount);

        void ChoiceFromImp(Player imp, Player target);
        void ChoiceFromAssassin(Player assassin, Player? target);
        void ChoiceFromGodfather(Player godfather, Player target);
        void ChoiceFromRavenkeeper(Player ravenkeeper, Player target, Character character);
    }
}
