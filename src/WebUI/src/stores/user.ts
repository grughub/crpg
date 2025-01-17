import { type User, type UserItem } from '@/models/user';
import { type Character } from '@/models/character';
import { ClanMember, type Clan } from '@/models/clan';

import { getUser, getUserItems, buyUserItem, getUserClan } from '@/services/users-service';
import { getCharacters } from '@/services/characters-service';
import { getClanMembers, getClanMember } from '@/services/clan-service';

interface State {
  user: User | null;
  characters: Character[];
  userItems: UserItem[];
  clan: Clan | null;
  clanMember: ClanMember | null;
}

export const useUserStore = defineStore('user', {
  state: (): State => ({
    user: null,
    characters: [],
    userItems: [],
    clan: null,
    clanMember: null,
  }),

  getters: {
    activeCharacterId: state => state.user?.activeCharacterId || state.characters?.[0]?.id || null,
  },

  actions: {
    validateCharacter(id: number) {
      return this.characters.some(c => c.id === id);
    },

    replaceCharacter(character: Character) {
      this.characters.splice(
        this.characters.findIndex(c => c.id === character.id),
        1,
        character
      );
    },

    async fetchUser() {
      this.user = await getUser();
    },

    async fetchCharacters() {
      this.characters = await getCharacters();
    },

    async fetchUserItems() {
      this.userItems = await getUserItems();
    },

    addUserItem(userItem: UserItem) {
      this.userItems.push(userItem);
    },

    subtractGold(loss: number) {
      this.user!.gold -= loss;
    },

    async buyItem(itemId: string) {
      const userItem = await buyUserItem(itemId);
      this.addUserItem(userItem);
      this.subtractGold(userItem.baseItem.price);
    },

    async getUserClan() {
      this.clan = await getUserClan();
    },

    async getUserClanMember() {
      if (this.clan === null || this.user === null) return;

      this.clanMember = getClanMember(await getClanMembers(this.clan.id), this.user.id);
    },
  },
});
