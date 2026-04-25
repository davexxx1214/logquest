const SAVE_KEY = "logquest.prototype.v1";
const TEAM_LIMIT = 3;
const INITIAL_UNLOCKED_REGIONS = ["meadow"];
const INITIAL_UNLOCKED_CLASSES = ["warrior", "monk"];
const INITIAL_FLOOR_CAP = 3;

const classData = {
  warrior: {
    name: "战士",
    mark: "W",
    spriteIndex: 0,
    unlock: "初始",
    role: "稳定前排 / 斩击",
    colorA: "#a94838",
    colorB: "#5c2723",
    base: { hp: 132, atk: 18, mag: 2, def: 12, spd: 8, crit: 6 },
    growth: { hp: 18, atk: 4, mag: 1, def: 3, spd: 1, crit: 1 }
  },
  mage: {
    name: "法师",
    mark: "M",
    spriteIndex: 1,
    unlock: "沉没神殿",
    role: "奥术爆发 / 群体伤害",
    colorA: "#584fb1",
    colorB: "#251c62",
    base: { hp: 78, atk: 6, mag: 27, def: 5, spd: 11, crit: 8 },
    growth: { hp: 10, atk: 1, mag: 6, def: 1, spd: 2, crit: 1 }
  },
  monk: {
    name: "僧侣",
    mark: "C",
    spriteIndex: 2,
    unlock: "初始",
    role: "治疗 / 圣光惩戒",
    colorA: "#d8c36a",
    colorB: "#8a6a2c",
    base: { hp: 96, atk: 9, mag: 19, def: 8, spd: 9, crit: 5 },
    growth: { hp: 13, atk: 2, mag: 4, def: 2, spd: 1, crit: 1 }
  },
  hunter: {
    name: "猎人",
    mark: "H",
    spriteIndex: 3,
    unlock: "晨曦草原",
    role: "先手 / 暴击 / 点杀",
    colorA: "#4ca36c",
    colorB: "#214a2b",
    base: { hp: 92, atk: 21, mag: 4, def: 6, spd: 17, crit: 18 },
    growth: { hp: 12, atk: 5, mag: 1, def: 1, spd: 3, crit: 2 }
  },
  knight: {
    name: "骑士",
    mark: "K",
    spriteIndex: 4,
    unlock: "旧矿山",
    role: "护卫 / 减伤 / 反击",
    colorA: "#4e9bcb",
    colorB: "#24445d",
    base: { hp: 146, atk: 15, mag: 3, def: 16, spd: 6, crit: 4 },
    growth: { hp: 20, atk: 3, mag: 1, def: 4, spd: 1, crit: 1 }
  },
  druid: {
    name: "德鲁伊",
    mark: "D",
    spriteIndex: 5,
    unlock: "黑森林",
    role: "持续恢复 / 毒藤 / 自然之力",
    colorA: "#6eb65f",
    colorB: "#46672b",
    base: { hp: 106, atk: 11, mag: 17, def: 9, spd: 10, crit: 7 },
    growth: { hp: 15, atk: 2, mag: 4, def: 2, spd: 2, crit: 1 }
  }
};

const classSkills = {
  warrior: [
    { id: "guarded_stance", level: 1, name: "坚守架势", text: "受到伤害降低 8%" },
    { id: "sweeping_strike", level: 3, name: "横扫斩", text: "每 4 轮强化斩击" },
    { id: "battle_trance", level: 6, name: "战意", text: "半血后攻击提升" }
  ],
  mage: [
    { id: "arcane_focus", level: 1, name: "奥术专注", text: "魔法伤害更稳定" },
    { id: "ember_rain", level: 3, name: "火星雨", text: "每 3 轮群体攻击" },
    { id: "mana_surge", level: 6, name: "魔涌", text: "暴击后追加法术力" }
  ],
  monk: [
    { id: "minor_heal", level: 1, name: "小治疗术", text: "周期治疗最低生命队友" },
    { id: "ward_prayer", level: 3, name: "护佑祷言", text: "治疗时附加护盾" },
    { id: "turn_undead", level: 6, name: "驱邪", text: "对亡灵额外伤害" }
  ],
  hunter: [
    { id: "opening_shot", level: 1, name: "先制射击", text: "第 1 轮必定强化攻击" },
    { id: "marked_prey", level: 3, name: "猎物标记", text: "暴击伤害提升" },
    { id: "rapid_fire", level: 6, name: "连射", text: "有概率追加一击" }
  ],
  knight: [
    { id: "bodyguard", level: 1, name: "护卫", text: "优先承受敌方攻击" },
    { id: "counter", level: 3, name: "反击", text: "受击后有概率反击" },
    { id: "aegis_aura", level: 6, name: "庇护光环", text: "全队获得少量减伤" }
  ],
  druid: [
    { id: "thorn_vine", level: 1, name: "毒藤", text: "周期自然伤害" },
    { id: "wild_regrowth", level: 3, name: "野性再生", text: "毒藤后全队恢复" },
    { id: "beast_shape", level: 6, name: "兽形", text: "低血量时提升生存" }
  ]
};

