import { type PartialDeep } from 'type-fest';
import { mockGet } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';
import {
  type ItemFlat,
  ItemType,
  type Item,
  WeaponClass,
  ItemUsage,
  ItemFlags,
  WeaponFlags,
  ItemFieldFormat,
  ItemFieldCompareRule,
  ItemSlot,
  ItemFamilyType,
  DamageType,
} from '@/models/item';
import { type AggregationConfig, AggregationView } from '@/models/item-search';
import { UserItem } from '@/models/user';
import { Culture } from '@/models/culture';

vi.mock('@/services/item-search-service/aggregations.ts', () => ({
  aggregationsConfig: {
    type: {
      view: AggregationView.Checkbox,
    },
    flags: {
      view: AggregationView.Checkbox,
      format: ItemFieldFormat.List,
    },
    price: {
      view: AggregationView.Range,
      format: ItemFieldFormat.Number,
    },
    thrustDamage: {
      view: AggregationView.Range,
      format: ItemFieldFormat.Damage,
    },
    swingDamage: {
      view: AggregationView.Range,
      format: ItemFieldFormat.Damage,
    },
    damage: {
      view: AggregationView.Range,
      format: ItemFieldFormat.Damage,
    },
    requirement: {
      view: AggregationView.Range,
      format: ItemFieldFormat.Requirement,
      compareRule: ItemFieldCompareRule.Less,
    },
  } as AggregationConfig,
}));

import mockItems from '@/__mocks__/items.json';

import {
  getItems,
  getItemImage,
  getAvailableSlotsByItem,
  hasWeaponClassesByItemType,
  getWeaponClassesByItemType,
  getCompareItemsResult,
  humanizeBucket,
  getItemFieldDiffStr,
  type HumanBucket,
  IconBucketType,
} from './item-service';

it('getItems', async () => {
  mockGet('/items').willResolve(response<Item[]>(mockItems as Item[]));

  expect(await getItems()).toEqual(mockItems);
});

it('getItemImage', () => {
  expect(getItemImage('crpg_aserai_noble_sword_2_t5')).toEqual(
    '/items/crpg_aserai_noble_sword_2_t5.png'
  );
});

it.each<[PartialDeep<Item>, PartialDeep<Record<ItemSlot, UserItem>>, ItemSlot[]]>([
  [
    { type: ItemType.MountHarness, flags: [], armor: { familyType: ItemFamilyType.Horse } },
    {},
    [ItemSlot.MountHarness],
  ],
  [
    { type: ItemType.MountHarness, flags: [], armor: { familyType: ItemFamilyType.Horse } },
    { [ItemSlot.Mount]: { baseItem: { mount: { familyType: ItemFamilyType.Horse } } } },
    [ItemSlot.MountHarness],
  ],
  [
    { type: ItemType.MountHarness, flags: [], armor: { familyType: ItemFamilyType.Camel } },
    {},
    [ItemSlot.MountHarness],
  ],
  [
    { type: ItemType.MountHarness, flags: [], armor: { familyType: ItemFamilyType.Camel } },
    { [ItemSlot.Mount]: { baseItem: { mount: { familyType: ItemFamilyType.Horse } } } },
    [],
  ],
  [
    { type: ItemType.MountHarness, flags: [], armor: { familyType: ItemFamilyType.Horse } },
    { [ItemSlot.Mount]: { baseItem: { mount: { familyType: ItemFamilyType.Camel } } } },
    [],
  ],
  [
    { type: ItemType.OneHandedWeapon, flags: [] },
    {},
    [ItemSlot.Weapon0, ItemSlot.Weapon1, ItemSlot.Weapon2, ItemSlot.Weapon3],
  ],
  [{ type: ItemType.Banner, flags: [] }, {}, [ItemSlot.WeaponExtra]],
  [{ type: ItemType.Polearm, flags: [ItemFlags.DropOnWeaponChange] }, {}, [ItemSlot.WeaponExtra]], // Pike
  // [{ type: ItemType.Polearm, flags: [ItemFlags.DropOnWeaponChange] }, {}, [ItemSlot.WeaponExtra]], // Pike
])('getAvailableSlotsByItem - item: %j, equipedItems: %j', (item, equipedItems, expectation) => {
  expect(getAvailableSlotsByItem(item as Item, equipedItems as Record<ItemSlot, UserItem>)).toEqual(
    expectation
  );
});

it.each([
  [ItemType.Bow, false],
  [ItemType.Polearm, true],
])('hasWeaponClassesByItemType - itemType: %s', (itemType, expectation) => {
  expect(hasWeaponClassesByItemType(itemType)).toEqual(expectation);
});

it.each<[ItemType, WeaponClass[]]>([
  [
    ItemType.OneHandedWeapon,
    [WeaponClass.OneHandedSword, WeaponClass.OneHandedAxe, WeaponClass.Mace, WeaponClass.Dagger],
  ],
  [ItemType.Bow, []],
])('getWeaponClassesByItemType - itemType: %s', (itemType, expectation) => {
  expect(getWeaponClassesByItemType(itemType)).toEqual(expectation);
});

