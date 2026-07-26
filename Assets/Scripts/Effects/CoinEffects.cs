using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Raccoin.Effects
{
    // ===== 基础硬币效果 =====

    public class CE_Basic : CoinEffectBase { }

    public class CE_Gold : CoinEffectBase
    {
        [SerializeField] private float _valueMultiplier = 2f;
        protected override void OnSettleEffect()
        {
            // 金币结算时价值翻倍
        }
    }

    public class CE_Diamond : CoinEffectBase
    {
        [SerializeField] private float _valueMultiplier = 5f;
        protected override void OnSettleEffect() { }
    }

    public class CE_Ruby : CoinEffectBase
    {
        [SerializeField] private float _valueMultiplier = 3f;
        protected override void OnSettleEffect() { }
    }

    // ===== 动物系列 =====

    public class CE_Fox : CoinEffectBase
    {
        protected override void OnSpawnEffect() { /* 狐狸：偷取附近硬币价值 */ }
    }

    public class CE_Frog : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_FrogJump()); }
        private IEnumerator IE_FrogJump()
        {
            // 青蛙跳跃动画
            yield return new WaitForSeconds(0.5f);
        }
    }

    public class CE_Rabbit : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_RabbitJump()); }
        private IEnumerator IE_RabbitJump()
        {
            yield return new WaitForSeconds(0.3f);
        }
    }

    public class CE_Hen : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_HenFly()); }
        private IEnumerator IE_HenFly()
        {
            // 母鸡飞行下蛋
            yield return new WaitForSeconds(1f);
        }
        private IEnumerator IE_Attract()
        {
            yield return null;
        }
    }

    public class CE_Lion : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_Hunt()); }
        private IEnumerator IE_Hunt()
        {
            // 狮子捕猎其他硬币
            yield return null;
        }
    }

    public class CE_Tiger : CoinEffectBase { }
    public class CE_WolfBase : CoinEffectBase { }
    public class CE_Wolf : CE_WolfBase { }
    public class CE_Monkey : CoinEffectBase { }
    public class CE_Dog : CoinEffectBase { }
    public class CE_Pigeon : CoinEffectBase { }
    public class CE_Rat : CoinEffectBase { }
    public class CE_Rooster : CoinEffectBase
    {
        private IEnumerator IE_GoodMorning()
        {
            yield return null; // 公鸡打鸣
        }
    }

    // ===== 天文系列 =====

    public class CE_Sun : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    public class CE_Moon : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    public class CE_Mars : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    public class CE_Jupiter : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    public class CE_Saturn : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    public class CE_Neptune : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    public class CE_Uranus : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    public class CE_Venus : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
        private IEnumerator IE_VenusMove()
        {
            yield return null;
        }
    }

    public class CE_Pluto : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    public class CE_Earth : CoinEffectBase, ICoinAstro
    {
        public void AstroFly(Vector3 target) { }
    }

    // ===== 食物系列 =====

    public class CE_FindFood : CoinEffectBase
    {
        protected virtual IEnumerator IE_TryCook()
        {
            yield return null;
        }
    }

    public class CE_Rice : CE_FindFood { }
    public class CE_Sushi : CoinEffectBase { }
    public class CE_Pizza : CoinEffectBase { }
    public class CE_Onigiri : CoinEffectBase { }
    public class CE_Omurice : CE_FindFood { }
    public class CE_EggWaffle : CoinEffectBase { }
    public class CE_VeggieBurger : CE_FindFood { }
    public class CE_Dough : CE_FindFood { }
    public class CE_FiletOFish : CoinEffectBase { }
    public class CE_CloverPancake : CoinEffectBase { }
    public class CE_Chikuwa : CoinEffectBase { }
    public class CE_Risotto : CoinEffectBase { }
    public class CE_BeggarRice : CoinEffectBase { }
    public class CE_OyakoDon : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_Init()); }
        private IEnumerator IE_Init() { yield return null; }
    }
    public class CE_BeggarChicken : CoinEffectBase { }
    public class CE_MustardFish : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_Init()); }
        private IEnumerator IE_Init() { yield return null; }
    }
    public class CE_Salad : CoinEffectBase { }
    public class CE_Veggie : CE_FindFood { }
    public class CE_Nanakusa : CoinEffectBase { }

    // 糖类基类
    public class CE_SugarBase : CoinEffectBase { }
    public class CE_Jawbreaker : CE_SugarBase { }
    public class CE_CakeBase : CE_SugarBase { }
    public class CE_EightRice : CE_SugarBase { }
    public class CE_Hawthorn : CE_SugarBase { }

    // ===== 机械系列 =====

    public class CE_Gear : CoinEffectBase { }
    public class CE_Bolt : CoinEffectBase { }
    public class CE_Nut : CoinEffectBase { }
    public class CE_WindingKey : CoinEffectBase { }
    public class CE_DrillBit : CoinEffectBase
    {
        private IEnumerator IE_Drill()
        {
            yield return null; // 钻地效果
        }
    }
    public class CE_Clock : CoinEffectBase
    {
        private IEnumerator IE_ClockEvent()
        {
            yield return null; // 时钟事件
        }
    }

    // ===== 元素系列 =====

    public class CE_FireBall : CoinEffectBase { }
    public class CE_Ice : CoinEffectBase { }
    public class CE_Water : CE_Fertilizer, ICoinBeWater
    {
        public void BeWatered() { }
    }
    public class CE_Electricity : CoinEffectBase { }
    public class CE_Tornado : CoinEffectBase { }
    public class CE_Quake : CoinEffectBase { }
    public class CE_Meteor : CoinEffectBase { }
    public class CE_Aerolite : CoinEffectBase { }
    public class CE_Comet : CoinEffectBase { }

    // ===== 数学/特殊系列 =====

    public class CE_Multiply : CoinEffectBase
    {
        [SerializeField] private float _multiplier = 2f;
    }
    public class CE_Division : CoinEffectBase { }
    public class CE_Factorial : CoinEffectBase { }
    public class CE_Sum : CoinEffectBase
    {
        private IEnumerator IE_Try() { yield return null; }
    }
    public class CE_Percent : CoinEffectBase { }
    public class CE_Equal : CoinEffectBase { }
    public class CE_Add : CoinEffectBase { }
    public class CE_Zero : CoinEffectBase { }
    public class CE_infinity : CoinEffectBase { }

    // ===== RPG 系列 =====

    public class CE_RPGCoin : CoinEffectBase { }
    public class CE_RPG_Attack : CE_RPGCoin { }
    public class CE_RPG_Defend : CE_RPGCoin { }
    public class CE_RPG_Heal : CE_RPGCoin { }
    public class CE_RPG_Doom : CE_RPGCoin { }

    // ===== 外星系列 =====

    public class CE_BagAlien : CoinEffectBase { }
    public class CE_BallAlien : CoinEffectBase { }
    public class CE_SlotAlien : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(BornUpdateScale(1.5f)); }
    }

    // ===== 其他特殊效果 =====

    public class CE_Magnet : CoinEffectBase { }
    public class CE_Rocket : CoinEffectBase
    {
        private IEnumerator IE_RocketRun() { yield return null; }
    }
    public class CE_Giraffe : CoinEffectBase
    {
        private IEnumerator IE_InitSpin() { yield return null; }
        private IEnumerator IE_SpinNeck() { yield return null; }
        private IEnumerator IE_StartSpinNeck() { yield return null; }
    }
    public class CE_Killer : CoinEffectBase
    {
        private IEnumerator IE_Kill() { yield return null; }
    }
    public class CE_Ghost : CoinEffectBase
    {
        private IEnumerator IE_Ghost() { yield return null; }
    }
    public class CE_Bubble : CoinEffectBase
    {
        private IEnumerator BeforeBubbleEnd() { yield return null; }
    }
    public class CE_BubbleGum : CoinEffectBase { }
    public class CE_Glue : CoinEffectBase
    {
        private IEnumerator RecoverGlue() { yield return null; }
    }
    public class CE_Magic : CoinEffectBase { }
    public class CE_Clover : CoinEffectBase { }
    public class CE_Clown : CoinEffectBase { }
    public class CE_Bonus : CoinEffectBase { }
    public class CE_Combo : CoinEffectBase { }
    public class CE_ComboAdd : CoinEffectBase { }
    public class CE_Credit : CoinEffectBase { }
    public class CE_Lucky : CoinEffectBase { }
    public class CE_Rich : CoinEffectBase { }
    public class CE_Gamble : CoinEffectBase { }
    public class CE_Roulette : CoinEffectBase { }
    public class CE_Explosion : CoinEffectBase { }
    public class CE_TimeBomb : CoinEffectBase { }
    public class CE_DoomStar : CoinEffectBase { }
    public class CE_Bigger : CoinEffectBase { }
    public class CE_Smaller : CoinEffectBase { }
    public class CE_Half : CoinEffectBase { }
    public class CE_Bloody : CoinEffectBase { }
    public class CE_Soul : CoinEffectBase { }
    public class CE_Star : CoinEffectBase { }
    public class CE_Square : CoinEffectBase { }
    public class CE_Collapse : CoinEffectBase { }
    public class CE_Wormhole : CoinEffectBase { }
    public class CE_Lode : CoinEffectBase { }
    public class CE_Quartz : CoinEffectBase { }
    public class CE_Radiation : CoinEffectBase { }
    public class CE_Sand : CoinEffectBase { }
    public class CE_Snowman : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(BornUpdateScale(2f, 0.5f)); }
    }
    public class CE_Slime : CoinEffectBase { }
    public class CE_SleepWalk : CoinEffectBase { }
    public class CE_Speaker : CoinEffectBase { }
    public class CE_Stomach : CoinEffectBase { }
    public class CE_Thief : CoinEffectBase { }
    public class CE_Ticket : CoinEffectBase { }
    public class CE_CollectTicket : CoinEffectBase { }
    public class CE_RedPocket : CoinEffectBase { }
    public class CE_WishWell : CoinEffectBase { }
    public class CE_Pinwheel : CoinEffectBase { }
    public class CE_Paint : CoinEffectBase { }
    public class CE_LotusLeaf : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_Init()); }
        private IEnumerator IE_Init() { yield return null; }
    }
    public class CE_ElectricEye : CoinEffectBase
    {
        private IEnumerator IE_EyeLaser() { yield return null; }
    }
    public class CE_CoinAlien : CoinEffectBase { }
    public class CE_CoinEatingFlower : CoinEffectBase { }
    public class CE_Flower : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_Init()); }
        private IEnumerator IE_Init() { yield return null; }
    }
    public class CE_Fox2 : CoinEffectBase { }
    public class CE_Fridge : CoinEffectBase { }
    public class CE_Friend : CoinEffectBase { }
    public class CE_GingerCat : CoinEffectBase { }
    public class CE_Corn : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_Init()); }
        private IEnumerator IE_Init() { yield return null; }
    }
    public class CE_Banana : CoinEffectBase { }
    public class CE_Bean : CoinEffectBase { }
    public class CE_Battery : CoinEffectBase { }
    public class CE_BallDoctor : CoinEffectBase
    {
        private IEnumerator IE_ResearchQueue() { yield return null; }
    }
    public class CE_BlindBox : CoinEffectBase { }
    public class CE_Cheat : CoinEffectBase { }
    public class CE_Chocolate : CoinEffectBase { }
    public class CE_Clay : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(BornUpdateScale(1.2f)); }
        private IEnumerator IE_BeAttract() { yield return null; }
        private IEnumerator IE_Init() { yield return null; }
    }
    public class CE_Employee : CoinEffectBase { }
    public class CE_EmployeeBad : CoinEffectBase { }
    public class CE_Egg : CoinEffectBase { }
    public class CE_Fish : CoinEffectBase { }
    public class CE_Primal : CoinEffectBase { }
    public class CE_PowderBox : CoinEffectBase { }
    public class CE_Ruffian : CoinEffectBase { }
    public class CE_Saw : CoinEffectBase { }
    public class CE_Seed : CoinEffectBase
    {
        protected override void OnSpawnEffect() { StartCoroutine(IE_Init()); }
        private IEnumerator IE_Init() { yield return null; }
    }
    public class CE_Fertilizer : CoinEffectBase
    {
        protected virtual IEnumerator IE_BeAttract() { yield return null; }
    }
    public class CE_Shit : CE_Fertilizer { }
    public class CE_Ancient : CoinEffectBase { }
    public class CE_Ally : CoinEffectBase { }
    public class CE_Cockroach : CoinEffectBase
    {
        private IEnumerator IE_TryMove() { yield return null; }
    }

    // 金钱系列
    public class CE_Money : CoinEffectBase { }
    public class CE_Five : CE_Money { }
    public class CE_Ten : CE_Money { }
    public class CE_Hundred : CE_Money { }

    // 工业系列
    public class CE_BottleCap : CoinEffectBase { }
    public class CE_Bulb : CoinEffectBase { }
    public class CE_Candle : CoinEffectBase { }
    public class CE_Chimera : CoinEffectBase { }
    public class CE_Lamp : CoinEffectBase { }
    public class CE_Jeton : CoinEffectBase { }

    // ===== 效果修改器 =====

    public class CEMod_AstroFly : CoinEffectMod
    {
        private IEnumerator IE_AstroFly() { yield return null; }
    }

    public class CEMod_AttractCoin : CoinEffectMod, ICoinBeAttract
    {
        public void BeAttracted(Vector3 direction, float force) { }
    }

    public class CEMod_BubbleExpand : CoinEffectMod
    {
        private IEnumerator FinalExpand() { yield return null; }
    }

    public class CEMod_CookFood : CoinEffectMod
    {
        private IEnumerator IE_HuntToCook() { yield return null; }
    }

    public class CEMod_HuntBall : CoinEffectMod
    {
        private IEnumerator IE_Hunt() { yield return null; }
    }

    public class CEMod_HuntFood : CoinEffectMod
    {
        private IEnumerator IE_Hunt() { yield return null; }
    }

    public class CEMod_WaterPlant : CoinEffectMod
    {
        private IEnumerator IE_WaterPlant() { yield return null; }
    }
}