const regions = [
  {
    id: "meadow",
    name: "晨曦草原",
    tone: "低威胁",
    description: "边境村落外的风车草原。野兽与盗贼会在清晨巡游，适合第一批队伍测试自动战斗。",
    unlocks: ["第 3 层: 猎人职业", "第 5 层: 旧矿山路线"],
    rewards: [
      { floor: 3, classes: ["hunter"], message: "猎人加入了营地。" },
      { floor: 5, regions: ["mine"], message: "通往旧矿山的路标被重新立起。" }
    ],
    scene: ["#8fd3d2", "#f0ce78", "#61a05f", "#356d42", "#7cb84f", "#3f6f38"],
    icon: ["#a5d66d", "#4c853b"],
    dungeons: [
      {
        id: "beast-trail",
        name: "野兽小径",
        floors: 8,
        levelRange: [1, 8],
        monsterIds: ["slime", "wolf", "bandit", "hornet"],
        lootTable: ["粗糙短剑", "兽皮护腕", "旅人斗篷", "蜂刺戒指"]
      }
    ]
  },
  {
    id: "mine",
    name: "旧矿山",
    tone: "中威胁",
    description: "废弃矿道仍有火把在闪。矿工传说里，塌方深处埋着可强化前排职业的矿铁。",
    unlocks: ["第 4 层: 骑士职业", "第 7 层: 沉没神殿线索"],
    rewards: [
      { floor: 4, classes: ["knight"], message: "一名骑士接受了远征雇佣。" },
      { floor: 7, regions: ["temple"], message: "矿石碑文指向了沉没神殿。" }
    ],
    scene: ["#55636b", "#2d3642", "#826039", "#4d3229", "#6c573f", "#36281f"],
    icon: ["#82705b", "#3d3030"],
    dungeons: [
      {
        id: "fallen-shaft",
        name: "塌陷矿洞",
        floors: 10,
        levelRange: [4, 14],
        monsterIds: ["bat", "goblin", "scarab", "troll"],
        lootTable: ["矿工斧", "铁鳞甲", "硬壳盾", "幽暗灯芯"]
      }
    ]
  },
  {
    id: "temple",
    name: "沉没神殿",
    tone: "高威胁",
    description: "半截石阶沉在潮水里，神殿回廊会放大治疗与诅咒的价值。这里是僧侣定位的第一块试金石。",
    unlocks: ["第 5 层: 法师职业", "第 8 层: 黑森林入口"],
    rewards: [
      { floor: 5, classes: ["mage"], message: "神殿残卷唤来了法师学派的协助。" },
      { floor: 8, regions: ["forest"], message: "潮水退去，黑森林的古道显露出来。" }
    ],
    scene: ["#436c7f", "#223544", "#537c73", "#2d5551", "#43515e", "#1d2630"],
    icon: ["#629bb0", "#24505c"],
    dungeons: [
      {
        id: "sunken-cloister",
        name: "潮湿回廊",
        floors: 12,
        levelRange: [8, 22],
        monsterIds: ["skeleton", "mire", "cultist", "gargoyle"],
        lootTable: ["裂纹圣印", "潮汐法杖", "骨片护符", "石像肩甲"]
      }
    ]
  },
  {
    id: "forest",
    name: "黑森林",
    tone: "精英威胁",
    description: "树冠遮住星光，腐殖土里藏着古老根须。持续恢复和自然抗性在这里更有价值。",
    unlocks: ["第 5 层: 德鲁伊职业", "第 8 层: 魔法塔坐标"],
    rewards: [
      { floor: 5, classes: ["druid"], message: "德鲁伊从林间营火旁加入队伍。" },
      { floor: 8, regions: ["tower"], message: "林中石环指出了魔法塔的位置。" }
    ],
    scene: ["#2e5a48", "#172c27", "#456b32", "#233e29", "#34492b", "#182219"],
    icon: ["#4f8f52", "#1c3a28"],
    dungeons: [
      {
        id: "root-hollow",
        name: "根须空洞",
        floors: 12,
        levelRange: [14, 28],
        monsterIds: ["bramble", "wisp", "dryad", "boar"],
        lootTable: ["荆棘短矛", "苔纹披肩", "月露护符", "古木指环"]
      }
    ]
  },
  {
    id: "tower",
    name: "魔法塔",
    tone: "奥术威胁",
    description: "塔内楼梯会自行转向，书页与镜面都可能先于敌人发动攻击。高魔力装备开始变得关键。",
    unlocks: ["第 6 层: 边境要塞情报", "第 10 层: 奥术武具"],
    rewards: [
      { floor: 6, regions: ["fortress"], message: "塔顶星图标出了边境要塞的补给线。" }
    ],
    scene: ["#51456f", "#241f39", "#7b6c9b", "#41375e", "#56516a", "#242431"],
    icon: ["#7e74bb", "#312656"],
    dungeons: [
      {
        id: "spiral-archive",
        name: "螺旋书库",
        floors: 14,
        levelRange: [20, 36],
        monsterIds: ["apprentice", "livingbook", "manawyrm", "mirror"],
        lootTable: ["星屑魔杖", "抄写员长袍", "棱镜戒指", "符文书签"]
      }
    ]
  },
  {
    id: "fortress",
    name: "边境要塞",
    tone: "首领威胁",
    description: "旧王国的军械仍在城墙上运转。这里用来测试中期队伍的承伤、续航和爆发窗口。",
    unlocks: ["第 8 层: 中期远征完成", "第 12 层: 高阶装备"],
    rewards: [
      { floor: 8, message: "边境要塞的烽火重新点亮，中期远征线完成。" }
    ],
    scene: ["#756b63", "#35313a", "#8b6d55", "#4b3d36", "#6d635a", "#28252a"],
    icon: ["#9a8272", "#4c4244"],
    dungeons: [
      {
        id: "bastion-keep",
        name: "断墙内堡",
        floors: 16,
        levelRange: [28, 48],
        monsterIds: ["deserter", "warhound", "ballista", "captain"],
        lootTable: ["边军长剑", "壁垒胸甲", "军旗护符", "校尉戒指"]
      }
    ]
  }
];

const monsters = {
  slime: { name: "绿泥史莱姆", mark: "S", spriteIndex: 0, family: "软体", colorA: "#67c45d", colorB: "#2f6c32", base: { hp: 42, atk: 8, mag: 0, def: 2, spd: 5, crit: 2 } },
  wolf: { name: "草原野狼", mark: "W", spriteIndex: 1, family: "野兽", colorA: "#9a8c79", colorB: "#4b413b", base: { hp: 54, atk: 12, mag: 0, def: 3, spd: 13, crit: 8 } },
  bandit: { name: "流窜盗贼", mark: "B", spriteIndex: 2, family: "人型", colorA: "#b65b3c", colorB: "#5c2a24", base: { hp: 62, atk: 13, mag: 1, def: 5, spd: 9, crit: 10 } },
  hornet: { name: "针刺巨蜂", mark: "H", spriteIndex: 3, family: "虫群", colorA: "#d8b947", colorB: "#654720", base: { hp: 45, atk: 15, mag: 0, def: 2, spd: 16, crit: 12 } },
  bat: { name: "矿洞蝙蝠", mark: "B", spriteIndex: 4, family: "野兽", colorA: "#6d6480", colorB: "#292334", base: { hp: 70, atk: 15, mag: 0, def: 4, spd: 17, crit: 8 } },
  goblin: { name: "哥布林矿工", mark: "G", spriteIndex: 5, family: "人型", colorA: "#7bad4a", colorB: "#3f5127", base: { hp: 88, atk: 18, mag: 0, def: 8, spd: 8, crit: 6 } },
  scarab: { name: "石壳虫", mark: "C", spriteIndex: 6, family: "虫群", colorA: "#8f7956", colorB: "#4a3b2b", base: { hp: 104, atk: 14, mag: 0, def: 14, spd: 5, crit: 3 } },
  troll: { name: "矿洞巨魔", mark: "T", spriteIndex: 7, family: "巨人", colorA: "#9a7250", colorB: "#563a2c", base: { hp: 150, atk: 24, mag: 0, def: 10, spd: 4, crit: 5 } },
  skeleton: { name: "骷髅侍从", mark: "S", spriteIndex: 8, family: "亡灵", colorA: "#d8d0ad", colorB: "#6d6752", base: { hp: 110, atk: 21, mag: 2, def: 10, spd: 9, crit: 7 } },
  mire: { name: "淤泥怪", mark: "M", spriteIndex: 9, family: "软体", colorA: "#456b5d", colorB: "#22392f", base: { hp: 142, atk: 18, mag: 8, def: 12, spd: 4, crit: 2 } },
  cultist: { name: "诅咒祭司", mark: "P", spriteIndex: 10, family: "人型", colorA: "#8d506c", colorB: "#3f2034", base: { hp: 118, atk: 12, mag: 27, def: 8, spd: 10, crit: 8 } },
  gargoyle: { name: "石像守卫", mark: "G", spriteIndex: 11, family: "构装", colorA: "#849096", colorB: "#3d4549", base: { hp: 176, atk: 24, mag: 4, def: 18, spd: 5, crit: 4 } },
  bramble: { name: "荆棘根须", mark: "R", spriteIndex: 12, family: "植物", colorA: "#486f38", colorB: "#25351e", base: { hp: 165, atk: 22, mag: 12, def: 15, spd: 5, crit: 3 } },
  wisp: { name: "林间鬼火", mark: "F", spriteIndex: 13, family: "精魂", colorA: "#7ed6a4", colorB: "#2b5c55", base: { hp: 116, atk: 10, mag: 32, def: 8, spd: 16, crit: 10 } },
  dryad: { name: "枯叶树妖", mark: "Y", spriteIndex: 14, family: "精魂", colorA: "#8fa35d", colorB: "#455231", base: { hp: 152, atk: 18, mag: 24, def: 13, spd: 11, crit: 8 } },
  boar: { name: "铁鬃野猪", mark: "O", spriteIndex: 15, family: "野兽", colorA: "#8b6a55", colorB: "#432f2a", base: { hp: 210, atk: 31, mag: 0, def: 16, spd: 9, crit: 7 } },
  apprentice: { name: "失控学徒", mark: "A", spriteIndex: 16, family: "人型", colorA: "#7261b9", colorB: "#33265f", base: { hp: 156, atk: 14, mag: 38, def: 12, spd: 13, crit: 10 } },
  livingbook: { name: "活化典籍", mark: "L", spriteIndex: 17, family: "构装", colorA: "#b89058", colorB: "#5c3d2a", base: { hp: 170, atk: 18, mag: 30, def: 18, spd: 8, crit: 4 } },
  manawyrm: { name: "法力幼龙", mark: "Y", spriteIndex: 18, family: "龙类", colorA: "#7ec0d8", colorB: "#2b5162", base: { hp: 190, atk: 26, mag: 30, def: 15, spd: 15, crit: 12 } },
  mirror: { name: "回声魔镜", mark: "E", spriteIndex: 19, family: "构装", colorA: "#9da6cf", colorB: "#41476f", base: { hp: 215, atk: 18, mag: 35, def: 20, spd: 7, crit: 6 } },
  deserter: { name: "要塞逃兵", mark: "D", spriteIndex: 20, family: "人型", colorA: "#98705f", colorB: "#4c3634", base: { hp: 215, atk: 34, mag: 4, def: 22, spd: 11, crit: 9 } },
  warhound: { name: "披甲战犬", mark: "H", spriteIndex: 21, family: "野兽", colorA: "#6d6a65", colorB: "#303033", base: { hp: 180, atk: 37, mag: 0, def: 18, spd: 18, crit: 14 } },
  ballista: { name: "自律弩机", mark: "X", spriteIndex: 22, family: "构装", colorA: "#9a8062", colorB: "#4c3b2d", base: { hp: 240, atk: 42, mag: 0, def: 26, spd: 5, crit: 8 } },
  captain: { name: "亡国校尉", mark: "C", spriteIndex: 23, family: "亡灵", colorA: "#8c91a0", colorB: "#3b3d4c", base: { hp: 270, atk: 38, mag: 18, def: 28, spd: 10, crit: 11 } }
};