describe('humanizeBucket', () => {
  it.each<[keyof ItemFlat, any, HumanBucket]>([
    ['type', null, { icon: null, label: 'type - invalid bucket' }],
    [
      'type',
      ItemType.OneHandedWeapon,
      {
        icon: { name: 'item-type-one-handed-weapon', type: IconBucketType.Svg },
        label: 'item.type.OneHandedWeapon',
      },
    ],
    [
      'weaponClass',
      WeaponClass.OneHandedPolearm,
      {
        icon: { name: 'weapon-class-one-handed-polearm', type: IconBucketType.Svg },
        label: 'item.weaponClass.OneHandedPolearm',
      },
    ],
    [
      'damageType',
      DamageType.Cut,
      {
        icon: { name: 'damage-type-cut', type: IconBucketType.Svg },
        label: 'item.damageType.Cut.long',
      },
    ],
    [
      'culture',
      Culture.Vlandia,
      {
        icon: { name: 'culture-vlandia', type: IconBucketType.Asset },
        label: 'item.culture.Vlandia',
      },
    ],
    [
      'mountArmorFamilyType',
      ItemFamilyType.Horse,
      {
        icon: { name: 'mount-type-horse', type: IconBucketType.Svg },
        label: 'item.familyType.1',
      },
    ],
    [
      'mountFamilyType',
      ItemFamilyType.Camel,
      {
        icon: { name: 'mount-type-camel', type: IconBucketType.Svg },
        label: 'item.familyType.2',
      },
    ],
    [
      'flags',
      ItemFlags.DropOnWeaponChange,
      {
        icon: { name: 'item-flag-drop-on-change', type: IconBucketType.Svg },
        label: 'item.flags.DropOnWeaponChange',
      },
    ],
    [
      'flags',
      WeaponFlags.CanDismount,
      {
        icon: { name: 'item-flag-can-dismount', type: IconBucketType.Svg },
        label: 'item.weaponFlags.CanDismount',
      },
    ],
    [
      'flags',
      ItemUsage.PolearmBracing,
      {
        icon: { name: 'item-flag-brace', type: IconBucketType.Svg },
        label: 'item.usage.polearm_bracing',
      },
    ],
    [
      'requirement',
      18,
      {
        icon: null,
        label: 'item.requirementFormat',
      },
    ],
    [
      'price',
      1234,
      {
        icon: null,
        label: '1234',
      },
    ],
    [
      'handling',
      12,
      {
        icon: null,
        label: '12',
      },
    ],
  ])('aggKey: %s, bucket: %s ', (aggKey, bucket, expectation) => {
    expect(humanizeBucket(aggKey, bucket)).toEqual(expectation);
  });

  it('damage', () => {
    const item: Partial<ItemFlat> = {
      swingDamageType: DamageType.Cut,
    };

    expect(humanizeBucket('swingDamage', 10, item as ItemFlat)).toEqual({
      icon: null,
      label: 'item.damageTypeFormat',
    });
  });

  it('damage', () => {
    const item: Partial<ItemFlat> = {};

    expect(humanizeBucket('thrustDamage', 0, item as ItemFlat)).toEqual({
      icon: null,
      label: '0',
    });
  });
});

it('compareItemsResult', () => {
  const items = [
    {
      weight: 2.22,
      length: 105,
      handling: 82,
      flags: ['UseTeamColor'],
    },
    {
      weight: 2.07,
      length: 99,
      handling: 86,
      flags: [],
    },
  ] as ItemFlat[];

  const aggregationConfig = {
    length: {
      title: 'Length',
      compareRule: 'Bigger',
    },
    flags: {
      title: 'Flags',
    },
    handling: {
      title: 'Handling',
      compareRule: 'Bigger',
    },
    weight: {
      title: 'Thrust damage',
      compareRule: 'Less',
    },
  } as AggregationConfig;

  expect(getCompareItemsResult(items, aggregationConfig)).toEqual({
    length: 105,
    handling: 86,
    weight: 2.07,
  });
});

it.each<[ItemFieldCompareRule, number, number, string]>([
  [ItemFieldCompareRule.Bigger, 0, 0, ''],
  [ItemFieldCompareRule.Bigger, -1, 0, '-1'],
  [ItemFieldCompareRule.Bigger, -1, -1, ''],

  [ItemFieldCompareRule.Bigger, 1, 1, ''],
  [ItemFieldCompareRule.Bigger, 2, 1, ''],
  [ItemFieldCompareRule.Bigger, 1, 2, '-1'],

  [ItemFieldCompareRule.Less, 0, 0, ''],
  [ItemFieldCompareRule.Less, 0, -1, '+1'],
  [ItemFieldCompareRule.Less, -1, -1, ''],

  [ItemFieldCompareRule.Less, 1, 1, ''],
  [ItemFieldCompareRule.Less, 1, 2, ''],
  [ItemFieldCompareRule.Less, 2, 1, '+1'],
])(
  'getItemFieldDiffStr - compareRule: %s, value: %s, bestValue: %s',
  (compareRule, value, bestValue, expectation) => {
    expect(getItemFieldDiffStr(compareRule, value, bestValue)).toEqual(expectation);
  }
);

it.todo('TODO: computeSalePrice', () => {});

it.todo('TODO: computeAverageRepairCostPerHour', () => {});

it.todo('TODO: computeBrokenItemRepairCost', () => {});