const rarity = [
  { name: "普通", className: "loot-rarity-common", chance: 0.58, power: 1 },
  { name: "优秀", className: "loot-rarity-uncommon", chance: 0.28, power: 1.35 },
  { name: "稀有", className: "loot-rarity-rare", chance: 0.11, power: 1.85 },
  { name: "史诗", className: "loot-rarity-epic", chance: 0.03, power: 2.6 }
];

const slotNames = {
  weapon: "武器",
  armor: "防具",
  trinket: "饰品"
};

const itemCatalog = {
  "粗糙短剑": { slot: "weapon", stats: ["atk"], icon: "SW" },
  "兽皮护腕": { slot: "armor", stats: ["def", "hp"], icon: "GL" },
  "旅人斗篷": { slot: "armor", stats: ["spd", "def"], icon: "CP" },
  "蜂刺戒指": { slot: "trinket", stats: ["crit", "spd"], icon: "RG" },
  "矿工斧": { slot: "weapon", stats: ["atk", "crit"], icon: "AX" },
  "铁鳞甲": { slot: "armor", stats: ["def", "hp"], icon: "AR" },
  "硬壳盾": { slot: "armor", stats: ["def"], icon: "SH" },
  "幽暗灯芯": { slot: "trinket", stats: ["mag", "crit"], icon: "LT" },
  "裂纹圣印": { slot: "trinket", stats: ["mag", "def"], icon: "SG" },
  "潮汐法杖": { slot: "weapon", stats: ["mag"], icon: "ST" },
  "骨片护符": { slot: "trinket", stats: ["hp", "mag"], icon: "CH" },
  "石像肩甲": { slot: "armor", stats: ["def", "hp"], icon: "PA" },
  "荆棘短矛": { slot: "weapon", stats: ["atk", "mag"], icon: "SP" },
  "苔纹披肩": { slot: "armor", stats: ["def", "mag"], icon: "MC" },
  "月露护符": { slot: "trinket", stats: ["hp", "mag"], icon: "MD" },
  "古木指环": { slot: "trinket", stats: ["def", "crit"], icon: "OR" },
  "星屑魔杖": { slot: "weapon", stats: ["mag", "crit"], icon: "MW" },
  "抄写员长袍": { slot: "armor", stats: ["mag", "def"], icon: "SR" },
  "棱镜戒指": { slot: "trinket", stats: ["mag", "spd"], icon: "PR" },
  "符文书签": { slot: "trinket", stats: ["crit", "mag"], icon: "RB" },
  "边军长剑": { slot: "weapon", stats: ["atk", "def"], icon: "LS" },
  "壁垒胸甲": { slot: "armor", stats: ["hp", "def"], icon: "BA" },
  "军旗护符": { slot: "trinket", stats: ["hp", "atk"], icon: "BF" },
  "校尉戒指": { slot: "trinket", stats: ["atk", "crit"], icon: "CR" }
};

const affixPool = [
  { name: "锋利", stats: { atk: 1 }, weight: 1.25 },
  { name: "秘纹", stats: { mag: 1 }, weight: 1.25 },
  { name: "坚韧", stats: { hp: 6, def: 1 }, weight: 1.05 },
  { name: "迅捷", stats: { spd: 1, crit: 1 }, weight: 1 },
  { name: "守护", stats: { def: 2 }, weight: 0.95 },
  { name: "鹰眼", stats: { crit: 2 }, weight: 0.9 }
];

let state = loadState();
let selectedRegionId = state.selectedRegionId || regions[0].id;
let selectedDungeonId = state.selectedDungeonId || regions[0].dungeons[0].id;
let selectedFloor = state.selectedFloor || 1;
let tickTimer = null;

const els = {
  saveStatus: document.querySelector("#saveStatus"),
  resetBtn: document.querySelector("#resetBtn"),
  regionCount: document.querySelector("#regionCount"),
  mapList: document.querySelector("#mapList"),
  selectedRegionName: document.querySelector("#selectedRegionName"),
  selectedRegionText: document.querySelector("#selectedRegionText"),
  eventList: document.querySelector("#eventList"),
  dungeonSelect: document.querySelector("#dungeonSelect"),
  floorSelect: document.querySelector("#floorSelect"),
  floorMeterFill: document.querySelector("#floorMeterFill"),
  dangerBadge: document.querySelector("#dangerBadge"),
  startBtn: document.querySelector("#startBtn"),
  quickBtn: document.querySelector("#quickBtn"),
  activeRun: document.querySelector("#activeRun"),
  activeRunTitle: document.querySelector("#activeRunTitle"),
  timeLeft: document.querySelector("#timeLeft"),
  runProgress: document.querySelector("#runProgress"),
  heroList: document.querySelector("#heroList"),
  teamCount: document.querySelector("#teamCount"),
  monsterList: document.querySelector("#monsterList"),
  monsterLevelBand: document.querySelector("#monsterLevelBand"),
  combatLog: document.querySelector("#combatLog"),
  resultBadge: document.querySelector("#resultBadge"),
  lootList: document.querySelector("#lootList"),
  regionScene: document.querySelector("#regionScene")
};

function createInitialHeroes() {
  return Object.keys(classData).map((id) => ({
    id,
    level: INITIAL_UNLOCKED_CLASSES.includes(id) ? 2 : 1,
    xp: 0,
    unlocked: INITIAL_UNLOCKED_CLASSES.includes(id),
    equipment: createEmptyEquipment()
  }));
}

function createInitialProgression() {
  const floors = {};
  regions.forEach((region) => {
    region.dungeons.forEach((dungeon) => {
      floors[dungeon.id] = INITIAL_UNLOCKED_REGIONS.includes(region.id) ? Math.min(INITIAL_FLOOR_CAP, dungeon.floors) : 0;
    });
  });
  return {
    unlockedRegions: [...INITIAL_UNLOCKED_REGIONS],
    unlockedClasses: [...INITIAL_UNLOCKED_CLASSES],
    floors,
    completedFloors: {}
  };
}

function loadState() {
  const defaults = {
    heroes: createInitialHeroes(),
    selectedHeroIds: ["warrior", "monk"],
    progression: createInitialProgression(),
    inventory: [],
    history: [],
    activeRun: null
  };

  try {
    const stored = JSON.parse(localStorage.getItem(SAVE_KEY));
    if (!stored) return defaults;
    const merged = {
      ...defaults,
      ...stored,
      progression: mergeProgression(stored.progression, stored),
      heroes: mergeHeroes(stored.heroes || defaults.heroes),
      inventory: Array.isArray(stored.inventory) ? stored.inventory.slice(0, 60) : [],
      selectedHeroIds: (stored.selectedHeroIds || defaults.selectedHeroIds).slice(0, TEAM_LIMIT)
    };
    merged.heroes.forEach((hero) => {
      hero.unlocked = merged.progression.unlockedClasses.includes(hero.id);
    });
    merged.selectedHeroIds = merged.selectedHeroIds.filter((id) => merged.progression.unlockedClasses.includes(id));
    if (!merged.selectedHeroIds.length) {
      merged.selectedHeroIds = merged.heroes.filter((hero) => hero.unlocked).slice(0, TEAM_LIMIT).map((hero) => hero.id);
    }
    return merged;
  } catch {
    return defaults;
  }
}

function mergeHeroes(savedHeroes) {
  const byId = new Map(savedHeroes.map((hero) => [hero.id, hero]));
  return createInitialHeroes().map((hero) => {
    const saved = byId.get(hero.id) || {};
    return {
      ...hero,
      ...saved,
      equipment: { ...createEmptyEquipment(), ...(saved.equipment || {}) }
    };
  });
}

function mergeProgression(savedProgression, stored) {
  const base = createInitialProgression();
  if (!savedProgression) {
    return deriveLegacyProgression(stored, base);
  }

  const progression = {
    unlockedRegions: unique([...base.unlockedRegions, ...(savedProgression.unlockedRegions || [])]),
    unlockedClasses: unique([...base.unlockedClasses, ...(savedProgression.unlockedClasses || [])]),
    floors: { ...base.floors, ...(savedProgression.floors || {}) },
    completedFloors: { ...(savedProgression.completedFloors || {}) }
  };
  normalizeProgression(progression);
  return progression;
}

function deriveLegacyProgression(stored, base) {
  const progression = {
    unlockedRegions: [...base.unlockedRegions],
    unlockedClasses: unique([
      ...base.unlockedClasses,
      ...((stored?.heroes || []).filter((hero) => hero.unlocked).map((hero) => hero.id))
    ]),
    floors: { ...base.floors },
    completedFloors: {}
  };
  (stored?.history || []).forEach((run) => {
    if (!run.success) return;
    progression.unlockedRegions.push(run.regionId);
    progression.completedFloors[run.dungeonId] = Math.max(progression.completedFloors[run.dungeonId] || 0, run.floor || 0);
  });
  normalizeProgression(progression);
  return progression;
}

function normalizeProgression(progression) {
  progression.unlockedRegions = unique(progression.unlockedRegions).filter((id) => regions.some((region) => region.id === id));
  progression.unlockedClasses = unique(progression.unlockedClasses).filter((id) => classData[id]);
  regions.forEach((region) => {
    region.dungeons.forEach((dungeon) => {
      if (!(dungeon.id in progression.floors)) progression.floors[dungeon.id] = progression.unlockedRegions.includes(region.id) ? 1 : 0;
      if (!progression.unlockedRegions.includes(region.id)) progression.floors[dungeon.id] = 0;
      progression.floors[dungeon.id] = Math.max(0, Math.min(dungeon.floors, progression.floors[dungeon.id]));
    });
  });
}

function createEmptyEquipment() {
  return { weapon: null, armor: null, trinket: null };
}

function saveState(message = "本地存档已更新") {
  state.selectedRegionId = selectedRegionId;
  state.selectedDungeonId = selectedDungeonId;
  state.selectedFloor = selectedFloor;
  localStorage.setItem(SAVE_KEY, JSON.stringify(state));
  els.saveStatus.textContent = message;
}

function getRegion() {
  return regions.find((region) => region.id === selectedRegionId) || regions[0];
}

function getDungeon(region = getRegion()) {
  return region.dungeons.find((dungeon) => dungeon.id === selectedDungeonId) || region.dungeons[0];
}

function isRegionUnlocked(regionId) {
  return state.progression.unlockedRegions.includes(regionId);
}

function isClassUnlocked(classId) {
  return state.progression.unlockedClasses.includes(classId);
}

function getFloorCap(dungeon) {
  return Math.max(0, Math.min(dungeon.floors, state.progression.floors[dungeon.id] || 0));
}

function getCompletedFloor(dungeonId) {
  return state.progression.completedFloors[dungeonId] || 0;
}

function ensureSelection() {
  const unlockedRegion = regions.find((region) => isRegionUnlocked(region.id)) || regions[0];
  if (!isRegionUnlocked(selectedRegionId)) {
    selectedRegionId = unlockedRegion.id;
    selectedDungeonId = unlockedRegion.dungeons[0].id;
  }

  const region = getRegion();
  if (!region.dungeons.some((dungeon) => dungeon.id === selectedDungeonId)) {
    selectedDungeonId = region.dungeons[0].id;
  }

  const dungeon = getDungeon(region);
  const cap = getFloorCap(dungeon);
  selectedFloor = Math.max(1, Math.min(selectedFloor || 1, Math.max(1, cap)));
  if (cap === 0 && region.id !== unlockedRegion.id) {
    selectedRegionId = unlockedRegion.id;
    selectedDungeonId = unlockedRegion.dungeons[0].id;
    selectedFloor = 1;
  }

  state.selectedHeroIds = state.selectedHeroIds.filter((id) => isClassUnlocked(id)).slice(0, TEAM_LIMIT);
  if (!state.selectedHeroIds.length) {
    state.selectedHeroIds = state.heroes.filter((hero) => hero.unlocked).slice(0, TEAM_LIMIT).map((hero) => hero.id);
  }
}

function getHero(id) {
  return state.heroes.find((hero) => hero.id === id);
}

function statBlock(classId, level) {
  const data = classData[classId];
  const stats = {};
  for (const [key, value] of Object.entries(data.base)) {
    stats[key] = value + data.growth[key] * (level - 1);
  }
  return stats;
}

function heroStats(hero) {
  const stats = statBlock(hero.id, hero.level);
  getEquippedItems(hero).forEach((item) => {
    for (const [stat, value] of Object.entries(item.stats || {})) {
      stats[stat] = (stats[stat] || 0) + value;
    }
  });
  return stats;
}

function getEquippedItems(hero) {
  return Object.values(hero.equipment || {}).filter(Boolean);
}

function getUnlockedSkills(classId, level) {
  return (classSkills[classId] || []).filter((skill) => level >= skill.level);
}

function hasSkill(unit, skillId) {
  return unit.skills?.some((skill) => skill.id === skillId);
}

function monsterStats(monsterId, level) {
  const template = monsters[monsterId];
  const scale = 1 + (level - 1) * 0.14;
  return {
    hp: Math.round(template.base.hp * scale),
    atk: Math.round(template.base.atk * scale),
    mag: Math.round(template.base.mag * scale),
    def: Math.round(template.base.def * scale),
    spd: Math.round(template.base.spd + level * 0.45),
    crit: Math.round(template.base.crit + level * 0.25)
  };
}

function xpForLevel(level) {
  return 70 + level * level * 55;
}

function render() {
  ensureSelection();
  renderMap();
  renderMission();
  renderHeroes();
  renderMonsters();
  renderLogs();
  renderLoot();
  updateActiveRun();
}

function renderMap() {
  els.regionCount.textContent = `${state.progression.unlockedRegions.length} / ${regions.length}`;
  els.mapList.innerHTML = "";
  regions.forEach((region) => {
    const dungeon = region.dungeons[0];
    const unlocked = isRegionUnlocked(region.id);
    const cap = getFloorCap(dungeon);
    const completed = getCompletedFloor(dungeon.id);
    const button = document.createElement("button");
    button.type = "button";
    button.disabled = !unlocked;
    button.className = `map-tile ${region.id === selectedRegionId ? "selected" : ""} ${unlocked ? "" : "locked"}`;
    button.innerHTML = `
      <span class="tile-icon" style="--accent-a:${region.icon[0]};--accent-b:${region.icon[1]}"></span>
      <span>
        <strong>${region.name}</strong>
        <span class="small-line">${unlocked ? `${region.tone} · 已通 ${completed}/${dungeon.floors} · 可至 ${cap}` : "未发现路线"}</span>
      </span>
    `;
    button.addEventListener("click", () => {
      if (!unlocked) return;
      selectedRegionId = region.id;
      selectedDungeonId = region.dungeons[0].id;
      selectedFloor = Math.max(1, Math.min(getFloorCap(region.dungeons[0]), region.dungeons[0].floors));
      saveState("已切换地点");
      render();
    });
    els.mapList.append(button);
  });
}

function renderMission() {
  const region = getRegion();
  const dungeon = getDungeon(region);
  const cap = getFloorCap(dungeon);
  selectedFloor = Math.max(1, Math.min(selectedFloor, Math.max(1, cap)));
  const [minLevel, maxLevel] = floorLevelBand(dungeon, selectedFloor);
  const completed = getCompletedFloor(dungeon.id);

  els.selectedRegionName.textContent = region.name;
  els.selectedRegionText.textContent = `${region.description} 当前已通关 ${completed}/${dungeon.floors} 层，可挑战至第 ${cap} 层。`;
  els.eventList.innerHTML = region.unlocks.map((item) => `<span class="event-pill">${item}</span>`).join("");
  els.regionScene.style.setProperty("--scene-sky-a", region.scene[0]);
  els.regionScene.style.setProperty("--scene-sky-b", region.scene[1]);
  els.regionScene.style.setProperty("--scene-mid-a", region.scene[2]);
  els.regionScene.style.setProperty("--scene-mid-b", region.scene[3]);
  els.regionScene.style.setProperty("--scene-ground-a", region.scene[4]);
  els.regionScene.style.setProperty("--scene-ground-b", region.scene[5]);

  els.dungeonSelect.innerHTML = region.dungeons
    .map((item) => `<option value="${item.id}" ${item.id === selectedDungeonId ? "selected" : ""}>${item.name}</option>`)
    .join("");

  els.floorSelect.innerHTML = Array.from({ length: dungeon.floors }, (_, index) => {
    const floor = index + 1;
    const locked = floor > cap;
    return `<option value="${floor}" ${floor === selectedFloor ? "selected" : ""} ${locked ? "disabled" : ""}>第 ${floor} 层${locked ? "（未探索）" : ""}</option>`;
  }).join("");

  els.floorMeterFill.style.width = `${Math.round((selectedFloor / dungeon.floors) * 100)}%`;
  els.dangerBadge.textContent = `Lv.${minLevel}-${maxLevel} · ${completed}/${dungeon.floors}`;
  els.startBtn.disabled = Boolean(state.activeRun) || state.selectedHeroIds.length === 0 || cap === 0;
}

function renderHeroes() {
  els.teamCount.textContent = `${state.selectedHeroIds.length} / ${TEAM_LIMIT}`;
  els.heroList.innerHTML = "";
  state.heroes.forEach((hero) => {
    const data = classData[hero.id];
    const stats = heroStats(hero);
    const selected = state.selectedHeroIds.includes(hero.id);
    const nextXp = xpForLevel(hero.level);
    const skills = hero.unlocked ? getUnlockedSkills(hero.id, hero.level) : [];
    const gearPower = getEquippedItems(hero).reduce((sum, item) => sum + scoreItem(item), 0);
    const gearLine = Object.entries(hero.equipment || {})
      .map(([slot, item]) => `${slotNames[slot]}:${item ? item.name : "空"}`)
      .join(" / ");
    const button = document.createElement("button");
    button.type = "button";
    button.disabled = !hero.unlocked;
    button.className = `hero-tile ${selected ? "selected" : ""} ${hero.unlocked ? "" : "locked"}`;
    button.style.setProperty("--sprite-a", data.colorA);
    button.style.setProperty("--sprite-b", data.colorB);
    button.innerHTML = `
      <span class="hero-portrait">${data.mark}</span>
      <span>
        <strong>${data.name} Lv.${hero.level}</strong>
        <span class="small-line">${hero.unlocked ? `${data.role} · XP ${hero.xp}/${nextXp} · 装等 ${gearPower}` : `未解锁 · 来源: ${data.unlock}`}</span>
        <span class="hero-statbar">
          <span>HP ${stats.hp}</span><span>ATK ${stats.atk}</span><span>MAG ${stats.mag}</span><span>DEF ${stats.def}</span><span>SPD ${stats.spd}</span><span>CRT ${stats.crit}</span>
        </span>
        <span class="skill-row">
          ${skills.map((skill) => `<span title="${skill.text}">${skill.name}</span>`).join("")}
        </span>
        <span class="gear-row">${gearLine}</span>
      </span>
    `;
    button.addEventListener("click", () => toggleHero(hero.id));
    els.heroList.append(button);
  });
}

function renderMonsters() {
  const dungeon = getDungeon();
  const [minLevel, maxLevel] = floorLevelBand(dungeon, selectedFloor);
  els.monsterLevelBand.textContent = `Lv.${minLevel}-${maxLevel}`;
  els.monsterList.innerHTML = "";
  dungeon.monsterIds.forEach((monsterId) => {
    const monster = monsters[monsterId];
    const stats = monsterStats(monsterId, Math.round((minLevel + maxLevel) / 2));
    const row = document.createElement("div");
    row.className = "monster-tile";
    row.style.setProperty("--sprite-a", monster.colorA);
    row.style.setProperty("--sprite-b", monster.colorB);
    row.innerHTML = `
      <span class="monster-sprite">${monster.mark}</span>
      <span>
        <strong>${monster.name}</strong>
        <span class="small-line">${monster.family} · HP ${stats.hp} · ATK ${stats.atk} · DEF ${stats.def}</span>
      </span>
    `;
    els.monsterList.append(row);
  });
}

function renderLogs() {
  const visibleLogs = getVisibleLogs();
  if (!visibleLogs.length) {
    els.combatLog.innerHTML = `<p class="log-entry">选择地点、层数和最多 3 名英雄，然后派遣队伍。战斗会自动进行，日志只读。</p>`;
    els.resultBadge.textContent = state.activeRun ? "进行中" : "等待出击";
    return;
  }

  els.combatLog.innerHTML = visibleLogs
    .map((entry) => `<p class="log-entry ${entry.type || ""}">${entry.text}</p>`)
    .join("");
  els.combatLog.scrollTop = els.combatLog.scrollHeight;

  if (state.activeRun) {
    els.resultBadge.textContent = "进行中";
  } else if (state.history[0]) {
    els.resultBadge.textContent = state.history[0].success ? "挑战成功" : "挑战失败";
  }
}

function renderLoot() {
  const equipped = state.heroes.flatMap((hero) => getEquippedItems(hero));
  const inventory = state.inventory || [];
  const inventoryHtml = `
    <div class="inventory-summary">
      <span>已装备 ${equipped.length} 件</span>
      <span>库存 ${inventory.length} 件</span>
    </div>
    ${
      inventory.length
        ? inventory.slice(0, 5).map((item) => renderItemRow(item, "库存")).join("")
        : `<div class="loot-row"><span>库存为空</span><span class="small-line">更强装备会自动穿戴</span></div>`
    }
  `;

  if (!state.history.length) {
    els.lootList.innerHTML = `${inventoryHtml}<div class="loot-row"><span>尚无结算记录</span><span class="small-line">首次出击后显示</span></div>`;
    return;
  }

  const historyHtml = state.history
    .slice(0, 8)
    .map((run) => {
      const loot = run.loot?.length
        ? run.loot.map((item) => `<span class="${item.className}">${item.rarity} ${item.name}</span>`).join(" / ")
        : "未获得装备";
      return `
        <div class="loot-row">
          <span>
            <strong>${run.regionName} · ${run.dungeonName} ${run.floor} 层</strong>
            <span class="small-line">${run.success ? "成功" : "失败"} · XP ${run.xp} · ${loot}</span>
          </span>
          <span class="small-line">${run.finishedAt}</span>
        </div>
      `;
    })
    .join("");

  els.lootList.innerHTML = `${inventoryHtml}<div class="subhead">最近结算</div>${historyHtml}`;
}

function renderItemRow(item, label) {
  return `
    <div class="item-row">
      <span class="item-icon">${item.icon || "??"}</span>
      <span>
        <strong class="${item.className || ""}">${item.rarity || "普通"} ${item.name}</strong>
        <span class="small-line">${label} · ${slotNames[item.slot] || "装备"} · ${formatStats(item.stats)}</span>
      </span>
    </div>
  `;
}

function toggleHero(heroId) {
  if (state.activeRun) return;
  if (!isClassUnlocked(heroId)) return;
  const selected = state.selectedHeroIds.includes(heroId);
  if (selected) {
    state.selectedHeroIds = state.selectedHeroIds.filter((id) => id !== heroId);
  } else if (state.selectedHeroIds.length < TEAM_LIMIT) {
    state.selectedHeroIds = [...state.selectedHeroIds, heroId];
  }
  saveState("队伍已更新");
  render();
}

function floorLevelBand(dungeon, floor) {
  const [min, max] = dungeon.levelRange;
  const span = max - min;
  const floorRatio = dungeon.floors === 1 ? 0 : (floor - 1) / (dungeon.floors - 1);
  const center = Math.round(min + span * floorRatio);
  return [Math.max(1, center - 1), center + 1];
}

function getDurationMs(floor) {
  return (25 + floor * 16) * 1000;
}

function startRun() {
  if (state.activeRun || state.selectedHeroIds.length === 0) return;
  const region = getRegion();
  const dungeon = getDungeon(region);
  if (!isRegionUnlocked(region.id) || selectedFloor > getFloorCap(dungeon)) return;
  const simulation = simulateBattle(region, dungeon, selectedFloor, state.selectedHeroIds);
  const startedAt = Date.now();
  const durationMs = getDurationMs(selectedFloor);

  state.activeRun = {
    id: crypto.randomUUID ? crypto.randomUUID() : String(startedAt),
    regionId: region.id,
    regionName: region.name,
    dungeonId: dungeon.id,
    dungeonName: dungeon.name,
    floor: selectedFloor,
    heroIds: [...state.selectedHeroIds],
    startedAt,
    endAt: startedAt + durationMs,
    durationMs,
    simulation
  };

  saveState("队伍已经出发");
  render();
}

function simulateBattle(region, dungeon, floor, heroIds) {
  const logs = [{ text: `队伍进入 ${region.name} / ${dungeon.name} 第 ${floor} 层。`, type: "" }];
  const team = heroIds.map((id) => {
    const hero = getHero(id);
    const data = classData[id];
    const stats = heroStats(hero);
    const skills = getUnlockedSkills(id, hero.level);
    return { side: "hero", id, name: data.name, classId: id, maxHp: stats.hp, hp: stats.hp, stats, skills, cooldown: 0 };
  });
  const enemyTeam = createEncounter(dungeon, floor);
  enemyTeam.forEach((enemy) => logs.push({ text: `遭遇 ${enemy.name} Lv.${enemy.level}。`, type: "danger" }));

  let round = 1;
  while (round <= 36 && alive(team).length && alive(enemyTeam).length) {
    logs.push({ text: `-- 第 ${round} 轮 --` });
    const actors = [...alive(team), ...alive(enemyTeam)].sort((a, b) => b.stats.spd - a.stats.spd);
    for (const actor of actors) {
      if (actor.hp <= 0 || !alive(team).length || !alive(enemyTeam).length) continue;
      if (actor.side === "hero") {
        heroAction(actor, team, enemyTeam, round, logs);
      } else {
        monsterAction(actor, team, logs);
      }
    }
    round += 1;
  }

  const success = alive(team).length > 0 && alive(enemyTeam).length === 0;
  if (success) {
    logs.push({ text: "迷宫层主被击退，远征完成。", type: "success" });
  } else {
    logs.push({ text: "队伍被迫撤退，挑战失败。", type: "danger" });
  }

  const xp = Math.round((success ? 34 : 14) * floor + enemyTeam.reduce((sum, enemy) => sum + enemy.level * 5, 0));
  const loot = success ? rollLoot(dungeon, floor) : [];
  loot.forEach((item) => logs.push({ text: `发现掉落：${item.rarity} ${item.name}（${slotNames[item.slot]} · ${formatStats(item.stats)}）`, type: "loot" }));

  return { success, xp, loot, logs, rounds: round - 1 };
}

function createEncounter(dungeon, floor) {
  const [minLevel, maxLevel] = floorLevelBand(dungeon, floor);
  const count = Math.min(4, 2 + Math.floor((floor + 1) / 4));
  const result = [];
  for (let index = 0; index < count; index += 1) {
    const monsterId = dungeon.monsterIds[(floor + index + Math.floor(Math.random() * dungeon.monsterIds.length)) % dungeon.monsterIds.length];
    const level = rand(minLevel, maxLevel);
    const stats = monsterStats(monsterId, level);
    const template = monsters[monsterId];
    result.push({
      side: "monster",
      id: `${monsterId}-${index}`,
      monsterId,
      name: template.name,
      level,
      maxHp: stats.hp,
      hp: stats.hp,
      stats
    });
  }
  return result;
}

function heroAction(hero, team, enemyTeam, round, logs) {
  const enemies = alive(enemyTeam);
  const target = lowestHp(enemies);
  if (!target) return;

  if (hasSkill(hero, "minor_heal") && (round % 3 === 0 || lowestHp(team).hp / lowestHp(team).maxHp < 0.45)) {
    const ally = lowestHp(team);
    const heal = Math.round(hero.stats.mag * 1.55 + hero.stats.atk * 0.45);
    ally.hp = Math.min(ally.maxHp, ally.hp + heal);
    if (hasSkill(hero, "ward_prayer")) {
      ally.guard = (ally.guard || 0) + Math.round(hero.stats.mag * 0.55);
    }
    logs.push({ text: `${hero.name} 施放小治疗术，为 ${ally.name} 恢复 ${heal} 生命。`, type: "success" });
    return;
  }

  if (hasSkill(hero, "thorn_vine") && round % 3 === 1) {
    const damage = Math.round(hero.stats.mag * 1.2 + hero.stats.atk * 0.7);
    const finalDamage = applyDamage(damage, target.stats.def * 0.7);
    target.hp -= finalDamage;
    logs.push({ text: `${hero.name} 召出毒藤缠住 ${target.name}，造成 ${finalDamage} 伤害。` });
    finishIfDead(target, logs);
    if (hasSkill(hero, "wild_regrowth")) {
      alive(team).forEach((ally) => {
        ally.hp = Math.min(ally.maxHp, ally.hp + Math.round(hero.stats.mag * 0.28));
      });
      logs.push({ text: `${hero.name} 的野性再生让队伍缓慢恢复。`, type: "success" });
    }
    return;
  }

  if (hasSkill(hero, "ember_rain") && round % 3 === 0) {
    const damage = Math.round(hero.stats.mag * 1.15 + hero.stats.atk);
    alive(enemyTeam).forEach((enemy) => {
      const finalDamage = applyDamage(damage, enemy.stats.def * 0.45);
      enemy.hp -= finalDamage;
      logs.push({ text: `${hero.name} 释放火星雨击中 ${enemy.name}，造成 ${finalDamage} 伤害。` });
      finishIfDead(enemy, logs);
    });
    return;
  }

  const baseDamage = hero.stats.atk + Math.round(hero.stats.mag * 0.65);
  const crit = Math.random() * 100 < hero.stats.crit;
  const classBonus =
    hasSkill(hero, "opening_shot") && round === 1 ? 1.45 :
    hero.classId === "hunter" ? 1.22 :
    hasSkill(hero, "sweeping_strike") && round % 4 === 0 ? 1.35 :
    hero.classId === "knight" ? 0.92 : 1;
  const critBonus = hasSkill(hero, "marked_prey") ? 1.95 : 1.7;
  const finalDamage = applyDamage(baseDamage * classBonus * (crit ? critBonus : 1), target.stats.def);
  target.hp -= finalDamage;
  logs.push({ text: `${hero.name}${crit ? " 暴击" : ""} 攻击 ${target.name}，造成 ${finalDamage} 伤害。` });
  finishIfDead(target, logs);

  if (hasSkill(hero, "rapid_fire") && target.hp > 0 && Math.random() < 0.22) {
    const rapidDamage = applyDamage(baseDamage * 0.58, target.stats.def);
    target.hp -= rapidDamage;
    logs.push({ text: `${hero.name} 触发连射，追加 ${rapidDamage} 伤害。` });
    finishIfDead(target, logs);
  }
}

function monsterAction(monster, team, logs) {
  const targets = alive(team);
  if (!targets.length) return;
  const protectedTarget = targets.find((hero) => hasSkill(hero, "bodyguard") && hero.hp > 0);
  const target = protectedTarget || targets[rand(0, targets.length - 1)];
  const crit = Math.random() * 100 < monster.stats.crit;
  const teamReduction = targets.some((hero) => hasSkill(hero, "aegis_aura")) ? 0.94 : 1;
  const targetReduction =
    hasSkill(target, "guarded_stance") ? 0.92 :
    hasSkill(target, "beast_shape") && target.hp / target.maxHp < 0.4 ? 0.88 : 1;
  let finalDamage = applyDamage((monster.stats.atk + monster.stats.mag * 0.7) * (crit ? 1.55 : 1) * teamReduction * targetReduction, target.stats.def);
  if (target.guard) {
    const absorbed = Math.min(target.guard, finalDamage);
    target.guard -= absorbed;
    finalDamage -= absorbed;
  }
  target.hp -= finalDamage;
  logs.push({ text: `${monster.name}${crit ? " 凶狠一击" : ""} 命中 ${target.name}，造成 ${finalDamage} 伤害。`, type: finalDamage > 30 ? "danger" : "" });
  finishIfDead(target, logs);

  if (target.hp > 0 && hasSkill(target, "counter") && Math.random() < 0.28) {
    const counterDamage = applyDamage(target.stats.atk * 0.7, monster.stats.def);
    monster.hp -= counterDamage;
    logs.push({ text: `${target.name} 反击 ${monster.name}，造成 ${counterDamage} 伤害。`, type: "success" });
    finishIfDead(monster, logs);
  }
}

function applyDamage(power, defense) {
  const variance = 0.88 + Math.random() * 0.24;
  return Math.max(1, Math.round((power * variance) - defense * 0.58));
}

function finishIfDead(unit, logs) {
  if (unit.hp <= 0) {
    unit.hp = 0;
    logs.push({ text: `${unit.name} 倒下。`, type: unit.side === "hero" ? "danger" : "success" });
  }
}

function alive(units) {
  return units.filter((unit) => unit.hp > 0);
}

function lowestHp(units) {
  return units.reduce((lowest, unit) => {
    if (!lowest) return unit;
    return unit.hp / unit.maxHp < lowest.hp / lowest.maxHp ? unit : lowest;
  }, null);
}

function rollLoot(dungeon, floor) {
  const drops = [];
  const attempts = Math.random() < 0.35 + floor * 0.035 ? 1 : 0;
  for (let index = 0; index < attempts; index += 1) {
    const rarityRoll = Math.random();
    let cumulative = 0;
    const picked = rarity.find((item) => {
      cumulative += item.chance;
      return rarityRoll <= cumulative;
    }) || rarity[0];
    drops.push(createEquipmentItem(dungeon.lootTable[rand(0, dungeon.lootTable.length - 1)], picked, floor));
  }
  return drops;
}

function createEquipmentItem(baseName, pickedRarity, floor) {
  const base = itemCatalog[baseName] || { slot: "trinket", stats: ["hp"], icon: "??" };
  const itemLevel = Math.max(1, Math.round(floor * pickedRarity.power));
  const stats = {};
  base.stats.forEach((stat) => {
    addStat(stats, stat, statRoll(stat, itemLevel, pickedRarity.power));
  });

  const affixCount = pickedRarity.name === "史诗" ? 2 : pickedRarity.name === "稀有" ? 1 : Math.random() < 0.36 ? 1 : 0;
  const affixes = [];
  for (let index = 0; index < affixCount; index += 1) {
    const affix = affixPool[rand(0, affixPool.length - 1)];
    affixes.push(affix.name);
    for (const [stat, value] of Object.entries(affix.stats)) {
      addStat(stats, stat, statRoll(stat, itemLevel, affix.weight) * value);
    }
  }

  const prefix = affixes.length ? `${affixes.join("")}的` : "";
  return {
    id: crypto.randomUUID ? crypto.randomUUID() : `${Date.now()}-${Math.random()}`,
    baseName,
    name: `${prefix}${baseName} +${Math.max(1, Math.round(itemLevel * 0.45))}`,
    icon: base.icon,
    slot: base.slot,
    level: itemLevel,
    rarity: pickedRarity.name,
    className: pickedRarity.className,
    stats,
    affixes
  };
}

function statRoll(stat, level, weight) {
  const value = Math.max(1, Math.round((level + 1) * weight));
  if (stat === "hp") return value * 5;
  if (stat === "crit") return Math.max(1, Math.round(value * 0.75));
  return value;
}

function addStat(stats, stat, value) {
  stats[stat] = (stats[stat] || 0) + value;
}

function formatStats(stats) {
  const labels = { hp: "HP", atk: "ATK", mag: "MAG", def: "DEF", spd: "SPD", crit: "CRT" };
  return Object.entries(stats || {})
    .map(([stat, value]) => `${labels[stat] || stat}+${value}`)
    .join(" ");
}

function scoreItem(item) {
  return Object.entries(item?.stats || {}).reduce((sum, [stat, value]) => {
    const weights = { hp: 0.22, atk: 1.2, mag: 1.2, def: 1.05, spd: 1.1, crit: 0.75 };
    return sum + Math.round(value * (weights[stat] || 1));
  }, 0);
}

function itemValueForHero(item, hero) {
  const classWeights = {
    warrior: { hp: 0.28, atk: 1.35, mag: 0.35, def: 1.15, spd: 0.8, crit: 0.7 },
    mage: { hp: 0.18, atk: 0.45, mag: 1.65, def: 0.8, spd: 0.9, crit: 0.9 },
    monk: { hp: 0.22, atk: 0.65, mag: 1.35, def: 1.05, spd: 0.85, crit: 0.55 },
    hunter: { hp: 0.18, atk: 1.45, mag: 0.35, def: 0.7, spd: 1.35, crit: 1.35 },
    knight: { hp: 0.32, atk: 1, mag: 0.25, def: 1.55, spd: 0.55, crit: 0.45 },
    druid: { hp: 0.24, atk: 0.8, mag: 1.25, def: 0.95, spd: 0.9, crit: 0.65 }
  };
  const weights = classWeights[hero.id] || {};
  return Object.entries(item.stats || {}).reduce((sum, [stat, value]) => sum + value * (weights[stat] || 1), 0);
}

function autoEquipLoot(heroIds, loot) {
  const changes = [];
  loot.forEach((drop) => {
    const item = normalizeItem(drop);
    let best = null;
    heroIds.forEach((heroId) => {
      const hero = getHero(heroId);
      const current = hero.equipment?.[item.slot];
      const gain = itemValueForHero(item, hero) - (current ? itemValueForHero(current, hero) : 0);
      if (!best || gain > best.gain) {
        best = { hero, current, gain };
      }
    });

    if (best && best.gain > 0) {
      if (best.current) {
        state.inventory = [{ ...best.current, ownerId: null, ownerName: null }, ...state.inventory].slice(0, 60);
      }
      best.hero.equipment[item.slot] = { ...item, ownerId: best.hero.id, ownerName: classData[best.hero.id].name };
      changes.push(`${classData[best.hero.id].name} 装备了 ${item.rarity} ${item.name}`);
    } else {
      state.inventory = [{ ...item, ownerId: null, ownerName: null }, ...state.inventory].slice(0, 60);
      changes.push(`${item.rarity} ${item.name} 已收入库存`);
    }
  });
  return changes;
}

function normalizeItem(item) {
  if (item?.slot && item?.stats) return item;
  const baseName = Object.keys(itemCatalog).find((name) => item?.name?.includes(name)) || "粗糙短剑";
  const picked = rarity.find((entry) => entry.name === item?.rarity) || rarity[0];
  return createEquipmentItem(baseName, picked, item?.level || 1);
}

function updateActiveRun() {
  if (!state.activeRun) {
    els.activeRun.hidden = true;
    els.startBtn.disabled = state.selectedHeroIds.length === 0;
    return;
  }

  const run = state.activeRun;
  const now = Date.now();
  if (now >= run.endAt) {
    finishRun();
    return;
  }

  const elapsed = now - run.startedAt;
  const remaining = run.endAt - now;
  const progress = Math.min(100, Math.round((elapsed / run.durationMs) * 100));
  els.activeRun.hidden = false;
  els.activeRunTitle.textContent = `${run.regionName} / ${run.dungeonName} 第 ${run.floor} 层`;
  els.timeLeft.textContent = formatTime(remaining);
  els.runProgress.style.width = `${progress}%`;
  els.startBtn.disabled = true;
  renderLogs();
}

function applyProgressRewards(run) {
  if (!run.success) return [];
  const region = regions.find((item) => item.id === run.regionId);
  const dungeon = region?.dungeons.find((item) => item.id === run.dungeonId);
  if (!region || !dungeon) return [];

  const messages = [];
  const previousCompleted = getCompletedFloor(dungeon.id);
  const newCompleted = Math.max(previousCompleted, run.floor);
  state.progression.completedFloors[dungeon.id] = newCompleted;
  const nextCap = Math.min(dungeon.floors, Math.max(getFloorCap(dungeon), run.floor + 2));
  if (nextCap > getFloorCap(dungeon)) {
    state.progression.floors[dungeon.id] = nextCap;
    messages.push(`${dungeon.name} 可挑战层数提升至第 ${nextCap} 层。`);
  }

  (region.rewards || []).forEach((reward) => {
    if (previousCompleted >= reward.floor || newCompleted < reward.floor) return;
    (reward.classes || []).forEach((classId) => {
      if (!state.progression.unlockedClasses.includes(classId)) {
        state.progression.unlockedClasses.push(classId);
        const hero = getHero(classId);
        if (hero) hero.unlocked = true;
        messages.push(`${classData[classId].name} 已解锁。`);
      }
    });
    (reward.regions || []).forEach((regionId) => {
      if (!state.progression.unlockedRegions.includes(regionId)) {
        state.progression.unlockedRegions.push(regionId);
        const unlockedRegion = regions.find((item) => item.id === regionId);
        unlockedRegion?.dungeons.forEach((item) => {
          state.progression.floors[item.id] = Math.max(state.progression.floors[item.id] || 0, Math.min(INITIAL_FLOOR_CAP, item.floors));
        });
        messages.push(`${unlockedRegion?.name || regionId} 已在大地图上解锁。`);
      }
    });
    if (reward.message) messages.push(reward.message);
  });

  normalizeProgression(state.progression);
  return unique(messages);
}

function getPendingRewards(region, completedFloor) {
  return (region.rewards || []).filter((reward) => reward.floor > completedFloor);
}

function finishRun() {
  const run = state.activeRun;
  if (!run) return;
  const finishedAt = new Date().toLocaleTimeString("zh-CN", { hour: "2-digit", minute: "2-digit" });
  const finalRun = {
    ...run,
    finishedAt,
    success: run.simulation.success,
    xp: run.simulation.xp,
    loot: run.simulation.loot
  };

  if (run.simulation.success) {
    awardXp(run.heroIds, run.simulation.xp);
  } else {
    awardXp(run.heroIds, Math.round(run.simulation.xp * 0.45));
  }

  const equipChanges = run.simulation.success ? autoEquipLoot(run.heroIds, run.simulation.loot) : [];
  const progressChanges = applyProgressRewards(finalRun);
  progressChanges.forEach((text) => run.simulation.logs.push({ text, type: "success" }));
  equipChanges.forEach((text) => run.simulation.logs.push({ text, type: "loot" }));
  finalRun.equipChanges = equipChanges;
  finalRun.progressChanges = progressChanges;
  state.history = [finalRun, ...state.history].slice(0, 24);
  state.activeRun = null;
  saveState("远征已结算");
  render();
}

function awardXp(heroIds, xp) {
  heroIds.forEach((id) => {
    const hero = getHero(id);
    hero.xp += xp;
    while (hero.xp >= xpForLevel(hero.level)) {
      hero.xp -= xpForLevel(hero.level);
      hero.level += 1;
    }
  });
}

function getVisibleLogs() {
  if (state.activeRun) {
    const { simulation, startedAt, durationMs } = state.activeRun;
    const elapsedRatio = Math.max(0.08, Math.min(1, (Date.now() - startedAt) / durationMs));
    const visibleCount = Math.max(1, Math.ceil(simulation.logs.length * elapsedRatio));
    return simulation.logs.slice(0, visibleCount);
  }

  return state.history[0]?.simulation?.logs || [];
}

function runQuickSimulation() {
  const region = getRegion();
  const dungeon = getDungeon(region);
  const total = 100;
  let wins = 0;
  let xp = 0;
  let lootCount = 0;
  for (let index = 0; index < total; index += 1) {
    const result = simulateBattle(region, dungeon, selectedFloor, state.selectedHeroIds);
    if (result.success) wins += 1;
    xp += result.xp;
    lootCount += result.loot.length;
  }
  els.combatLog.innerHTML = `
    <p class="log-entry loot">快速模拟：${region.name} / ${dungeon.name} 第 ${selectedFloor} 层</p>
    <p class="log-entry">样本：${total} 场</p>
    <p class="log-entry ${wins >= 55 ? "success" : "danger"}">胜率：${wins}%</p>
    <p class="log-entry">平均经验：${Math.round(xp / total)}</p>
    <p class="log-entry">平均掉落：${(lootCount / total).toFixed(2)} 件/场</p>
  `;
  els.resultBadge.textContent = "强度测试";
}

function formatTime(ms) {
  const totalSeconds = Math.max(0, Math.ceil(ms / 1000));
  const minutes = String(Math.floor(totalSeconds / 60)).padStart(2, "0");
  const seconds = String(totalSeconds % 60).padStart(2, "0");
  return `${minutes}:${seconds}`;
}

function rand(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function unique(values) {
  return [...new Set(values.filter(Boolean))];
}

els.dungeonSelect.addEventListener("change", (event) => {
  selectedDungeonId = event.target.value;
  selectedFloor = 1;
  saveState("已切换迷宫");
  render();
});

els.floorSelect.addEventListener("change", (event) => {
  selectedFloor = Number(event.target.value);
  saveState("已切换层数");
  render();
});

els.startBtn.addEventListener("click", startRun);
els.quickBtn.addEventListener("click", runQuickSimulation);
els.resetBtn.addEventListener("click", () => {
  if (!confirm("确认重置本地原型存档？")) return;
  localStorage.removeItem(SAVE_KEY);
  state = loadState();
  selectedRegionId = regions[0].id;
  selectedDungeonId = regions[0].dungeons[0].id;
  selectedFloor = 1;
  saveState("存档已重置");
  render();
});

render();
tickTimer = setInterval(updateActiveRun, 1000);
window.addEventListener("beforeunload", () => clearInterval(tickTimer));
